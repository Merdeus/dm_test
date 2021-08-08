using Sandbox.Rcon;
using Steamworks.ServerList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class ThirdPersonCamera : BaseCamera
	{
		[UserVar]
		public static bool thirdperson_orbit { get; set; } = false;

		[UserVar]
		public static bool thirdperson_collision { get; set; } = false;

		private Angles orbitAngles;
		private float orbitDistance = 150;

		public override void Update()
		{
			var player = Player.Local;
			if ( player == null ) return;

			Pos = player.WorldPos;
			Vector3 targetPos;

			if ( thirdperson_orbit )
			{
				Pos += Vector3.Up * 40;
				Rot = Rotation.From( orbitAngles );

				targetPos = Pos + Rot.Backward * orbitDistance;
			}
			else
			{
				Pos = player.WorldPos;
				Pos += Vector3.Up * 70;
				Pos += player.EyeRot.Right * 30;
				Rot = Rotation.FromAxis( Vector3.Up, 4 ) * player.EyeRot;

				const float distance = 130;
				targetPos = Pos + player.EyeRot.Forward * -distance;
			}

			if ( thirdperson_collision )
			{
				var tr = Trace.Ray( Pos, targetPos )
					.Ignore( player )
					.Radius( 8 )
					.Run();

				Pos = tr.EndPos;
			}
			else
			{
				Pos = targetPos;
			}

			FieldOfView = 70;

			Viewer = null;
		}

		public override void BuildInput( ClientInput input )
		{
			if ( thirdperson_orbit && input.Down( InputButton.Walk ) )
			{
				if ( input.Down( InputButton.Attack1 ) )
				{
					orbitDistance += input.AnalogLook.pitch;
					orbitDistance = orbitDistance.Clamp( 0, 1000 );
				}
				else
				{
					orbitAngles.yaw += input.AnalogLook.yaw;
					orbitAngles.pitch += input.AnalogLook.pitch;
					orbitAngles = orbitAngles.Normal;
					orbitAngles.pitch = orbitAngles.pitch.Clamp( -89, 89 );
				}

				input.AnalogLook = Angles.Zero;

				input.Clear();
				input.StopProcessing = true;
			}

			base.BuildInput( input );
		}
	}
}
