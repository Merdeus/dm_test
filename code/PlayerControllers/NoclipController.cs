using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	[ClassLibrary]
	public class NoclipController : BasePlayerController
	{
		public override void Tick()
		{
			var vel = (Input.Rot.Forward * Input.Forward) + (Input.Rot.Left * Input.Left);

			vel = vel.Normal * 2000;

			if ( Input.Down( InputButton.Run ) )
				vel *= 5.0f;

			if ( Input.Down( InputButton.Duck ) )
				vel *= 0.2f;

			Velocity += vel * Time.Delta;

			if ( Velocity.LengthSquared > 0.01f )
			{
				Pos += Velocity * Time.Delta;
			}

			Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

			if ( Input.Down( InputButton.Jump ) )
				Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

			WishVelocity = Velocity;
			GroundEntity = null;
		}
	}
}
