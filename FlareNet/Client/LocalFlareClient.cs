using ENet;
using FlareNet.Debug;

namespace FlareNet.Client
{
	internal class LocalFlareClient : FlareClient
	{
		/// <summary>
		/// Buffer for the incoming packets to copy their data to.
		/// </summary> Note: value is roughly twice maximum safe packet size.
		protected readonly byte[] receivePacketBuffer = new byte[1024];

		protected Host Host { get; set; }
		protected Address Address { get; set; }

		internal LocalFlareClient(string ip, ushort port)
		{
			Library.Initialize();

			// Setup the address
			Address = new Address() { Port = port };
			Address.SetHost(ip);

			// Setup the host
			Host = new Host();
			Host.Create(1, 2);

			// Setup the peer
			Peer = Host.Connect(Address, 2);
		}

		protected LocalFlareClient() { }

		internal override void Update()
		{
			// Handle the next network event
			if (Host.Service(0, out var networkEvent) > 0)
			{
				switch (networkEvent.Type)
				{
					case EventType.Connect:
						OnConnect(networkEvent);
						break;
					case EventType.Disconnect:
						OnDisconnect(networkEvent);
						break;
					case EventType.Timeout:
						OnTimeout(networkEvent);
						break;
					case EventType.Receive:
						OnMessageReceived(networkEvent);
						break;
					case EventType.None:
						break;
				}
			}
		}

		protected virtual void OnConnect(Event e)
		{
			NetworkLogger.Log(NetworkLogEvent.ClientConnect);
		}

		protected virtual void OnDisconnect(Event e)
		{
			NetworkLogger.Log(NetworkLogEvent.ClientDisconnect);
		}

		protected virtual void OnTimeout(Event e)
		{
			NetworkLogger.Log(NetworkLogEvent.ClientTimeout);
		}

		protected virtual void OnMessageReceived(Event e)
		{
			NetworkLogger.Log($"Packet from server on Channel [{e.ChannelID}] with Length [{e.Packet.Length}]");

			// Deserialize message data
			e.Packet.CopyTo(receivePacketBuffer);
			Message message = new Message(receivePacketBuffer, e.Packet.Length + 4);

			// Process the message and invoke any callback
			MessageHandler.ProcessMessage(message, null);
		}

		public override void Disconnect()
		{
			Peer.Disconnect(0);

			if (Host != null)
			{
				Host.Flush();
				Host.Dispose();
			}

			Library.Deinitialize();
		}
	}
}
