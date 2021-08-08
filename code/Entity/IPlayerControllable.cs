using Sandbox.UI;

namespace Sandbox
{
	public interface IPlayerControllable
	{
		void OnPlayerControlTick( Player owner );
	}

	public interface IPlayerInput
	{
		void BuildInput( ClientInput owner );
	}

	public interface IPlayerCamera
	{
		void ModifyCamera( Camera owner );
	}
}
