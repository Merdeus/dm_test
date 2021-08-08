using Sandbox.UI;
using System;

namespace Sandbox
{
	/// <summary>
	/// An entity that holds the HUD class. This is an entity so you can 
	/// add RPCs and shit to let your server send messages to control the
	/// HUd.
	/// </summary>
	[ClassLibrary( "hud" )]
	public class Hud : Entity
	{
		public static Hud Current { get; set; }
		public static RootPanel CurrentPanel => Current?.RootPanel;
		public RootPanel RootPanel { get; set; }

		public Hud()
		{
			Transmit = TransmitType.Always;
			Current = this;

			if ( IsClient )
			{
				//
				// The root panel that holds 
				// all the other controls.
				//
				RootPanel = new RootPanel();
				RootPanel.Style.Set( "pointer-events: none; width: 100%; height: 100%;" );
				RootPanel.AcceptsFocus = false;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			//
			// clean up the panel 
			// or it'll stick around forever
			//
			RootPanel?.Delete();
			RootPanel = null;
		}
	}
}
