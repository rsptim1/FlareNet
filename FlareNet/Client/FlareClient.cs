using ENet;

namespace FlareNet.Client
{
	public class FlareClient : IClient
	{
		internal Peer Peer { get; set; }
		
		public ulong TotalDataIn => Peer.BytesReceived;

		public ulong TotalDataOut => Peer.BytesSent;

		public uint Id => Peer.ID;

		public uint Ping => Peer.RoundTripTime;

		public PeerState ClientState => Peer.State;

		public string IpAddress => Peer.IP;

		public ushort Port => Peer.Port;

		public bool IsConnected => Peer.State == PeerState.Connected || Peer.State == PeerState.ConnectionSucceeded;

		public FlareClient(Peer peer)
		{
			Peer = peer;
		}

		public FlareClient() { }

		internal virtual void Update()
		{
		}

		public virtual void SendMessage(Message message, byte channel)
		{
			// Create and send packet
			Packet packet = default;
			packet.Create(message.GetBufferArray(), PacketFlags.Reliable);
			Peer.Send(channel, ref packet);
		}

		public virtual void Disconnect()
		{
			Peer.DisconnectNow(0);
		}

		public void Send(byte channel, Packet packet)
		{
			Peer.Send(channel, ref packet);
		}
	}
}
