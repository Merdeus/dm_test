
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Sandbox.UI
{
	public partial class KillFeed : Panel
	{
		public static KillFeed Current;

		public KillFeed()
		{
			Current = this;

			StyleSheet = StyleSheet.FromFile( "/ui/killfeed/KillFeed.scss" );
		}

		[ClientCmd( "killfeed_add", CanBeCalledFromServer = true )]
		public static void AddEntry( ulong lsteamid, string left, ulong rsteamid, string right, string method )
		{
			if ( Current == null )
				return;

			var e = Current.AddChild<KillFeedEntry>();

			e.Left.Text = left;
			e.Left.SetClass( "me", lsteamid == (Player.Local?.SteamId) );

			e.Method.Text = method;

			e.Right.Text = right;
			e.Right.SetClass( "me", rsteamid == (Player.Local?.SteamId) );
		}
	}
}