using Sandbox.UI;
using System.Collections.Generic;

namespace Sandbox
{
	/// <summary>
	/// A common base we can use for weapons so we don't have to implement the logic over and over
	/// again.
	/// </summary>
	public partial class BaseWeapon : BaseCarriable, IPlayerControllable
	{
		public virtual float PrimaryRate => 5.0f; 
		public virtual float SecondaryRate => 15.0f;

		public override void Spawn()
		{
			base.Spawn();

			CollisionGroup = CollisionGroup.Weapon; // so players touch it as a trigger but not as a solid
			SetInteractsAs( CollisionLayer.Debris ); // so player movement doesn't walk into it
		}

		[NetPredicted]
		public TimeSince TimeSincePrimaryAttack { get; set; }

		[NetPredicted]
		public TimeSince TimeSinceSecondaryAttack { get; set; }

		public virtual void OnPlayerControlTick( Player owner )
		{
			if ( owner.Input.Down( InputButton.Reload ) )
			{
				Reload( owner );
			}

			if ( CanPrimaryAttack( owner ) )
			{
				TimeSincePrimaryAttack = 0;
				AttackPrimary( owner );
			}

			if ( CanSecondaryAttack( owner ) )
			{
				TimeSinceSecondaryAttack = 0;
				AttackSecondary( owner );
			}
		}

		public virtual void Reload( Player owner )
		{

		}
		public virtual bool CanPrimaryAttack( Player owner )
		{
			if ( !owner.Input.Down( InputButton.Attack1 ) ) return false;

			var rate = PrimaryRate;
			if ( rate <= 0 ) return true;

			return TimeSincePrimaryAttack > (1 / rate);
		}

		public virtual void AttackPrimary( Player owner )
		{

		}

		public virtual bool CanSecondaryAttack( Player owner )
		{
			if ( !owner.Input.Down( InputButton.Attack2 ) ) return false;

			var rate = SecondaryRate;
			if ( rate <= 0 ) return true;

			return TimeSinceSecondaryAttack > (1 / rate);
		}

		public virtual void AttackSecondary( Player owner )
		{

		}

		/// <summary>
		/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
		/// hits, like if you're going through layers or ricocet'ing or something.
		/// </summary>
		public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
		{
			bool InWater = Physics.TestPointContents( start, CollisionLayer.Water );

			var tr = Trace.Ray( start, end )
					.UseHitboxes()
					.HitLayer( CollisionLayer.Water, !InWater )
					.Ignore( Owner )
					.Ignore( this )
					.Size( radius )
					.Run();

			yield return tr;

			//
			// Another trace, bullet going through thin material, penetrating water surface?
			//
		}
	}
}
