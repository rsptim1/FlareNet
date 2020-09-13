using ENet;
using FlareNet.Client;
using FlareNet.Debug;

namespace FlareNet.Server
{
	public class FlareServer : FlareClient
	{
		internal FlareClientManager ClientManager { get; private set; }

		public ServerConfig Config { get; set; }

		public FlareServer(ushort port, ServerConfig config)
		{
			Config = config;
			FlareNetwork.InitializeLibrary();
			StartServer(port);
		}

		private void StartServer(ushort port)
		{
			Host = new Host();

			//Create the address for the host to use.
			Address = new Address { Port = port };

			//Initialize the host.
			Host.Create(Address, Config.MaxConnections);

			ClientManager = new FlareClientManager(Config.MaxConnections);
			FlareNetwork.ClientUpdate += Update;
			NetworkLogger.Log(Debug.NetworkLogEvent.ServerStart);
		}

		#region Updates

		protected override void OnConnect(Event e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] connected from [{peer.IP}]");
			ClientManager.AddClient(new FlareClientShell(peer));
		}

		protected override void OnDisconnect(Event e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] disconnected from [{peer.IP}]");
			ClientManager.RemoveClient(peer.ID);
		}

		protected override void OnTimeout(Event e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] timeout from [{peer.IP}]");
			ClientManager.RemoveClient(peer.ID);
		}

		/// <summary>
		/// Called when the server receives a message from a client.
		/// </summary>
		/// <param name="networkEvent"></param>
		protected override void OnMessageReceived(Event networkEvent)
		{
			NetworkLogger.Log($"Packet from [{networkEvent.Peer.IP}] ({networkEvent.Peer.ID}) " +
				$"on Channel [{networkEvent.ChannelID}] Length [{networkEvent.Packet.Length}]");

			if (!ClientManager.TryGetClient(networkEvent.Peer.ID, out FlareClientShell client))
			{
				NetworkLogger.Log($"Message received from a null client with ID [{networkEvent.Peer.ID}]", LogLevel.Warning);
			}

			// Deserialize message data
			networkEvent.Packet.CopyTo(receivePacketBuffer);
			Message message = new Message(receivePacketBuffer, networkEvent.Packet.Length + 4);

			// Process the message and invoke any callback
			MessageHandler.ProcessMessage(message, client);
		}

		#endregion

		#region Messages

		/// <summary>
		/// Send a message to all connected clients.
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <param name="channel">The channel to send the message through</param>
		public override void SendMessage(Message message, byte channel = 0)
		{
			// Create packet and broadcast
			Packet packet = default;
			packet.Create(message.GetBufferArray());
			Host.Broadcast(channel, ref packet);
		}

		/// <summary>
		/// Send a message to an array of clients.
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <param name="clients">The clients to send to</param>
		/// <param name="channel">The channel to send the message through</param>
		public void SendMessage(Message message, IClient[] clients, byte channel = 0)
		{
			// Extract the array of peers from the clients
			// TODO: Figure out a more efficient way for this.
			int l = clients.Length;
			Peer[] peers = new Peer[l];
			for (int i = 0; i < l; ++i)
			{
				peers[i] = clients[i].Peer;
			}

			// Create packet and send to selected clients
			Packet packet = default;
			packet.Create(message.GetBufferArray(), PacketFlags.Reliable);
			Host.Broadcast(channel, ref packet, peers);
		}

		#endregion

		/// <summary>
		/// Try to get a client connected to this server.
		/// </summary>
		/// <param name="clientId">The ID of the client</param>
		/// <param name="client">The requested client</param>
		/// <returns>True if a client is found</returns>
		public bool TryGetClient(uint clientId, out IClient client)
		{
			// Try to grab the client shell from the client manager
			bool result = ClientManager.TryGetClient(clientId, out var shell);
			client = shell;

			return result;
		}

		/// <summary>
		/// Get all clients connected to this server.
		/// </summary>
		/// <returns>An array of all connected IClients</returns>
		public IClient[] GetAllClients()
		{
			return ClientManager.GetAllClients();
		}

		/// <summary>
		/// Disconnects all clients and shuts the server down.
		/// </summary>
		public override void Disconnect()
		{
			NetworkLogger.Log(NetworkLogEvent.ServerStop);

			// Kick all remaining clients off
			var clients = ClientManager.GetAllClients();
			for (int i = 0; i < clients.Length; ++i)
			{
				clients[i].Disconnect();
			}
			ClientManager = null;

			// Shut the rest of the client down
			Shutdown();
			//_ = Upnp.ClosePort($"FlareServer{Address.Port}", Address.Port);
		}
	}
}
