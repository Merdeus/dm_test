using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox
{
	public class Unstuck
	{
		public BasePlayerController Controller;

		public bool IsActive; // replicate

		internal int StuckTries = 0;

		public Unstuck( BasePlayerController controller )
		{
			Controller = controller;
		}

		public virtual bool TestAndFix() 
		{
			var result = Controller.TraceBBox( Controller.Pos, Controller.Pos );

			// Not stuck, we cool
			if ( !result.StartedSolid )
			{
				StuckTries = 0;
				return false;
			}

			if ( result.StartedSolid )
			{
				if ( BasePlayerController.Debug )
				{
					DebugOverlay.Text( Controller.Pos, $"[stuck in {result.Entity}]", Color.Red );
					Box( result.Entity, Color.Red );
				}
			}

			//
			// Client can't jiggle its way out, needs to wait for 
			// server correction to come
			//
			if ( Host.IsClient )
				return true;

			int AttemptsPerTick = 20;

			for ( int i=0; i< AttemptsPerTick; i++ )
			{
				var pos = Controller.Pos + Vector3.Random.Normal * (((float)StuckTries) / 2.0f);
				result = Controller.TraceBBox( pos, pos );

				if ( !result.StartedSolid )
				{
					if ( BasePlayerController.Debug )
					{
						DebugOverlay.Text( Controller.Pos, $"unstuck after {StuckTries} tries ({StuckTries* AttemptsPerTick} tests)", Color.Green, 5.0f );
						DebugOverlay.Line( pos, Controller.Pos, Color.Green, 5.0f, false );
					}

					Controller.Pos = pos;
					return false;
				}
				else
				{
					if ( BasePlayerController.Debug )
					{
						DebugOverlay.Line( pos, Controller.Pos, Color.Yellow, 0.5f, false );
					}
				}
			}

			StuckTries++;

			return true;
		}

		public void Box( Entity ent, Color color, float duration = 0.0f )
		{
			if ( ent is ModelEntity modelEnt )
			{
				var bbox = modelEnt.OOBBox;
				DebugOverlay.Box( duration, modelEnt.Pos, modelEnt.Rot, bbox.Mins, bbox.Maxs, color );
			}
			else
			{
				DebugOverlay.Box( duration, ent.Pos, ent.Rot, -1, 1, color );
			}
		}
	}
}
