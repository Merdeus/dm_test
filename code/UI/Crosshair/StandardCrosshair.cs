
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Sandbox.UI
{
	public partial class StandardCrosshair : Panel
	{
		public StandardCrosshair()
		{
			StyleSheet = StyleSheet.FromFile( "/ui/crosshair/StandardCrosshair.scss" );
		}

		float scale = 1;

		public override void Tick()
		{
			base.Tick();

			Style.Width = 12 * scale;
			Style.Height = 12 * scale;
			Style.Dirty();

			scale = scale.LerpTo( 1, Time.Delta * 5 );

		}

		public override void OnEvent( string eventName )
		{
			if ( eventName == "onattack" )
			{
				scale = 10;
				return;
			}

			base.OnEvent( eventName ); 
		}
	}
}