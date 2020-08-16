using ENet;
using FlareNet.Client;
using FlareNet.Debug;

namespace FlareNet.Server
{
	internal class FlareServer : LocalFlareClient
	{
		public FlareClientManager ClientManager { get; private set; }

		public ServerConfig Config { get; set; }

		public FlareServer(ServerConfig config, ushort port)
		{
			Config = config;
			StartServer(port);
		}

		private void StartServer(ushort port)
		{
			Host = new Host();

			//Create the address for the host to use.
			Address = new Address { Port = port };

			//Initialize the host.
			Host.Create(Address, Config.MaxConnections, 2);
			Host.EnableCompression();


			ClientManager = new FlareClientManager(Config.MaxConnections);
			NetworkLogger.Log(Debug.NetworkLogEvent.ServerStart);
		}

		protected override void OnConnect(NetworkEvent e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] connected from [{peer.IP}]");
			ClientManager.AddClient(new FlareClient(peer));
		}

		protected override void OnDisconnect(NetworkEvent e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] disconnected from [{peer.IP}]");
			ClientManager.RemoveClient(peer.ID);
		}

		protected override void OnTimeout(NetworkEvent e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] timeout from [{peer.IP}]");
			ClientManager.RemoveClient(peer.ID);
		}

		/// <summary>
		/// Called when the server receives a message from a client.
		/// </summary>
		/// <param name="networkEvent"></param>
		protected override void OnMessageReceived(NetworkEvent networkEvent)
		{
			NetworkLogger.Log($"Packet from [{networkEvent.Peer.IP}] ({networkEvent.Peer.ID}) " +
				$"on Channel [{networkEvent.ChannelID}] Length [{networkEvent.Packet.Length}]");

			if (!ClientManager.TryGetClient(networkEvent.Peer.ID, out IClient client))
			{
				NetworkLogger.Log($"Message received from a null client with ID [{networkEvent.Peer.ID}]", LogLevel.Warning);
			}

			// Deserialize message data
			networkEvent.Packet.CopyTo(receivePacketBuffer);
			Message message = new Message(receivePacketBuffer, networkEvent.Packet.Length + 4);

			// Process the message and invoke any callback
			MessageHandler.ProcessMessage(message, client);
		}

		/// <summary>
		/// Send a message to all connected clients.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="sendMode"></param>
		public override void SendMessage(Message message, SendMode sendMode)
		{
			// Create packet and broadcast
			using (Packet packet = default)
			{
				packet.Create(message.GetBufferArray(), sendMode);
				Host.Broadcast((byte)sendMode, packet);
			}
		}

		/// <summary>
		/// Send a message to an array of peers.
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <param name="clients">The clients to send to</param>
		/// <param name="sendMode"></param>
		public void BroadcastMessage(Message message, Peer[] clients, SendMode sendMode)
		{
			// Create packet and send to selected clients
			using (Packet packet = default)
			{
				packet.Create(message.GetBufferArray(), sendMode);
				Host.Broadcast((byte)sendMode, packet, ref clients);
			}
		}

		public override void Disconnect()
		{
			NetworkLogger.Log(NetworkLogEvent.ServerStop);

			foreach (var client in ClientManager.GetAllClients())
			{
				client.Disconnect();
			}

			if (Host != null)
			{
				Host.Flush();
				Host.Dispose();
			}

			ClientManager = null;
		}
	}
}
