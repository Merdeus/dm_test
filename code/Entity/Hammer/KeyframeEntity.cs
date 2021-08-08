using Sandbox.UI;
using System;
using System.Threading.Tasks;

namespace Sandbox
{
	/// <summary>
	/// An entity that is moved programatically. Like an elevator
	/// or a kliner smashing star wars garbage compactor
	/// </summary>
	public partial class KeyframeEntity : ModelEntity
	{
		int movement = 0;
		public async Task<bool> KeyframeTo( Transform target, float seconds, Easing.Function easing = null )
		{
			var start = Transform;

			var moveid = ++movement;

			for ( float f = 0; f < 1; )
			{
				await Task.NextPhysicsFrame();

				if ( moveid != movement )
					return false;

				var eased = easing != null ? easing( f ) : f;

				var newtx = Transform.Lerp( start, target, eased, false );

				TryKeyframeTo( newtx );

				f += Time.Delta / seconds;
			}

			Transform = target;
			return true;
		}

		/// <summary>
		/// Try to move to this position.
		/// </summary>
		public virtual bool TryKeyframeTo( Transform pos )
		{
			Transform = pos;
			return true;
		}
	}
}
