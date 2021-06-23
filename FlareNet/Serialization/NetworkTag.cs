using ENet;
using System;

namespace FlareNet
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
	public sealed class NetworkTagAttribute : Attribute
	{
		public readonly ushort Value;
		private readonly PacketFlags packetOptions;

		public NetworkTagAttribute(ushort tag, PacketOptions options = PacketOptions.Reliable)
		{
			Value = tag;
			packetOptions = (PacketFlags)options;
		}

		internal static NetworkTagAttribute GetTag(Type t)
		{
			return (NetworkTagAttribute)GetCustomAttribute(t, typeof(NetworkTagAttribute));
		}

		internal PacketFlags PacketFlags => packetOptions;
	}

	internal static class NetworkTags
	{
		internal const ushort ClientConnected = 65500;
		internal const ushort ClientDisconnected = 65501;
		internal const ushort IdAssignment = 65502;
		internal const ushort ClientAssigned = 65503;
	}
}
