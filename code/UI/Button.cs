using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{

	[ClassLibrary( "Button" )]
	public class Button : Panel
	{
		Label label;

		public void SetText( string text )
		{
			if ( label == null )
			{
				label = AddChild<Label>();
			}

			label.Text = text;
		}

		public override void SetProperty( string name, string value )
		{
			switch ( name )
			{
				case "text":
					{
						SetText( value );
						return;
					}

				case "html":
					{
						SetText( value );
						return;
					}
			}

			base.SetProperty( name, value );
		}

		public void Click()
		{
			OnEvent( "onclick" );
		}

	}

	namespace Construct
	{
		public static class ButtonConstructor
		{
			public static Button Button( this PanelCreator self, string text, Action onClick = null )
			{
				var control = self.panel.AddChild<Button>();
				control.SetText( text );

				if ( onClick != null )
					control.AddEvent( "onclick", onClick );

				return control;
			}

			public static Button ButtonWithConsoleCommand( this PanelCreator self, string text, string command )
			{
				return Button( self, text, () => ConsoleSystem.Run( command ) );
			}
		}
	}

}