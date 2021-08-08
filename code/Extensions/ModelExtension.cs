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
	/// Extensions for Model
	/// </summary>
	public static partial class ModelExtensions
	{
		/// <summary>
		/// Get a list of break pieces for this model. These are stored in the "break_list" data key.
		/// </summary>
		public static ModelBreakPiece[] GetBreakPieces( this Model self )
		{
			return self.GetData<ModelBreakPiece[]>( "break_list" );
		}

		/// <summary>
		/// Get a list of break pieces for this model. These are stored in the "break_list" data key.
		/// </summary>
		public static ModelPropData GetPropData( this Model self )
		{
			return self.GetData<ModelPropData>( "prop_data" );
		}
	}

	public struct ModelBreakPiece
	{
		[JsonPropertyName( "piece_name" )]
		public string PieceName { get; set; }

		public string Model { get; set; }
		public string Ragdoll { get; set; }
		public Vector3 Offset { get; set; }
		public float FadeTime { get; set; }
		public float FadeMinDist { get; set; }
		public float FadeMaxDist { get; set; }

		[JsonPropertyName( "random_spawn_chance" )]
		public float RandomSpawnChance { get; set; }

		[JsonPropertyName( "is_essential_piece" )]
		public bool IsEssential { get; set; }		
		
		[JsonPropertyName( "collision_group_override" )]
		public string CollisionGroupOverride { get; set; }

		public CollisionGroup GetCollisionGroup()
		{
			if ( CollisionGroupOverride == "debris" ) return CollisionGroup.Debris;
			if ( CollisionGroupOverride == "interactive" ) return CollisionGroup.Interactive;

			return CollisionGroup.Default;
		}

		// collision_group_override
		// name_mode
		// placementbone
		// placementattachment
	}

	/// <summary>
	/// The prop_data field from the model
	/// </summary>
	public struct ModelPropData
	{
		public float Health { get; set; }
	}

}