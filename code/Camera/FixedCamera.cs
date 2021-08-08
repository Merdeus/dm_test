using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class FixedCamera : BaseCamera
	{
		Vector3 TargetPos;
		Rotation TargetRot;

		public FixedCamera()
		{
			var player = Player.Local;
			if ( player != null )
			{
				Pos = player.EyePos;
				TargetPos = Pos;

				Rot = player.EyeRot;
				TargetRot = Rot;
			}
		}

		public override void Update()
		{
			FieldOfView = 70;
			Pos = TargetPos;
			Rot = TargetRot;
			Viewer = null;
		}
	}
}
