using ENet;

namespace FlareNet.Client
{
	public abstract class FlareClientBase : IClient
	{
		public Peer Peer { get; internal set; }
		
		public ulong TotalDataIn => Peer.BytesReceived;

		public ulong TotalDataOut => Peer.BytesSent;

		public uint Id => Peer.ID;

		public uint Ping => Peer.RoundTripTime;

		public PeerState ClientState => Peer.State;

		public string IpAddress => Peer.IP;

		public ushort Port => Peer.Port;

		public bool IsConnected => Peer.State == PeerState.Connected || Peer.State == PeerState.ConnectionSucceeded;
		public bool IsUnableToConnect => Peer.State == PeerState.Disconnected;

		public abstract void Disconnect();
	}
}
