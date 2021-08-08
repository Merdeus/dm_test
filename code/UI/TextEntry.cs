using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{

	[ClassLibrary( "TextEntry" )]
	public partial class TextEntry : Label
	{
		public bool AllowEmojiReplace { get; set; } = false;

		public TextEntry()
		{
			AcceptsFocus = true;
		}

		public int CaretPos { get; protected set; }

		public void SetCaretPos( int p, bool selecting = false )
		{
			if ( SelectionEnd == 0 && SelectionStart == 0 && selecting )
			{
				SelectionStart = CaretPos.Clamp( 0, Text.Length );
			}

			CaretPos = p.Clamp( 0, Text.Length );

			if ( selecting )
			{
				SelectionEnd = CaretPos;
			}
			else
			{
				SelectionEnd = 0;
				SelectionStart = 0;
			}
		}

		void CaretSantity()
		{
			if ( CaretPos > Text.Length ) CaretPos = Text.Length;
			if ( SelectionStart > Text.Length ) SelectionStart = Text.Length;
			if ( SelectionEnd > Text.Length ) SelectionEnd = Text.Length;
		}

		public string GetSelectedText()
		{
			CaretSantity();

			var s = Math.Min( SelectionStart, SelectionEnd );
			var e = Math.Max( SelectionStart, SelectionEnd );

			return Text.Substring( s, e - s );
		}

		bool HasSelection() => SelectionStart != SelectionEnd;

		void ReplaceSelection( string str )
		{
			var s = Math.Min( SelectionStart, SelectionEnd );
			var e = Math.Max( SelectionStart, SelectionEnd );
			var len = e - s;

			if ( CaretPos > e ) CaretPos -= len;
			else if ( CaretPos > s ) CaretPos = s;

			CaretPos += str.Length;

			Text = Text.Remove( s, len );
			Text = Text.Insert( s, str );

			SelectionStart = 0;
			SelectionEnd = 0;
		}

		void SetSelection( int start, int end )
		{
			var s = Math.Min( start, end ).Clamp( 0, Text.Length );
			var e = Math.Max( start, end ).Clamp( 0, Text.Length );

			if ( s == e )
			{
				s = 0;
				e = 0;
			}

			SelectionStart = s;
			SelectionEnd = e;
		}

		public void MoveCaratPos( int delta, bool selecting = false )
		{
			SetCaretPos( CaretPos + delta, selecting );
		}

		public override void OnPaste( string text )
		{
			foreach ( var c in text )
			{
				OnKeyTyped( c );
			}
		}

		public override string GetClipboardValue( bool cut )
		{
			if ( !HasSelection() )
				return null;

			var txt = GetSelectedText();

			if ( cut )
			{
				ReplaceSelection( "" );
			}

			return txt;
		}

		public override void OnButtonTyped( string button, KeyModifiers km )
		{
			//Log.Info( $"OnButtonTyped {button}" );

			if ( HasSelection() && ( button == "delete" || button == "backspace" ) )
			{
				ReplaceSelection( "" );
				OnValueChanged();

				return;
			}

			if ( button == "delete" )
			{
				if ( CaretPos < Text.Length )
				{
					Text = Text.Remove( CaretPos, 1 );
					OnValueChanged();
				}

				return;
			}

			if ( button == "backspace" )
			{
				if ( CaretPos > 0 )
				{
					Text = Text.Remove( CaretPos - 1, 1 );

					MoveCaratPos( -1 );
					OnValueChanged();
				}

				return; 
			}

			if ( button == "a" && km.Ctrl )
			{
				SelectionStart = 0;
				SelectionEnd = Text.Length;
				return;
			}

			if ( button == "home" )
			{
				SetCaretPos( 0, km.Shift );
				return;
			}

			if ( button == "end" )
			{
				SetCaretPos( Text.Length, km.Shift );
				return;
			}

			if ( button == "left" )
			{
				MoveCaratPos( -1, km.Shift );
				return;
			}

			if ( button == "right" )
			{
				MoveCaratPos( 1, km.Shift );
				return;
			}

			if ( button == "down" || button == "up" )
			{
				if ( AutoCompletePanel != null )
				{
					AutoCompletePanel.MoveSelection( button == "up" ? -1 : 1 );
					AutoCompleteSelectionChanged();
					return;
				}

				//
				// We have history items, autocomplete using those
				//
				if ( string.IsNullOrEmpty( Text ) && AutoCompletePanel == null && History.Count > 0 )
				{
					UpdateAutoComplete( History.ToArray() );

					// select last item
					AutoCompletePanel.MoveSelection( -1 );
					AutoCompleteSelectionChanged();

					return;
				}

				MoveCaratPos( button == "up" ? -10000 : 100000 );
				return;
			}

			if ( button == "enter" )
			{
				if ( AutoCompletePanel != null && AutoCompletePanel.SelectedChild != null )
				{
					DestroyAutoComplete();
				}

				Blur();
				OnEvent( "onsubmit" );
				return;
			}

			if ( button == "escape" )
			{
				if ( AutoCompletePanel != null )
				{
					AutoCompleteCancel();
					return;
				}

				Blur();
				OnEvent( "oncancel" );
				return;
			}

			base.OnButtonTyped( button, km );
		}

		public override void OnEvent( string eventName )
		{
			if ( eventName == "onmousedown" )
			{
				var pos = GetLetterAt( MousePos );

				if ( pos  >= 0 )
				{
					SetCaretPos( pos );
				}
			}

			if ( eventName == "onmouseup" )
			{
				var pos = GetLetterAt( MousePos );
				if ( SelectionEnd > 0 ) pos = SelectionEnd;
				 CaretPos = pos.Clamp( 0, Text.Length );
			}

			if ( eventName == "onmousemove" && HasActive )
			{
				var pos = GetLetterAt( MousePos );
				if ( pos != CaretPos )
				{
					SetSelection( CaretPos, pos );
				}
			}

			if ( eventName == "onfocus" )
			{
				UpdateAutoComplete();
			}

			if ( eventName == "onblur" )
			{
				UpdateAutoComplete();
			}

			base.OnEvent( eventName );
		}

		public override void OnDoubleClick( string button )
		{
			if ( button  == "mouseleft" )
			{
				SelectWord( GetLetterAt( MousePos ) );
				CaretPos = SelectionEnd;
			}

			base.OnDoubleClick( button );
		}

		public void SelectWord( int wordPos )
		{
			if ( Text.Length <= wordPos )
			{
				SelectionStart = 0;
				SelectionEnd = 0;
				return;
			}

			SelectionStart = wordPos;
			SelectionEnd = wordPos;

			while ( SelectionStart > 0 )
			{
				if ( char.IsWhiteSpace( Text[SelectionStart - 1] ) )
					break;

				SelectionStart--;
			}

			while ( SelectionEnd < Text.Length )
			{
				if ( char.IsWhiteSpace( Text[SelectionEnd] ) )
					break;

				SelectionEnd++;
			}
		}

		public override void OnKeyTyped( char k )
		{
			CaretSantity();

			if ( HasSelection() )
			{
				ReplaceSelection( k.ToString() );
			}
			else
			{
				Text = Text.Insert( CaretPos, k.ToString() );
				MoveCaratPos( 1 );
			}

			if ( k == ':' )
			{
				RealtimeEmojiReplace();
			}

			OnValueChanged();
		}

		public override void DrawContent( Renderer renderer, ref RenderState state ) 
		{
			CaretSantity();
			ShouldDrawSelection = HasFocus;
			
			base.DrawContent( renderer, ref state );

			var blinkRate = 1;

			if ( HasFocus && (RealTime.Now * blinkRate )% 1.0f > 0.5f )
			{
				var caret = GetCaretRect( CaretPos );
				renderer.DrawRect( caret, Color.Yellow );
			}
		}

		void RealtimeEmojiReplace()
		{
			if ( !AllowEmojiReplace ) 
				return;

			if ( CaretPos == 0 )
				return;

			string lookup = null;

			for ( int i = CaretPos-2; i >= 0; i-- )
			{
				var c = Text[i];

				if ( char.IsWhiteSpace( c ) )
					return;

				if ( c == ':' )
				{
					lookup = Text.Substring( i, CaretPos - i );
					break;
				}

				if ( i == 0 )
					return;
			}

			if ( lookup == null )
				return;

			var replace = Emoji.FindEmoji( lookup );
			if ( replace == null )
				return;

			var lengthDelta = replace.Length - lookup.Length;

			Text = Text.Replace( lookup, replace );
			CaretPos += lengthDelta;
		}

		public virtual void OnValueChanged()
		{
			UpdateDataBind();
			UpdateAutoComplete();
		}

		public override void Tick()
		{
			base.Tick();

			PlaceholderLabel?.SetClass( "hidden", !string.IsNullOrEmpty( Text ) );
		}
	}

	namespace Construct
	{
		public static class TextEntryConstructor
		{
			public static TextEntry TextEntry( this PanelCreator self, string text )
			{
				var control = self.panel.AddChild<TextEntry>();
				control.Text = text;

				return control;
			}
		}
	}

}
