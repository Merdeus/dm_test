using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{

	[ClassLibrary( "IconPanel", Icon = Icon.Icons )]
	public class IconPanel : Label
	{
		public Icon _icon;

		public Icon Icon
		{
			get => _icon;
			set
			{
				if ( _icon == value ) return;
				_icon = value;
				Text = ((char)(int)_icon).ToString();
			}
		}
	}

	namespace Construct
	{
		public static class IconPanelConstructor
		{
			public static IconPanel Icon( this PanelCreator self, Icon icon )
			{
				var control = self.panel.AddChild<IconPanel>();
				control.Icon = icon;
				return control;
			}
		}
	}

}