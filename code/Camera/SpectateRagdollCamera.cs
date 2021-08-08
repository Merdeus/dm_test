using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class SpectateRagdollCamera : BaseCamera
	{
		Vector3 FocusPoint;

		public override void Activated()
		{
			base.Activated();

			FocusPoint = LastPos - GetViewOffset();
			FieldOfView = 70;
		}

		public override void Update()
		{
			var player = Player.Local as BasePlayer;
			if ( player == null ) return;

			// lerp the focus point
			FocusPoint = Vector3.Lerp( FocusPoint, GetSpectatePoint(), Time.Delta * 5.0f );

			Pos = FocusPoint + GetViewOffset();
			Rot = player.EyeRot;

			FieldOfView = FieldOfView.LerpTo( 50, Time.Delta * 3.0f );
			Viewer = null;
		}

		public virtual Vector3 GetSpectatePoint()
		{
			var player = Player.Local as BasePlayer;
			if ( player == null ) return LastPos;
			if ( !player.Corpse.IsValid() ) return player.Pos;

			return player.Corpse.PhysicsGroup.MassCenter;
		}

		public virtual Vector3 GetViewOffset()
		{
			var player = Player.Local as BasePlayer;
			if ( player == null ) return Vector3.Zero;

			return player.EyeRot.Forward * -130 + Vector3.Up * 20;
		}
	}
}
