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
	public class StandardPlayerAnimator : PlayerAnimator
	{
		TimeSince TimeSinceFootShuffle = 60;

		public override void Tick()
		{
			var idealRotation = Rotation.LookAt( Input.Rot.Forward.WithZ( 0 ), Vector3.Up );

			DoRotation( idealRotation );
			DoWalk( idealRotation );

			//
			// Let the animation graph know some shit
			//
			SetParam( "b_grounded", GroundEntity != null );
			SetParam( "b_swim", Player.WaterLevel.Fraction > 0.5f );

			//
			// Look in the direction what the player's input is facing
			//
			SetLookAt( "lookat_pos", Player.EyePos + Input.Rot.Forward * 200 );


			SetParam( "b_ducked", HasTag( "ducked" ) );

			if ( Player.ActiveChild is BaseCarriable carry )
			{
				carry.TickPlayerAnimator( this );
			}
			else
			{
				SetParam( "holdtype", 0 );
				SetParam( "aimat_weight", 0.0f );
			}

		}

		public virtual void DoRotation( Rotation idealRotation )
		{
			//
			// Our ideal player model rotation is the way we're facing
			//
			var allowYawDiff = Player.ActiveChild == null ? 120 : 50;

			float turnSpeed = 0.01f;
			if ( HasTag( "ducked" ) ) turnSpeed = 0.1f;

			//
			// If we're moving, rotate to our ideal rotation
			//
			Rot = Rotation.Slerp( Rot, idealRotation, WishVelocity.Length * Time.Delta * turnSpeed );

			//
			// Clamp the foot rotation to within 120 degrees of the ideal rotation
			//
			Rot = Rot.Clamp( idealRotation, allowYawDiff, out var change );

			//
			// If we did restrict, and are standing still, add a foot shuffle
			//
			if ( change > 1 && WishVelocity.Length <= 1 ) TimeSinceFootShuffle = 0;

			SetParam( "b_shuffle", TimeSinceFootShuffle < 0.1 );
		}

		void DoWalk( Rotation idealRotation )
		{
			//
			// These tweak the animation speeds to something we feel is right,
			// so the foot speed matches the floor speed. Your art should probably
			// do this - but that ain't how we roll
			//
			SetParam( "walkspeed_scale", 2.0f / 190.0f );
			SetParam( "runspeed_scale", 2.0f / 320.0f );
			SetParam( "duckspeed_scale", 2.0f / 80.0f );

			//
			// Work out our movement relative to our body rotation
			//
			var moveDir = WishVelocity.Normal * 100.0f; // extend move dir out so forward and sidewards can reach max input values
			var forward = idealRotation.Forward.Dot( moveDir );
			var sideward = idealRotation.Left.Dot( moveDir );

			//
			// Set our speeds on the animgraph
			//
			SetParam( "forward", forward );
			SetParam( "sideward", sideward );
			SetParam( "wishspeed", WishVelocity.Length );
		}

		public override void OnEvent( string name )
		{
			// DebugOverlay.Text( Pos + Vector3.Up * 100, name, 5.0f );

			if ( name == "jump" )
			{
				Trigger( "b_jump" );
			}

			base.OnEvent( name );
		}
	}
}
