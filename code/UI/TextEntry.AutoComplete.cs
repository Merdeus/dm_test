using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{
	public partial class TextEntry : Label
	{
		/// <summary>
		/// If you hook a method up here we'll do autocomplete on it
		/// </summary>
		public Func<string, object[]> AutoComplete { get; set; }

		internal Menu AutoCompletePanel;


		public void UpdateAutoComplete()
		{
			if ( AutoComplete == null || !HasFocus )
			{
				DestroyAutoComplete();
				return;
			}

			var results = AutoComplete( Text );
			if ( results == null || results.Length == 0 )
			{
				DestroyAutoComplete();
				return;
			}

			UpdateAutoComplete( results );
		}

		public void UpdateAutoComplete( object[] options )
		{ 
			if ( AutoCompletePanel == null )
			{
				AutoCompletePanel = AddChild<Menu>();
				AutoCompletePanel.AddClass( "autocomplete" );
			}

			AutoCompletePanel.DeleteChildren( true );
			AutoCompletePanel.UserData = Text;

			foreach ( var r in options )
			{
				var b = AutoCompletePanel.Add.Button( r.ToString(), () => AutoCompleteSelected( r ) );
				b.UserData = r;
			}
		}

		public virtual void DestroyAutoComplete()
		{
			AutoCompletePanel?.Delete();
			AutoCompletePanel = null;
		}

		void AutoCompleteSelected( object obj )
		{
			Text = obj.ToString();
			Focus();
			OnValueChanged();

			CaretPos = Text.Length;
		}

		protected virtual void AutoCompleteSelectionChanged()
		{
			var selected = AutoCompletePanel.SelectedChild;
			if ( selected == null ) return;

			Text = selected.UserData.ToString();
			MoveCaratPos( Text.Length );
		}

		protected virtual void AutoCompleteCancel()
		{
			Text = AutoCompletePanel.UserData.ToString();
			DestroyAutoComplete();
		}
	}
}
