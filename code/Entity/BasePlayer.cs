using Sandbox.UI;

namespace Sandbox
{
	/// <summary>
	/// This is what you should derive your player from. This base exists in addon code
	/// so we can take advantage of codegen for replication. The side effect is that we
	/// can put stuff in here that we don't need to access from the engine - which gives
	/// more transparency to our code.
	/// </summary>
	public partial class BasePlayer : Player
	{
		/// <summary>
		/// The PlayerController takes player input and moves the player. This needs
		/// to match between client and server. The client moves the local player and
		/// then checks that when the server moves the player, everything is the same.
		/// This is called prediction. If it doesn't match the player resets everything
		/// to what the server did, that's a prediction error.
		/// You should really never manually set this on the client - it's replicated so
		/// that setting the class on the server will automatically network and set it 
		/// on the client.
		/// </summary>
		[NetPredicted]
		public PlayerController Controller { get; set; }

		/// <summary>
		/// This is used for noclip mode
		/// </summary>
		[NetPredicted]
		public PlayerController DevController { get; set; }

		/// <summary>
		/// Return the controller to use. Remember any logic you use here needs to match
		/// on both client and server. This is called as an accessor every tick.. so maybe
		/// avoid creating new classes here or you're gonna be making a ton of garbage!
		/// </summary>
		public override PlayerController GetActiveController()
		{
			if ( DevController != null ) return DevController;

			return Controller;
		}


		/// <summary>
		/// The player animator is responsible for positioning/rotating the player and 
		/// interacting with the animation graph.
		/// </summary>
		[NetPredicted]
		public PlayerAnimator Animator { get; set; }

		/// <summary>
		/// Return the controller to use. Remember any logic you use here needs to match
		/// on both client and server. This is called as an accessor every tick.. so maybe
		/// avoid creating new classes here or you're gonna be making a ton of garbage!
		/// </summary>
		public override PlayerAnimator GetActiveAnimator() => Animator;

		/// <summary>
		/// The current camera to use. You should set this on the server and the results will be
		/// networked down to the client.
		/// </summary>
		[NetPredicted]
		public Camera Camera { get; set; }

		/// <summary>
		/// This allows developers to override the default camera without disturbing it.
		/// </summary>
		[NetPredicted]
		public Camera DevCamera { get; set; }

		/// <summary>
		/// Returns the camera to use. By default if DevCamera is set we return that, else Camera.
		/// </summary>
		public override Camera GetActiveCamera()
		{
			if ( DevCamera != null ) return DevCamera;

			return Camera;
		}


		TimeSince timeSinceDied;

		/// <summary>
		/// Called every tick to simulate the player. This is called on the
		/// client as well as the server (for prediction). So be careful!
		/// </summary>
		protected override void Tick()
		{
			if ( LifeState == LifeState.Dead )
			{
				if ( timeSinceDied > 3 && IsServer )
				{
					Respawn();
				}
			}

			TickActiveChild();
		}

		/// <summary>
		/// Called during player tick to run a tick on the active item. This allows
		/// for example, a gun to be fired (while using prediction etc).
		/// </summary>
		protected virtual void TickActiveChild()
		{
			if ( ActiveChild is IPlayerControllable playerController )
			{
				playerController.OnPlayerControlTick( this );
			}
		}

		/// <summary>
		/// Called once the player's health reaches 0
		/// </summary>
		public override void OnKilled()
		{
			GameBase.Current?.PlayerKilled( this );

			timeSinceDied = 0;
			Deaths++;
			LifeState = LifeState.Dead;
			StopUsing();

			if ( LastAttacker is BasePlayer attackingPlayer )
			{
				attackingPlayer.Kills++;
			}

			// Drop weapon

			// m_pl.deadflag = true;

			//	RumbleEffect( RUMBLE_STOP_ALL, 0, RUMBLE_FLAGS_NONE );

			// Holster Weapon

			// Set health 0

			//SetAnimation( PLAYER_DIE );
			//SetViewOffset( VEC_DEAD_VIEWHEIGHT );
			//SetViewOffset( vec3_origin );

			// CollisionProp()->DisableAllCollisions();

			// 	SetMoveType( MOVETYPE_FLYGRAVITY );
			// SetGroundEntity( NULL );

			// m_flDeathTime = gpGlobals->curtime();

			// only count alive players
			// ClearLastKnownArea();

			//
			// clear the deceased's sound channels.(may have been firing or reloading when killed)
			// EmitSound( "BaseCombatCharacter.StopWeaponSounds" );

			// BecomeRagdoll( info, forceVector );
		}

		/// <summary>
		/// Sets LifeState to Alive, Health to Max, nulls velocity, and calls Gamemode.PlayerRespawn
		/// </summary>
		public override void Respawn()
		{
			Host.AssertServer();

			// Copy Corpse
			LifeState = LifeState.Alive;
			Health = 100;
			Velocity = Vector3.Zero;
			UpdatePhysicsHull();
			ResetInterpolation();

			GameBase.Current?.PlayerRespawn( this );
		}

		/// <summary>
		/// Called from the gamemode, clientside only.
		/// </summary>
		public override void BuildInput( ClientInput input )
		{
			VoiceRecord = input.Down( InputButton.Voice );

			GetActiveCamera()?.BuildInput( input );

			if ( input.StopProcessing )
				return;

			GetActiveController()?.BuildInput( input );

			if ( input.StopProcessing )
				return;

			GetActiveAnimator()?.BuildInput( input );

			if ( input.StopProcessing )
				return;

			if ( ActiveChild is IPlayerInput ipc )
			{
				ipc.BuildInput( input );
			}
		}

		/// <summary>
		/// A generic corpse entity
		/// </summary>
		public Entity Corpse { get; set; }


		/// <summary>
		/// Called after the camera setup logic has run. Allow the player to 
		/// do stuff to the camera, or using the camera. Such as positioning entities 
		/// relative to it, like viewmodels etc.
		/// </summary>
		public override void PostCameraSetup( Camera camera )
		{
			Host.AssertClient();

			if ( ActiveChild is IPlayerCamera cam ) 
			{
				cam.ModifyCamera( camera );
			}
		}


		/// <summary>
		/// Number of deaths. We use the PlayerInfo system to store these variables because
		/// we want them to be available to all clients - not just the ones that can see us right now.
		/// </summary>
		public virtual int Deaths
		{
			get => GetScore<int>( "deaths" );
			set => SetScore( "deaths", value );
		}

		/// <summary>
		/// Number of kills. We use the PlayerInfo system to store these variables because
		/// we want them to be available to all clients - not just the ones that can see us right now.
		/// </summary>
		public virtual int Kills
		{
			get => GetScore<int>( "kills" );
			set => SetScore( "kills", value );
		}


		TimeSince timeSinceLastFootstep = 0;

		/// <summary>
		/// A foostep has arrived!
		/// </summary>
		public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
		{
			if ( LifeState != LifeState.Alive )
				return;

			if ( !IsClient )
				return;

			if ( timeSinceLastFootstep < 0.1f )
				return;

			timeSinceLastFootstep = 0;

			//DebugOverlay.Box( 1, pos, -1, 1, Color.Red );
			//DebugOverlay.Text( pos, $"{volume}", Color.White, 5 );

			var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
				.Radius( 1 )
				.Ignore( this )
				.Run();

			if ( !tr.Hit ) return;

			tr.Surface.DoFootstep( this, tr, foot, volume );
		}

	}

}
