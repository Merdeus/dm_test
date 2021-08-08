using System;

namespace Sandbox
{
	public class DevCamera : BaseCamera
	{
		Angles LookAngles;
		Vector3 MoveInput;

		Vector3 TargetPos;
		Rotation TargetRot;

		bool PivotEnabled;
		Vector3 PivotPos;
		float PivotDist;

		float MoveSpeed;
		float FovOverride = 0;


		// TODO - saved client convar
		public static bool Overlays = true;

		/// <summary>
		/// On the camera becoming activated, snap to the current view position
		/// </summary>
		public override void Activated()
		{
			base.Activated();

			TargetPos = Camera.LastPos;
			TargetRot = Camera.LastRot;

			Pos = TargetPos;
			Rot = TargetRot;
			LookAngles = Rot.Angles();
			FovOverride = 80;

			DoFPoint = 0.0f;
			DoFBlurSize = 20.0f;
			

			if ( Overlays )
			{
				var pos = new Vector3( 100, 100 );
				var duration = 4.0f;

				DebugOverlay.ScreenText( pos, 0, Color.White, "  .                        ", duration );
				DebugOverlay.ScreenText( pos, 1, Color.White, ",-| ,-. .  , ,-. ,-. ,-,-. ", duration );
				DebugOverlay.ScreenText( pos, 2, Color.White, "| | |-' | /  |   ,-| | | | ", duration );
				DebugOverlay.ScreenText( pos, 3, Color.White, "`-^ `-' `'   `-' `-^ ' ' ' ", duration );

				DebugOverlay.ScreenText( pos, 5, Color.White, "  Free:  WSAD, Run/Crouch for speed", duration );
				DebugOverlay.ScreenText( pos, 6, Color.White, " Pivot:  Hold Walk (Default Alt) - forward + back to zoom in/out", duration );
				DebugOverlay.ScreenText( pos, 8, Color.White, "Reload:  Toggle Overlays", duration );
			}

			//
			// Set the devcamera class on the HUD. It's up to the HUD what it does with it.
			//
			Hud.Current?.RootPanel?.SetClass( "devcamera", true );
		}

		public override void Deactivated()
		{
			base.Deactivated();

			Hud.Current?.RootPanel?.SetClass( "devcamera", false );
		}

		public override void Update()
		{
			var player = Player.Local;
			if ( player == null ) return;

			var tr = Trace.Ray( Pos, Pos + Rot.Forward * 4096 ).UseHitboxes().Run();

			// DebugOverlay.Box( tr.EndPos, Vector3.One * -1, Vector3.One, Color.Red );

			FieldOfView = FovOverride;

			Viewer = null;
			{
				var lerpTarget = tr.EndPos.Distance( Pos );

				DoFPoint = lerpTarget;// DoFPoint.LerpTo( lerpTarget, Time.Delta * 10 );
				
				var pos = new Vector3( 100, 100 );
				if ( Overlays )
				{
					DebugOverlay.ScreenText( pos, 10, Color.White, "Focus distance " + DoFPoint.ToString("F") , 0.0f );
					DebugOverlay.ScreenText( pos, 11, Color.White, "Blur Size " + DoFBlurSize.ToString("F") , 0.0f );
				}
			}

			if ( PivotEnabled )
			{
				PivotMove();
			}
			else
			{
				FreeMove();
			}


			if ( Overlays )
			{
				var normalRot = Rotation.LookAt( tr.Normal );
				DebugOverlay.Axis( tr.EndPos, normalRot, 3.0f );

				if ( tr.Entity != null && !tr.Entity.IsWorld )
				{
					DebugOverlay.Text( tr.EndPos + Vector3.Up * 20, $"Entity: {tr.Entity} ({tr.Entity.EngineEntityName})\n" +
																	$" Index: {tr.Entity.NetworkIdent}\n" +
																	$"Health: {tr.Entity.Health}", Color.White );

					if ( tr.Entity is ModelEntity modelEnt )
					{
						var bbox = modelEnt.OOBBox;
						DebugOverlay.Box( 0, tr.Entity.Pos, tr.Entity.Rot, bbox.Mins, bbox.Maxs, Color.Green );

						for ( int i = 0; i < modelEnt.BoneCount; i++ )
						{
							var tx = modelEnt.GetBoneTransform( i );
							var name = modelEnt.GetBoneName( i );
							var parent = modelEnt.GetBoneParent( i );


							if ( parent > -1 )
							{
								var ptx = modelEnt.GetBoneTransform( parent );
								DebugOverlay.Line( tx.Pos, ptx.Pos, Color.White, depthTest: false );
							}
						}

					}
				}
			}
		}

		public override void BuildInput( ClientInput input )
		{

			MoveInput = input.AnalogMove;

			MoveSpeed = 1;
			if ( input.Down( InputButton.Run ) ) MoveSpeed = 5;
			if ( input.Down( InputButton.Duck ) ) MoveSpeed = 0.2f;

			if ( input.Pressed( InputButton.Walk ) )
			{
				var tr = Trace.Ray( Pos, Pos + Rot.Forward * 4096 ).Run();
				PivotPos = tr.EndPos;
				PivotDist = Vector3.DistanceBetween( tr.EndPos, Pos );
			}

			if( input.Down( InputButton.Use ) )
				DoFBlurSize = Math.Clamp( DoFBlurSize + ( Time.Delta * 3.0f ), 0.0f, 100.0f );
			
			if( input.Down( InputButton.Menu ) )
				DoFBlurSize = Math.Clamp( DoFBlurSize - ( Time.Delta * 3.0f ), 0.0f, 100.0f );
				

			if ( input.Down( InputButton.Attack2 ) )
			{
				FovOverride += input.AnalogLook.pitch * (FovOverride / 30.0f);
				FovOverride = FovOverride.Clamp( 5, 150 );
				input.AnalogLook = default;
			}

			LookAngles += input.AnalogLook * (FovOverride / 80.0f);
			LookAngles.roll = 0;

			PivotEnabled = input.Down( InputButton.Walk );

			if ( input.Pressed( InputButton.Reload ) ) Overlays = !Overlays;

			input.Clear();
			input.StopProcessing = true;
		}

		void FreeMove()
		{
			var mv = MoveInput.Normal * 300 * RealTime.Delta * Rot * MoveSpeed;

			TargetRot = Rotation.From( LookAngles );
			TargetPos += mv;

			Pos = Vector3.Lerp( Pos, TargetPos, 10 * RealTime.Delta );
			Rot = Rotation.Slerp( Rot, TargetRot, 40 * RealTime.Delta );
		}

		void PivotMove()
		{
			PivotDist -= MoveInput.x * RealTime.Delta * 100 * (PivotDist / 50);
			PivotDist = PivotDist.Clamp( 1, 1000 );

			TargetRot = Rotation.From( LookAngles );
			Rot = Rotation.Slerp( Rot, TargetRot, 40 * RealTime.Delta );

			TargetPos = PivotPos + Rot.Forward * -PivotDist;
			Pos = TargetPos;

			if ( Overlays )
			{
				var scale = Vector3.One * (2 + MathF.Sin( RealTime.Now * 10 ) * 0.3f);
				DebugOverlay.Box( PivotPos, scale * -1, scale, Color.Green );
			}
		}
	}
}
