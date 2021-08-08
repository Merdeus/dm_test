using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{

	[ClassLibrary( "PropertySheet", Icon = Icon.BorderAll )]
	public class PropertySheet : Panel
	{
		static Logger log = Logging.GetLogger();

		public Panel PropertyList { get; internal set; }

		public PropertyClass _target;

		public PropertyClass Target
		{
			get => _target;
			set
			{
				if ( _target == value )
					return;

				_target = value;
				Rebuild();
			}
		}

		public PropertySheet()
		{
			PropertyList = Add.Panel( "property-list" );
		}

		void Rebuild()
		{
			throw new NotImplementedException();
			//PropertyList.DeleteChildren();

			if ( _target == null )
				return;

			var sheet = new Rcon.PropertySheet();

			_target.FillPropertySheet( sheet );

			if ( sheet == null )
				return;

			if ( sheet.Values != null )
			{
				RebuildProperties( sheet.Values );
			}

		}

		private void RebuildProperties( List<Rcon.Property> values )
		{
			foreach ( var group in values.GroupBy( x => x.Group ) )
			{
				var g = PropertyList.Add.PropertySection( group.Key, group.First().GroupIcon );

				foreach ( var prop in group )
				{
					g.Body.Add.Property( Target, prop );
				}
			}
		}
	}

	namespace Construct
	{
		public static class PropertySheetConstructor
		{
			public static PropertySheet PropertySheet( this PanelCreator self, PropertyClass propClass )
			{
				var control = self.panel.AddChild<PropertySheet>();
				control.Target = propClass;
				return control;
			}
		}
	}

}