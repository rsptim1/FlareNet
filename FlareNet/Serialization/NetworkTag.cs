using System;

namespace FlareNet
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
	public sealed class NetworkTagAttribute : Attribute
	{
		public readonly ushort Tag;

		public NetworkTagAttribute(ushort tag)
		{
			Tag = tag;
		}
	}
}
