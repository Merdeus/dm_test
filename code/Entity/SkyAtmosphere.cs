using System;
namespace Sandbox
{
	/// <summary>
	/// Fancy dynamic sky
	/// </summary>
    [ClassLibrary( "env_sky_atmosphere" )]
	public partial class AtmosphereSky : Sky
	{
		public AtmosphereSky() : base()
		{
			SkyMaterial = Material.Load( "materials/sky/atmosphere_sky.vmat" );
			SkyObject = new SkyboxObject( SkyMaterial );
		}
	}
}
