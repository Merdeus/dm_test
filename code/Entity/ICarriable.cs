using Sandbox.UI;

namespace Sandbox
{
	/// <summary>
	/// An entity implementing this interface us an indication that the 
	/// entity can be carried by a player in their inventory. 
	/// </summary>
	public interface ICarriable
	{
		/// <summary>
		/// Return false if this entity objects to being picked up by this entity
		/// </summary>
		bool CanCarry( Entity carrier );

		/// <summary>
		/// Allow the entity to do what it wants when it's added to the inventory.
		/// Default behaviour is to add the target entity as a parent and stop moving.
		/// </summary>
		void OnCarryStart( Entity parentEntity );

		/// <summary>
		/// Allow the entity to do what it wants when it's removed from the inventory
		/// </summary>
		void OnCarryDrop( Entity parentEntity );


		// TODO: Do we want to return shit like name, description, icon? I think not?
		//		 My feeling is that that presentation stuff should be another interface
		//		 Maybe a more generic thing than specializing on inventory items.


		// TODO: Do we want to return slot info? Probably not. Same as above, some games
		//		 Will want more info than we can provide. Weight, shape, count etc, some
		//		 games will just want to return the player slot, some games won't want
		//		 anything.

	}
}
