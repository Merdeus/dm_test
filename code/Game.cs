using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	/// <summary>
	/// This is the main base game
	/// </summary>
	[ClassLibrary( "base" )]
	public partial class Game : GameBase
	{
		public Game()
		{
			Transmit = TransmitType.Always;
		}

		/// <summary>
		/// A player has just joined the server - create a player entity
		/// </summary>
		public override Player CreatePlayer()
		{
			return Entity.Create<Player>();
		}

		public override void PlayerJoined( Player player )
		{
			Log.Info( $"\"{player.Name}\" has joined the game" );
			ChatBox.AddInformation( Player.All, $"{player.Name} has joined", $"avatar:{player.SteamId}" );

			//
			// Just spawn them straight in
			//
			player.Respawn();
		}

		public override void PlayerDisconnected( Player player, NetworkDisconnectionReason reason )
		{
			Log.Info( $"\"{player.Name}\" has left the game ({reason})" );

			ChatBox.AddInformation( Player.All, $"{player.Name} has left ({reason})", $"avatar:{player.SteamId}" );

			//
			// Just spawn them straight in
			//
			player.Respawn();
		}

		/// <summary>
		/// Called when a player has died, or been killed
		/// </summary>
		public override void PlayerKilled( Player player )
		{
			Log.Info( $"{player.Name} was killed" );

			if ( player.LastAttacker != null )
			{
				if ( player.LastAttacker is Player attackPlayer )
				{
					Sandbox.UI.KillFeed.AddEntry( Player.All, attackPlayer.SteamId, attackPlayer.Name, player.SteamId, player.Name, "killed" );
				}
				else
				{
					Sandbox.UI.KillFeed.AddEntry( Player.All, (ulong)player.LastAttacker.NetworkIdent, player.LastAttacker.ToString(), player.SteamId, player.Name, "killed" );
				}
			}
			else
			{
				Sandbox.UI.KillFeed.AddEntry( Player.All, (ulong)0, "", player.SteamId, player.Name, "died" );
			}


		}

		/// <summary>
		/// Put the player on a spawnpoint
		/// </summary>
		public override void PlayerRespawn( Player player )
		{
			var spawnpoints = Entity.All.Where( x => x.EngineEntityName == "info_player_start" );
			var randomSpawn = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

			if ( randomSpawn == null )
			{
				Log.Warning( "Couldn't find spawnpoint!" );
				return;
			}

			player.Pos = randomSpawn.Pos;
			player.Rot = randomSpawn.Rot;
		}

		/// <summary>
		/// Voice data coming from a player
		/// </summary>
		public override void PlayerVoiceIn( Player speaker, byte[] voiceData )
		{
			foreach ( var receiver in Player.All
				.Where( x => Vector3.DistanceBetween( x.WorldPos, speaker.WorldPos ) < 1000.0f ) )
			{
				OutputPlayerVoice( receiver, speaker, voiceData );
			}
		}

		/// <summary>
		/// Voice data coming from the server
		/// </summary>
		public override void PlayerVoiceOut( Player speaker, byte[] voiceData )
		{
			if ( !Player.VoiceHearSelf && speaker.IsLocalPlayer )
				return;

			speaker.PlayVoice( voiceData );
		}

		/// <summary>
		/// Which camera should we be rendering from?
		/// </summary>
		public override Camera GetActiveCamera()
		{
			return Player.Local?.GetActiveCamera();
		}

		/// <summary>
		/// Player typed kill in the console. Override if you don't want players
		/// to be allowed to kill themselves.
		/// </summary>
		public virtual void DoPlayerSuicide( Player player )
		{
			if ( !player.HasPermission( "suicide" ) )
				return;

			if ( player.LifeState != LifeState.Alive ) 
				return;

			DamageInfo damage = DamageInfo.Generic( 1000.0f )
										  .WithAttacker( player );

			player.TakeDamage( damage );
		}

		/// <summary>
		/// Player typed kill in the console. Override if you don't want players
		/// to be allowed to kill themselves.
		/// </summary>
		public virtual void DoPlayerNoclip( Player player )
		{
			if ( !player.HasPermission( "noclip" ) )
				return;

			// TODO - check can use cheats

			if ( player is BasePlayer basePlayer )
			{
				if ( basePlayer.DevController is NoclipController )
				{
					Log.Info( "Noclip Mode Off" );
					basePlayer.DevController = null;
				}
				else
				{
					Log.Info( "Noclip Mode On" );
					basePlayer.DevController = new NoclipController();
				}
			}
		}

		/// <summary>
		/// The player wants to enable the devcam. Probably shouldn't allow this
		/// unless you're in a sandbox mode or they're a dev.
		/// </summary>
		public virtual void DoPlayerDevCam( Player player )
		{
			if ( !player.HasPermission( "devcam" ) )
				return;

			if ( player is BasePlayer basePlayer )
			{
				if ( basePlayer.DevCamera is DevCamera )
				{
					basePlayer.DevCamera = null;
				}
				else
				{
					basePlayer.DevCamera = new DevCamera();
				}
			}
		}

		/// <summary>
		/// Clientside only. Called every frame to process the input.
		/// The results of this input are encoded into a user command and
		/// passed to the PlayerController both clientside and serverside.
		/// This routine is mainly responsible for taking input from mouse/controller
		/// and building look angles and move direction.
		/// </summary>
		public override void OnInput( ClientInput input )
		{
			var target = Player.Local;
			if ( target != null )
			{
				target.BuildInput( input );
			}
		}

		/// <summary>
		/// Called after the camera setup logic has run. Allow the gamemode to 
		/// do stuff to the camera, or using the camera. Such as positioning entities 
		/// relative to it, like viewmodels etc.
		/// </summary>
		public override void PostCameraSetup( Camera camera )
		{
			var target = Player.Local;
			if ( target != null )
			{
				target.PostCameraSetup( camera );
			}

			//
			// Position any viewmodels
			//
			BaseViewModel.PostCameraSetup( camera );
		}

		/// <summary>
		/// Called right after the level is loaded and all entities are spawned.
		/// </summary>
		public override void PostLevelLoaded()
		{

		}

	}
}
