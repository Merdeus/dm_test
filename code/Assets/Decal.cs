using NativeEngine;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox
{
	[ClassLibrary( "decal" )]
	public partial class DecalDefinition : Asset
	{
		/// <summary>
		/// A dictionary of 
		/// </summary>
		public static Dictionary<string, DecalDefinition> ByPath = new();

		public class DecalEntry
		{
			public Material Material { get; set; }
			public RangedFloat Width { get; set; }
			public RangedFloat Height { get; set; }
			public bool KeepAspect { get; set; }
			public RangedFloat Depth { get; set; }
			public RangedFloat Rotation { get; set; }
		}

		public DecalEntry[] Decals { get; set; }

		protected override void PostLoad()
		{
			ByPath[Path] = this;
		}

		/// <summary>
		/// Place this decal somewhere
		/// </summary>
		public void PlaceUsingTrace( TraceResult tr )
		{
			var entry = Rand.FromArray( Decals );
			if ( entry == null )
				return;

			var w = entry.Width.GetValue();
			var h = entry.Height.GetValue();
			var d = entry.Depth.GetValue();
			var r = entry.Rotation.GetValue();

			if ( entry.KeepAspect )
			{
				h = w * (entry.Width.x / entry.Height.x);
			}

			var rot = Rotation.LookAt( tr.Normal ) * Rotation.FromAxis( Vector3.Forward, r );

			var pos = tr.EndPos;

			if ( tr.Entity is ModelEntity me )
			{
				var tx = me.GetBoneTransform( tr.Bone );
				pos = tx.PointToLocal( pos );
				rot = tx.RotationToLocal( rot );
			}

			Place( entry.Material, tr.Entity, tr.Bone, pos, rot, new Vector3( w, h, d ) );
		}

		[ClientRpc]
		public static void Place( Material material, Entity ent, int bone, Vector3 localpos, Rotation localrot, Vector3 scale )
		{
			if ( ent is ModelEntity me )
			{
				var tx = me.GetBoneTransform( bone );
				var pos = tx.PointToWorld( localpos );
				var rot = tx.RotationToWorld( localrot );

				Sandbox.Decals.Place( material, ent, bone, pos, scale, rot );
			}
			else
			{
				Sandbox.Decals.Place( material, ent, bone, localpos, scale, localrot ); 
			}
		}
	}
}
