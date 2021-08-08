using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{

	[ClassLibrary( "Property", Icon = Icon.BorderAll )]
	public class PropertyField : Panel
	{
		static Logger log = Logging.GetLogger();

		public Rcon.Property Property { get; private set; }
		public PropertyClass Target { get; private set; }

		public Label Label { get; internal set; }
	//	public TextEntry Control { get; internal set; }

		public PropertyField()
		{
			throw new NotImplementedException();
			//Class.AddMany( "property-field textentry" );

			Label = Add.Label( "label", "label" );
		//	Control = Add.TextEntry( "control" );
		}

		public void SetTarget( PropertyClass target, Rcon.Property prop )
		{
			Target = target;
			Property = prop;

			Label.Text = prop.Label;

			throw new NotImplementedException();
			//Control.DataContext = target;
			//Control.Bind( prop.Name );

			{
				//Label.Tooltip = prop.Help;
			}
		}

		//public override LayoutCascade LayoutContent( LayoutCascade cascade )
		//{
		//	cascade = base.LayoutContent( cascade );

		//	throw new NotImplementedException();
			//Class.Set( "horizontal", size.x > 290 );

		//return cascade;
		//}

	}

	namespace Construct
	{
		public static class PropertyFieldConstructor
		{
			public static PropertyField Property( this PanelCreator self, PropertyClass target, Rcon.Property property )
			{
				var control = self.panel.AddChild<PropertyField>();
				control.SetTarget( target, property );
				return control;
			}
		}
	}

}