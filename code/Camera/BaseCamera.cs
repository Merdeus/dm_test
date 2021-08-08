using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public abstract class BaseCamera : Camera
	{
		/// <summary>
		/// This builds the default behaviour for our input
		/// </summary>
		public override void BuildInput( ClientInput input )
		{
			//
			// If we're using the mouse then
			// increase pitch sensitivity
			//
			if ( input.UsingMouse )
			{
				input.AnalogLook.pitch *= 1.5f;
			}

			// add the view move, clamp pitch
			input.ViewAngles += input.AnalogLook;
			input.ViewAngles.pitch = input.ViewAngles.pitch.Clamp( -89, 89 );

			// Just copy input as is
			input.InputDirection = input.AnalogMove;
		}

		/// <summary>
		/// Camera has become the active camera. You can use this as an opportunity
		/// to snap the positions if you're lerping etc.
		/// </summary>
		public override void Activated()
		{

		}


		/// <summary>
		/// Camera has stopped being the active camera.
		/// </summary>
		public override void Deactivated()
		{

		}

	}
}
