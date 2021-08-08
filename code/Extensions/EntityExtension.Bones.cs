using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public static partial class EntityExtensions
	{
		/// <summary>
		/// Copy the bones from the target entity, but at the current entity's position and rotation
		/// </summary>
		public static void CopyBonesFrom( this Entity self, Entity ent )
		{
			CopyBonesFrom( self, ent, self.Pos, self.Rot );
		}

		/// <summary>
		/// Copy the bones from the target entity, but at this position and rotation instead of the target entity's
		/// </summary>
		public static void CopyBonesFrom( this Entity self, Entity ent, Vector3 pos, Rotation rot )
		{
			//
			// This could be a slow way of doing it, if we copy this to the
			// engine we can do it faster. But for now leave it like this because
			// it's a good test on bone read/write.
			//

			if ( self is ModelEntity to )
			{
				if ( ent is ModelEntity me )
				{
					var localPos = me.Pos;
					var localRot = me.Rot.Inverse;
					var bones = Math.Min( to.BoneCount, me.BoneCount );

					for ( int i = 0; i < bones; i++ )
					{
						var tx = me.GetBoneTransform( i );

						tx.Pos = (tx.Pos - localPos) * localRot * rot + pos;
						tx.Rot = rot * (localRot * tx.Rot);

						to.SetBoneTransform( i, tx );
					}
				}
			}
		}


		/// <summary>
		/// Set the velocity of the ragdoll entity by working out the bone positions of from delta seconds ago 
		/// </summary>
		public static void SetRagdollVelocityFrom( this Entity self, Entity fromEnt, float delta = 0.1f, float linearAmount = 1.0f, float angularAmount = 1.0f )
		{
			if ( delta == 0 ) return;

			if ( self is ModelEntity to )
			{
				if ( fromEnt is ModelEntity from )
				{
					var bonesNow = from.ComputeBones( 0.0f );
					var bonesThn = from.ComputeBones( -delta );

					for ( int i = 0; i < from.BoneCount; i++ )
					{
						var body = to.GetBonePhysicsBody( i );
						if ( body == null ) continue;

						//
						// Linear velocity
						//
						if ( linearAmount > 0 )
						{
							var center = body.LocalMassCenter;
							var c0 = bonesThn[i].TransformVector( center );
							var c1 = bonesNow[i].TransformVector( center );
							var vLinearVelocity = (c1 - c0) * (linearAmount / delta);
							body.Velocity = vLinearVelocity;
						}

						//
						// Angular velocity
						//
						if ( angularAmount > 0 )
						{
							var diff = Rotation.Difference( bonesThn[i].Rot, bonesNow[i].Rot );
							body.AngularVelocity = new Vector3( diff.x, diff.y, diff.z ) * (angularAmount / delta);
						}
					}
				}
			}
		}




	}
}
