using System;

namespace Sandbox
{
    [ClassLibrary( "ambient_generic" )]
	public partial class AmbientGeneric : Entity
	{
		[Flags]
		public enum Flags
		{
			Everywhere = 1,
			StartSilent = 16,
			NotLooping = 32,
		}

		[HammerProp( "message" )]
		public string SoundName { get; set; }

		[HammerProp( "volstart" )]
		public float VolumeStart { get; set; } = 1.0f;

		Sound PlayingSound;

		[Input]
		protected void PlaySound()
		{
			PlayingSound = Sound.FromWorld( SoundName, WorldPos )
									.SetVolume( VolumeStart );
		}

		[Input]
		protected void StopSound()
		{
			PlayingSound.Stop();
			PlayingSound = default;
		}

		[Input]
		protected void ToggleSound()
		{
			
		}

	}
}
