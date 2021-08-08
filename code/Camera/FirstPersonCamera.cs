using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class FirstPersonCamera : BaseCamera
	{
		Vector3 lastPos;

		public override void Activated()
		{
			var player = Player.Local;
			if ( player == null ) return;

			Pos = player.EyePos;
			Rot = player.EyeRot;

			lastPos = Pos;
		}

		public override void Update()
		{
			var player = Player.Local;
			if ( player == null ) return;

			Pos = Vector3.Lerp( player.EyePos.WithZ( lastPos.z ), player.EyePos, 20.0f * Time.Delta );
			Rot = player.EyeRot;

			FieldOfView = 80;

			Viewer = player;
			lastPos = Pos;
		}
	}
}
