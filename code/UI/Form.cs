
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

namespace Sandbox.UI
{
	public class Form : Panel
	{
		public object Bound;

		public Form()
		{
			AddClass( "form" );
		}

		public void BindObject( object obj )
		{
			Bound = obj;

			foreach ( var property in Reflection.GetProperties( obj ) )
			{
				AddRow( property );
			}
		}

		public void AddRow( Property property )
		{
			var row = Add.Panel( "form-row" );

			var title = row.Add.Panel( "form-label" );
			title.Add.Label( property.Name );

			var value = row.Add.Panel( "form-value" );
			//value.Add.Label( $"{property.Value}" );

			var control = value.Add.TextEntry( "" );
			control.DataBind = property;
		}
	}
}