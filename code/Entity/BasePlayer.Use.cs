using Sandbox.UI;

namespace Sandbox
{
	public partial class BasePlayer 
	{
		//
		// I don't think any of this will work long term but it's
		// here for now.
		//


		public Entity Using { get; protected set; }

		/// <summary>
		/// This should be called somewhere in your player's tick to allow them to use entities
		/// </summary>
		protected virtual void TickPlayerUse()
		{
			// This is serverside only
			if ( !Host.IsServer ) return;

			// Turn prediction off
			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.Use ) )
				{
					Using = FindUsable();

					if ( Using == null )
					{
						UseFail();
						return;
					}
				}

				if ( !Input.Down( InputButton.Use ) )
				{
					StopUsing();
					return;
				}

				if ( !Using.IsValid() )
					return;

				// If we move too far away or something we should probably ClearUse()?

				//
				// If use returns true then we can keep using it
				//
				if ( Using is IUse use && use.OnUse( this ) )
					return;

				StopUsing();
			}
		}

		/// <summary>
		/// Player tried to use something but there was nothing there.
		/// Tradition is to give a dissapointed boop.
		/// </summary>
		protected virtual void UseFail()
		{
			PlaySound( "player_use_fail" );
		}

		/// <summary>
		/// If we're using an entity, stop using it
		/// </summary>
		protected virtual void StopUsing()
		{
			Using = null;
		}

		/// <summary>
		/// Find a usable entity for this player to use
		/// </summary>
		protected virtual Entity FindUsable()
		{
			var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 80 )
				.Radius( 2 )
				.Ignore( this )
				.Run();

			if ( tr.Entity == null ) return null;
			if ( tr.Entity is not IUse use ) return null;
			if ( !use.IsUsable( this ) ) return null;

			return tr.Entity;
		}
	}

}
