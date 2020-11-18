using ENet;

namespace FlareNet.Client
{
	public abstract class FlareClientBase : IClient
	{
		internal Peer Peer { get; set; }
		
		/// <summary>
		/// The total bytes recieved.
		/// </summary>
		public ulong TotalDataIn => Peer.BytesReceived;

		/// <summary>
		/// The total bytes sent.
		/// </summary>
		public ulong TotalDataOut => Peer.BytesSent;

		/// <summary>
		/// The ID of this client.
		/// </summary>
		public uint Id => Peer.ID;

		/// <summary>
		/// The round trip time.
		/// </summary>
		public uint Ping => Peer.RoundTripTime;

		/// <summary>
		/// The IP of this client.
		/// </summary>
		public string IpAddress => Peer.IP;

		/// <summary>
		/// The port used.
		/// </summary>
		public ushort Port => Peer.Port;

		public abstract void Disconnect();
	}
}
