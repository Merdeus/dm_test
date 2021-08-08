using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{
	[ClassLibrary( "ButtonGroup" )]
	public class ButtonGroup : Panel
	{
		// TODO - allow multi select
		// TODO - allow toggle off

		public Button AddButton( string value, Action action ) 
		{
			var btn = Add.Button( value, action );
			btn.AddEvent( "onclick", () => SelectedButton = btn );

			return btn;
		}

		public Button AddButtonActive( string value, Action<bool> action )
		{
			var btn = Add.Button( value );
			btn.AddEvent( "onclick", () => SelectedButton = btn );

			btn.AddEvent( "startactive", () => action( true ) );
			btn.AddEvent( "stopactive", () => action( false ) );

			return btn;
		}

		Panel _selected;

		public Panel SelectedButton
		{
			get => _selected;
			set
			{
				if ( _selected == value )
					return;

				_selected?.RemoveClass( "active" );
				_selected?.OnEvent( "stopactive" );

				_selected = value;

				_selected?.AddClass( "active" );
				_selected?.OnEvent( "startactive" );
			}
		}
	}

	namespace Construct
	{
		public static class ButtonGroupConstructor
		{
			public static ButtonGroup ButtonGroup( this PanelCreator self, string classes = null )
			{
				var control = self.panel.AddChild<ButtonGroup>();

				if ( classes != null )
					control.AddClass( classes );

				return control;
			}
		}
	}

}