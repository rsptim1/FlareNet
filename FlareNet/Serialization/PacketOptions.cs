using System;

namespace FlareNet
{
	[Flags]
	public enum PacketOptions
	{
		Unreliable = 0,
		Reliable = 1,
		Instant = 16,
		Unthrottled = 32
	}
}
