using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{
	public class BaseNameTag : Panel
	{
		public Label NameLabel;
		public Image Avatar;

		Player player;

		public BaseNameTag( Player player )
		{
			this.player = player;

			NameLabel = Add.Label( player.Name );
			Avatar = Add.Image( $"avatar:{player.SteamId}" );
		}

		public virtual void UpdateFromPlayer( Player player )
		{
			// Nothing to do unless we're showing health and shit
		}
	}

	public class NameTags : Panel
	{
		Dictionary<Player, BaseNameTag> ActiveTags = new Dictionary<Player, BaseNameTag>();

		public float MaxDrawDistance = 400;
		public int MaxTagsToShow = 5;

		public NameTags()
		{
			StyleSheet = StyleSheet.FromFile( "/ui/nametags/NameTags.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			var deleteList = new List<Player>();
			deleteList.AddRange( ActiveTags.Keys );

			int count = 0;
			foreach ( var player in Player.All.OrderBy( x => Vector3.DistanceBetween( x.EyePos, Camera.LastPos ) ) )
			{
				if ( UpdateNameTag( player ) )
				{
					deleteList.Remove( player );
					count++;
				}

				if ( count >= MaxTagsToShow )
					break;
			}

			foreach( var player in deleteList )
			{
				ActiveTags[player].Delete();
				ActiveTags.Remove( player );
			}
		}

		public virtual BaseNameTag CreateNameTag( Player player )
		{
			var tag = new BaseNameTag( player );
			tag.Parent = this;
			return tag;
		}

		public bool UpdateNameTag( Player player )
		{
			// Don't draw local player
			if ( player.IsLocalPlayer )
				return false;

			if ( player.LifeState != LifeState.Alive )
				return false;

			//
			// Where we putting the label, in world coords
			//
			var head = player.GetAttachment( "hat" );
			if ( head.Pos == Vector3.Zero )
				head.Pos = player.EyePos;

			var labelPos = head.Pos + head.Rot.Up * 5;


			//
			// Are we too far away?
			//
			float dist = labelPos.Distance( Camera.LastPos );
			if ( dist > MaxDrawDistance )
				return false;

			//
			// Are we looking in this direction?
			//
			var lookDir = (labelPos - Camera.LastPos).Normal;
			if ( Camera.LastRot.Forward.Dot( lookDir ) < 0.5 )
				return false;

			// TODO - can we see them


			MaxDrawDistance = 400;

			// Max Draw Distance


			var alpha = dist.LerpInverse( MaxDrawDistance, MaxDrawDistance * 0.1f, true );

			// If I understood this I'd make it proper function
			var objectSize = 0.05f / dist / (2.0f * MathF.Tan( (Camera.LastFieldOfView / 2.0f).DegreeToRadian() )) * 1500.0f;

			objectSize = objectSize.Clamp( 0.05f, 1.0f );

			if ( !ActiveTags.TryGetValue( player, out var tag ) )
			{
				tag = CreateNameTag( player );
				ActiveTags[player] = tag;
			}

			tag.UpdateFromPlayer( player );

			var screenPos = labelPos.ToScreen();

			tag.Style.Left = Length.Fraction( screenPos.x );
			tag.Style.Top = Length.Fraction( screenPos.y );
			tag.Style.Opacity = alpha;

			var transform = new PanelTransform();
			transform.AddTranslateY( Length.Fraction( -1.0f ) );
			transform.AddScale( objectSize );
			transform.AddTranslateX( Length.Fraction( -0.5f ) );

			tag.Style.Transform = transform;
			tag.Style.Dirty();

			return true;
		}
	}
}