using Sandbox.UI;
using System;
using System.Threading.Tasks;

namespace Sandbox
{
    [ClassLibrary( "func_door_rotating" )]
	public partial class FuncDoorRotating : FuncDoor
	{
		[HammerProp( "distance" )]
		public float RotationDegrees { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			RotationA = Rot.Angles();

			var degrees = RotationDegrees - Lip;

			if ( SpawnFlags.Has( Flags.RotateBackwards ) ) degrees *= -1.0f;

			if ( SpawnFlags.Has( Flags.Pitch ) ) RotationB = RotationA + new Angles( degrees, 0, 0 );
			else if ( SpawnFlags.Has( Flags.Roll ) ) RotationB = RotationA + new Angles( 0, 0, degrees );
			else RotationB = RotationA + new Angles( 0, degrees, 0 );
		}

		Angles RotationA;
		Angles RotationB;


		public override void UpdateState( bool open )
		{
			var rot = open ? RotationB : RotationA;

			if ( Speed <= 0 )
				Speed = 0.1f;

			var dfference = (Rot.Angles() - rot).Normal;
			var distance = dfference.Length;
			var seconds = distance / Speed;

			_ = DoMove( Rotation.From( rot ), seconds );
		}

		int movement = 0;

		async Task DoMove( Rotation target, float timeToTake )
		{
			var startPos = WorldRot;
			int moveid = ++movement;

			for ( float f = 0; f < 1; )
			{
				await Task.NextPhysicsFrame();

				if ( moveid != movement )
					return;

				var eased = Easing.BounceOut( f );

				var newPos = Rotation.Lerp( startPos, target, eased );
				SetPositionAndUpdateVelocity( newPos );
				f += Time.Delta / timeToTake;
			}
		}

		void SetPositionAndUpdateVelocity( Rotation pos )
		{
			var oldPos = WorldRot;
			//  set angular velocity so it lerps on client?
			// would it lerp anyway?
			WorldRot = pos;
		}
	}
}
