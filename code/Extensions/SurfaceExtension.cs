using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sandbox
{
	/// <summary>
	/// Extensions for Surfaces
	/// </summary>
	public static partial class SurfaceExtension
	{
		/// <summary>
		/// Create a particle effect and play an impact sound for this surface being hit by a bullet
		/// </summary>
		public static Particles DoBulletImpact( this Surface self, TraceResult tr )
		{
			//
			// No effects on resimulate
			//
			if ( !Prediction.FirstTime ) 
				return null;

			//
			// Drop a decal
			//
			var decalPath = Rand.FromArray( self.ImpactEffects.BulletDecal );
			if ( decalPath != null )
			{
				if ( DecalDefinition.ByPath.TryGetValue( decalPath, out var decal ) )
				{
					decal.PlaceUsingTrace( tr );
				}
			}

			//
			// Make an impact sound
			//
			if ( !string.IsNullOrWhiteSpace( self.Sounds.Bullet ) )
			{
				Sound.FromWorld( self.Sounds.Bullet, tr.EndPos );
			}

			//
			// Get us a particle effect
			//
			string particleName = Rand.FromArray( self.ImpactEffects.Bullet );
			if ( particleName  == null ) particleName = Rand.FromArray( self.ImpactEffects.Regular );

			if ( particleName != null )
			{
				var ps = Particles.Create( particleName, tr.EndPos );
				ps.SetForward( 0, tr.Normal );

				return ps;
			}



			return default;
		}



		/// <summary>
		/// Create a footstep effect
		/// </summary>
		public static void DoFootstep( this Surface self, Entity ent, TraceResult tr, int foot, float volume )
		{
			var sound = foot == 0 ? self.Sounds.FootLeft : self.Sounds.FootRight;

			if ( !string.IsNullOrWhiteSpace( sound ) )
			{
				ent.PlaySound( sound )
					.SetPosition( tr.EndPos )
					.SetVolume( volume );
			}
		}
	}

}
