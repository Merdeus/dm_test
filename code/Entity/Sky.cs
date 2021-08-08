using System;

namespace Sandbox
{
	/// <summary>
	/// Simple Skybox
	/// </summary>
    //[ClassLibrary( "env_sky" )]
	public partial class Sky : Entity
	{
		// Todo: hammerprop doesn't work with Material handles yet
		[HammerProp]
		public Material skyname { get; set; }
		
		public Material SkyMaterial { get; set; }
		public SkyboxObject SkyObject { get; set; }

		public Sky() : base()
		{
			Transmit = TransmitType.Always;
			SkyMaterial = skyname;

			if( IsClient )
			{
				SkyObject = new SkyboxObject( SkyMaterial );
			}
		}

		public override void Spawn()
		{
		}
	}
}
