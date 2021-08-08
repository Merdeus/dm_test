using Sandbox;
using Sandbox.UI;
using System.Linq;

public class VoiceList : Panel
{
	public VoiceList()
	{
		StyleSheet = StyleSheet.FromFile( "/UI/VoiceChat/VoiceList.scss" );

		Refresh();
	}

	private void Refresh()
	{
		var entries = ChildrenOfType<VoiceEntry>();

		// This is probably slow and there's a better way of doing this
		foreach ( var player in Player.All.Where( x => !entries.Any( y => y.Player == x ) ) )
		{
			if ( !player.VoiceUsed )
				continue;

			if ( player.LastVoiceTime + 0.5f < Time.Now )
				continue;

			var friendPanel = AddChild<VoiceEntry>();
			friendPanel.Update( player );
		}
	}

	public override void Tick()
	{
		base.Tick();

		Refresh();
	}
}
