using ENet;

namespace FlareNet
{
	public abstract class FlareClientBase : IClient
	{
		internal Peer Peer { get; set; }

		/// <summary>
		/// The total bytes recieved.
		/// </summary>
		public virtual ulong TotalDataIn => Peer.BytesReceived;

		/// <summary>
		/// The total bytes sent.
		/// </summary>
		public virtual ulong TotalDataOut => Peer.BytesSent;

		/// <summary>
		/// The ID of this client.
		/// </summary>
		public uint Id { get; internal set; }

		/// <summary>
		/// The round trip time.
		/// </summary>
		public uint Ping => Peer.RoundTripTime;

		/// <summary>
		/// The IP of this client.
		/// </summary>
		public virtual string IpAddress => Peer.IP;

		/// <summary>
		/// The port used.
		/// </summary>
		public virtual ushort Port => Peer.Port;

		public abstract void Disconnect();
	}
}
