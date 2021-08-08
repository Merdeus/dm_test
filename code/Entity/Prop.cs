using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox
{
	/// <summary>
	/// Your standard prop
	/// </summary>
	[ClassLibrary( "prop_physics" )]
	public partial class Prop : ModelEntity
	{
		[HammerProp]
		protected int CollisionGroupOverride { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			MoveType = MoveType.Physics;
			CollisionGroup = CollisionGroup.Interactive;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			// TODO CollisionGroupOverride https://files.facepunch.com/garry/42bed1e3-a031-43bb-9a46-9576b7b04dc5.png
		}

		public override void OnNewModel( Model model )
		{
			base.OnNewModel( model );

			if ( IsServer )
			{
				UpdatePropData( model );
			}
		}

		protected virtual void UpdatePropData( Model model )
		{
			Host.AssertServer();

			var propInfo = model.GetPropData();
			Health = (int) propInfo.Health;

			//
			// If health is unset, set it to -1 - which means it cannot be destroyed
			//
			if ( Health <= 0 )
				Health = -1;
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( info.Body.IsValid() )
			{
				info.Body.ApplyImpulseAt( info.Position, info.Force * 100 );
			}

			base.TakeDamage( info );
		}

		public override void OnKilled()
		{
			Breakables.Break( this );

			base.OnKilled();
		}

		protected override void OnRemove()
		{
			base.OnDestroy();

			Unweld( true );
		}
	}
}
