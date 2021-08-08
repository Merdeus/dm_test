using System;
using System.Threading.Tasks;

namespace Sandbox
{
    [ClassLibrary( "func_rot_button" )]
	public partial class FuncButtonRotating : KeyframeEntity, IUse
	{
		[Flags]
		public enum Flags
		{
			NotSolid = 1,
			Reverse = 2,
			Toggle = 32,
			Roll = 64,
			Pitch = 128,
			TouchActivates = 256,
			DamageActivates = 512,
			UseActivates = 1024,
			StartsLocked = 2048,
			Sparks = 4096,
			NonSolid = 16384,
		}

		[HammerProp( "wait" )]
		public float DelayBeforeReset { get; set; }

		[HammerProp( "distance" )]
		public float Degrees { get; set; }		
		
		[HammerProp( "speed" )]
		public float Speed { get; set; }

		[HammerProp( "use_sound" )]
		public string UseSound { get; set; }

		[HammerProp( "locked_sound" )]
		public string LockedSound { get; set; }

		[HammerProp( "unlocked_sound" )]
		public string UnlockedSound { get; set; }


		public bool Locked { get; set; }

		public float SecondsToTake;
		public bool State;
		public bool Moving;
		public Transform TxOff;
		public Transform TxOn;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			if ( SpawnFlags.Has( Flags.NonSolid ) )
			{
				EnableSolidCollisions = false;
			}

			Locked = SpawnFlags.Has( Flags.StartsLocked );

			TxOff = Transform;

			if ( SpawnFlags.Has( Flags.Reverse ) )
				Degrees *= -1;

			var angle = new Angles( 0, 1, 0 );
			if ( SpawnFlags.Has( Flags.Roll ) ) angle = new Angles( 0, 0, 1 );
			else if ( SpawnFlags.Has( Flags.Pitch ) ) angle = new Angles( 1, 0, 0 );

			var delta = Rotation.From( angle * Degrees );

			if ( Speed <= 0 ) Speed = 0.01f;
			SecondsToTake = Degrees / Speed;

			TxOn = TxOff;
			TxOn.Rot = delta * TxOn.Rot;
		}

		public virtual bool IsUsable( Entity user ) => !Moving && SpawnFlags.Has( Flags.UseActivates );


		/// <summary>
		/// A player has pressed this
		/// </summary>
		public virtual bool OnUse( Entity user )
		{
			if ( Locked )
			{
				FireOutput( "OnUseLocked", user, this );
				return false;
			}

			if ( SpawnFlags.Has( Flags.UseActivates ) )
			{
				DoPress( user );
			}

			return false;
		}

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );

			FireOutput( "OnDamaged", info.Attacker, this );

			if ( SpawnFlags.Has( Flags.DamageActivates ) )
			{
				DoPress( info.Attacker );
			}
		}

		internal void DoPress( Entity activator )
		{
			if ( Locked ) return;

			State = !State;
			FireOutput( "OnPressed", activator, this );

			_ = MoveToPosition( State, activator );
		}

		async Task MoveToPosition( bool state, Entity activator )
		{
			State = state;
			Moving = true;

			// try to move - if return false we were interupted or otherwise failed
			if ( ! await KeyframeTo( State ? TxOn : TxOff, SecondsToTake ) )
				return;

			if ( state ) FireOutput( "OnIn", activator, this );
			else FireOutput( "OnOut", activator, this );

			if ( state == false )
				return;

			if ( DelayBeforeReset <= 0 )
				return;

			await Task.DelaySeconds( DelayBeforeReset );

			await MoveToPosition( false, activator );

			Moving = false;
		}

		[Input]
		protected void Lock()
		{
			if ( Locked ) return;

			Locked = true;
			// lock sound
		}

		[Input]
		protected void Unlock()
		{
			if ( !Locked ) return;

			Locked = false;
			// unlock sound
		}

		[Input]
		protected void Press( Entity activator )
		{
			
		}

		[Input]
		protected void PressIn( Entity activator )
		{

		}

		[Input]
		protected void PressOut( Entity activator )
		{

		}
	}
}
