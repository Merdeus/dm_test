using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{

	[ClassLibrary( "PropertySection", Icon = Icon.BorderAll )]
	public class PropertySection : Panel
	{
		public Panel Header { get; internal set; }
		public Panel Body { get; internal set; }

		public IconPanel IconPanel { get; internal set; }
		public Label Label { get; internal set; }

		public PropertySection()
		{
			throw new NotImplementedException();
			//Class.Add( "property-section" );

			Header = Add.Panel( "header" );
			Body = Add.Panel( "body" );

			IconPanel = Header.Add.Icon( Icon.Folder );
			//IconPanel.HitTest = false;

			Label = Header.Add.Label( "Property Section", "label" );
			//Label.HitTest = false;

			Header.AddEvent( "onclick", Toggle );
			//Header.AcceptsInput = true;
		}

		void Toggle()
		{
			throw new NotImplementedException();
			//Class.Toggle( "closed" );
		}
	}

	namespace Construct
	{
		public static class PropertySectionConstructor
		{
			public static PropertySection PropertySection( this PanelCreator self, string label, Icon icon )
			{
				var control = self.panel.AddChild<PropertySection>();
				control.Label.Text = label;
				control.IconPanel.Icon = icon;
				return control;
			}
		}
	}

}