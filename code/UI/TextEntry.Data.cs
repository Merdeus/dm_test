using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{
	public partial class TextEntry : Label
	{
		public override void OnDataBindChanged( object data )
		{
			//
			// Don't update the contents from data if
			// we're editing it. That gets messy.
			//
			if ( HasFocus ) return;

			base.OnDataBindChanged( data ); 

			Text = $"{data}";
		}

		public virtual void UpdateDataBind()
		{
			if ( DataBind == null )
				return;

			var obj = Text.ToType( DataBind.Type );

			DataBind.Value = obj;
		}
	}
}