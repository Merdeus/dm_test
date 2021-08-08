using Sandbox.UI;
using System;
using System.Threading.Tasks;

namespace Sandbox
{
    [ClassLibrary( "func_door" )]
	public partial class FuncDoor : KeyframeEntity, IUse
	{
		public enum Flags
		{
			RotateBackwards = 2,
			NonSolidToPlayer = 4,
			Passable = 8,
			OneWay = 16,
			NoAutoReturn = 32,
			Roll = 64,
			Pitch= 128,
			Use = 256,
			NoNpcs = 512,
			Touch = 1024,
			StartLocked = 2048,
			Silent = 4096,
			UseCloses = 8192,
			SilentToNpcs = 16384,
			IgnoreUse = 32768,
			StartUnbreakable = 524288,
		}

		public enum DoorState
		{
			Open,
			Closed,
			Opening,
			Closing
		}

		[HammerProp( "movedir" )]
		public Angles MoveDir { get; set; }

		[HammerProp( "movedir_islocal" )]
		public bool MoveDirIsLocal { get; set; }

		[HammerProp( "spawnpos" )]
		public bool SpawnOpen { get; set; }

		[HammerProp( "lip" )]
		public float Lip { get; set; }

		[HammerProp( "speed" )]
		public float Speed { get; set; }

		[HammerProp( "wait" )]
		public float TimeBeforeReset { get; set; }

		Vector3 PositionA;
		Vector3 PositionB;

		public DoorState State { get; protected set; } = DoorState.Open;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			PositionA = WorldPos;

			// Get the direction we want to move
			var dir = Rotation.From( MoveDir ).Forward;
			if ( MoveDirIsLocal ) dir = Transform.NormalToWorld( dir );

			// Open position is the size of the bbox in the direction minus the lip size
			var boundSize = OOBBox.Size;

			PositionB = WorldPos + dir * (MathF.Abs( boundSize.Dot( dir ) ) - Lip);

			State = DoorState.Closed;

			if ( SpawnOpen )
			{
				WorldPos = PositionB;
				State = DoorState.Open;
			}
		}

		public virtual bool IsUsable( Entity user ) => SpawnFlags.Has( Flags.Use ) && !SpawnFlags.Has( Flags.IgnoreUse );

		/// <summary>
		/// A player has pressed this
		/// </summary>
		public virtual bool OnUse( Entity user )
		{
			if ( SpawnFlags.Has( Flags.Use ) )
			{
				Toggle();
			}

			return false;
		}

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );
		}


		[Input]
		protected void Toggle()
		{
			if ( State == DoorState.Open || State == DoorState.Opening ) State = DoorState.Closing;
			else if ( State == DoorState.Closed || State == DoorState.Closing ) State = DoorState.Opening;

			UpdateState( State == DoorState.Opening );
		}

		int movement = 0;
		async Task DoMove( Vector3 position )
		{
			var startPos = WorldPos;

			var distance = Vector3.DistanceBetween( startPos, position );
			var timeToTake = distance / Speed;
			var moveid = ++movement;

			for ( float f = 0; f < 1; )
			{
				await Task.NextPhysicsFrame();

				if ( moveid != movement )
					return;

				var eased = Easing.EaseInOut( f );

				var newPos = Vector3.Lerp( startPos, position, eased );
				SetPositionAndUpdateVelocity( newPos );
				f += Time.Delta / timeToTake;
			}
		}

		void SetPositionAndUpdateVelocity( Vector3 pos )
		{
			var oldPos = WorldPos;
			Velocity = oldPos - pos;
			WorldPos = pos;
		}

		public virtual void UpdateState( bool open )
		{
			var position = open ? PositionB : PositionA;

			if ( Speed <= 0 )
				Speed = 0.1f;

			var distance = WorldPos.Distance( position );
			var seconds = distance / Speed;

			//MoveTo( position, seconds );



			_ = DoMove( position );
		}


		public override void MoveFinished()
		{
			if ( State == DoorState.Opening ) State = DoorState.Open;
			if ( State == DoorState.Closing ) State = DoorState.Closed;

			if ( State == DoorState.Closed ) return;
			if ( TimeBeforeReset < 0 ) return;

			// TODO - cancel old task?
			_ = ToggleInSeconds( TimeBeforeReset );
		}

		public async Task ToggleInSeconds( float seconds )
		{
			await Task.DelaySeconds( seconds );

			Toggle();
		}
	}
}
