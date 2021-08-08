using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	[ClassLibrary]
	public class FlyingController : BasePlayerController
	{
		public float Bounce { get; set; } = 0.25f;
		public float Size { get; set; } = 20.0f;

		public override void Tick()
		{
			Rot = Input.Rot;

			var vel = (Input.Rot.Forward * Input.Forward) + (Input.Rot.Left * Input.Left);

			vel = vel.Normal * 2000;

			if ( Input.Down( InputButton.Run ) )
				vel *= 5.0f;

			if ( Input.Down( InputButton.Duck ) )
				vel *= 0.2f;

			Velocity += vel * Time.Delta;

			if ( Velocity.LengthSquared > 0.01f )
			{
				Move( Velocity * Time.Delta );
			}

			Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

			if ( Input.Down( InputButton.Jump ) )
				Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );
		}

		public void Move( Vector3 delta, int a = 0 )
		{
			if ( a > 1 )
				return;

			var len = delta.Length;
			var targetPos = Pos + delta;
			var tr = Trace.Ray( Pos, targetPos ).WorldOnly().Size( Size ).Run();

			if ( tr.StartedSolid )
			{
				Pos = targetPos;
			}
			else if ( tr.Hit )
			{
				Pos = tr.EndPos + tr.Normal;

				// subtract the normal from our velocity
				Velocity = Velocity.SubtractDirection( tr.Normal * (1.0f + Bounce) );

				delta = Velocity.Normal * delta.Length * (1.0f - tr.Fraction);

				Move( delta, ++a );
			}
			else
			{
				Pos = tr.EndPos;
			}
		}
	}
}
