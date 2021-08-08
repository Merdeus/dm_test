using Sandbox.UI;
using System.Collections.Generic;

namespace Sandbox
{
	/// <summary>
	/// A common base we can use for weapons so we don't have to implement the logic over and over
	/// again.
	/// </summary>
	public class BaseViewModel : AnimEntity
	{
		public static List<BaseViewModel> AllViewModels = new List<BaseViewModel>();

		public float FieldOfView { get; set; } = 65.0f;

		public BaseViewModel()
		{
			AllViewModels.Add( this );
		}

		public override void Spawn()
		{
			base.Spawn();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			AllViewModels.Remove( this );
		}

		public override void OnNewModel( Model model )
		{
			base.OnNewModel( model );

			//
			// TODO - read FOV from model data?
			//

		}


		public virtual void UpdateCamera( Camera camera )
		{
			WorldPos = camera.Pos;
			WorldRot = camera.Rot;

			camera.ViewModelFieldOfView = FieldOfView;
			//camera.ViewModelFieldOfView = 60.0f + System.MathF.Sin( Time.Now ) * 40;
		}

		public static void PostCameraSetup( Camera camera )
		{
			foreach( var vm in AllViewModels )
			{
				vm.UpdateCamera( camera );
			}
		}
	}
}
