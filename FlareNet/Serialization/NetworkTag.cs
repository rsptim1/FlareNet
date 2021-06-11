using System;

namespace FlareNet
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
	public sealed class NetworkTagAttribute : Attribute
	{
		public readonly ushort Value;

		public NetworkTagAttribute(ushort tag)
		{
			Value = tag;
		}

		internal static NetworkTagAttribute GetTag(Type t)
		{
			return (NetworkTagAttribute)GetCustomAttribute(t, typeof(NetworkTagAttribute));
		}
	}
}
