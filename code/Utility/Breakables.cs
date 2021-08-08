using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandbox
{
	/// <summary>
	/// Handle breaking a prop into bits
	/// </summary>
	public static class Breakables
	{
		// TODO - Limit max gibs
		// TODO - Fade old gibs
		// TODO - Debris collision group

		public static void Break( Entity ent, Result result = null )
		{
			if ( ent is ModelEntity modelEnt )
			{
				if ( result != null )
				{
					result.Source = ent;
				}

				Break( modelEnt.GetModel(), modelEnt.WorldPos, modelEnt.WorldRot, result, modelEnt.PhysicsBody );
			}
		}

		public static void Break( Model model, Vector3 pos, Rotation rot, Result result = null, PhysicsBody sourcePhysics = null )
		{
			if ( model == null || model.IsError )
				return;

			var breakList = model.GetBreakPieces();
			if ( breakList == null || breakList.Length <= 0 ) return;

			foreach ( var piece in breakList )
			{
				var mdl = Model.Load( piece.Model );
				var offset = mdl.GetAttachment( "placementOrigin" );

				var gib = new Prop
				{
					WorldPos = pos + rot * (piece.Offset - offset.Pos),
					WorldRot = rot,
					CollisionGroup = piece.GetCollisionGroup(),
				};

				gib.SetModel( mdl );

				var phys = gib.PhysicsBody;
				if ( phys != null )
				{
					// Apply the velocity at the parent object's position
					if ( sourcePhysics != null )
					{
						phys.Velocity = sourcePhysics.GetVelocityAtPoint( phys.Pos );
						phys.AngularVelocity = sourcePhysics.AngularVelocity;
					}
				}

				if ( piece.FadeTime > 0 )
				{
					_ = FadeAsync( gib, piece.FadeTime );
				}

				result?.AddProp( gib );
			}
		}

		static async Task FadeAsync( Prop gib, float fadeTime )
		{
			fadeTime += Rand.Float( -1, 1 );

			if ( fadeTime < 0.5f )
				fadeTime = 0.5f;

			await gib.Task.DelaySeconds( fadeTime );

			var fadePerFrame = 5 / 255.0f;

			while ( gib.RenderAlpha > 0 )
			{
				gib.RenderAlpha -= fadePerFrame;
				await gib.Task.Delay( 20 );
			}

			gib.Delete();
		}

		public class Result
		{
			public Entity Source;
			public List<ModelEntity> Props = new List<ModelEntity>();

			public void AddProp( ModelEntity prop )
			{
				Props.Add( prop );
			}
		}

	}
}
