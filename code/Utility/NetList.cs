using Sandbox;
using System;
using System.Collections.Generic;

namespace Sandbox
{
	/// <summary>
	/// Like an array, but not really
	/// </summary>
	public class NetList<T> : NetworkClass
	{
		internal Dictionary<int, T> Values = new();

		// TODO - Iteration

		/// <summary>
		/// Get a value
		/// </summary>
		public T Get( int i, T def = default )
		{
			if ( Values.TryGetValue( i, out var val ) )
				return val;

			return def;
		}

		/// <summary>
		/// Set a value
		/// </summary>
		public bool Set( int i, T value )
		{
			if ( Values.TryGetValue( i, out var current ) && object.Equals( current, value ) )
				return false;

			Values[i] = value;
			NetworkDirty( "Values", NetVarGroup.Net ); // TODO - .Net is wrong here
			return true;
		}

		/// <summary>
		/// Get using an emum
		/// </summary>
		public T Get<U>( U i, T def = default ) where U : System.Enum
		{
			return Get( Convert.ToInt32( i ), def );
		}

		/// <summary>
		/// Set using an emum
		/// </summary>
		public bool Set<U>( U i, T value ) where U : System.Enum
		{
			return Set( Convert.ToInt32( i ), value );
		}

		/// <summary>
		/// Wipe all values
		/// </summary>
		public void Clear()
		{
			Values.Clear();
			NetworkDirty( "Values", NetVarGroup.Net ); // TODO - .Net is wrong here
		}


		public override bool NetWrite( NetWrite write )
		{
			base.NetWrite( write );

			// Amount
			write.Write<short>( (short)Values.Count );

			// Key, Value
			foreach ( var e in Values )
			{
				write.Write<short>( (short)e.Key );
				write.Write( e.Value );
			}

			return true;
		}

		public override bool NetRead( NetRead read )
		{
			base.NetRead( read );

			// Amount
			int count = read.Read<short>();

			Clear();

			// Key, Value
			for ( int i = 0; i < count; i++ )
			{
				Values[read.Read<short>()] = read.Read<T>();
			}

			return true;
		}
	}
}