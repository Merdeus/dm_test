using Sandbox;
using Sandbox.UI;

namespace Sandbox.ScreenShake
{
	public class Perlin : CameraModifier
	{
		float Length;
		float Speed;
		float Size;
		float RotationAmount;
		float NoiseZ;

		TimeSince lifeTime = 0;
		float pos = 0;

		public Perlin( float length = 1.0f, float speed = 1.0f, float size = 1.0f, float rotation = 0.6f )
		{
			Length = length;
			Speed = speed;
			Size = size;
			RotationAmount = rotation;
			NoiseZ = Rand.Float( -10000, 10000 );

			pos = Rand.Float( 0, 100000 );
		}

		public override bool Update( Camera cam )
		{
			var delta = ((float)lifeTime).LerpInverse( 0, Length, true );
			delta = Easing.EaseOut( delta );
			var invdelta = 1 - delta;

			pos += Time.Delta * 10 * invdelta * Speed;

			float x = Noise.Perlin( pos, 0, NoiseZ );
			float y = Noise.Perlin( pos, 3.0f, NoiseZ );

			cam.Pos += (cam.Rot.Right * x + cam.Rot.Up * y) * invdelta * Size;
			cam.Rot *= Rotation.FromAxis( Vector3.Up, x * Size * invdelta * RotationAmount );
			cam.Rot *= Rotation.FromAxis( Vector3.Right, y * Size * invdelta * RotationAmount );

			return lifeTime < Length;
		}
	}
}
