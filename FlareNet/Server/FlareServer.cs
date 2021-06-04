using ENet;
using FlareNet.Server;

namespace FlareNet
{
	public sealed class FlareServer : FlareClient
	{
		internal FlareClientManager ClientManager { get; private set; }

		public ServerConfig Config { get; set; }

		public new string IpAddress => Address.GetIP();
		public new uint Port => Address.Port;
		public new ulong TotalDataIn => Host.BytesReceived;
		public new ulong TotalDataOut => Host.BytesSent;

		public override event OnClientConnected ClientConnected
		{
			add => ClientManager.ClientConnected += value;
			remove => ClientManager.ClientConnected -= value;
		}

		public override event OnClientDisconnected ClientDisconnected
		{
			add => ClientManager.ClientDisconnected += value;
			remove => ClientManager.ClientDisconnected -= value;
		}

		public FlareServer(ushort port, ServerConfig config = null) : base()
		{
			Config = config ?? new ServerConfig();
			
			StartServer(port);
		}

		private void StartServer(ushort port)
		{
			Host = new Host();

			// Create the address for the host to use
			Address = new Address { Port = port };

			// Initialize the host
			Host.Create(Address, Config.MaxConnections);

			ClientManager = new FlareClientManager(Config.MaxConnections);
			StartUpdateThread();

			NetworkLogger.Log(NetworkLogEvent.ServerStart);
		}

		#region Updates

		protected override void OnConnect(Event e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] connected from [{peer.IP}]");
			ClientManager?.AddClient(new FlareClientShell(peer));
		}

		protected override void OnDisconnect(Event e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] disconnected from [{peer.IP}]");
			ClientManager?.RemoveClient(peer.ID);
		}

		protected override void OnTimeout(Event e)
		{
			Peer peer = e.Peer;

			NetworkLogger.Log($"Client [{peer.ID}] timeout from [{peer.IP}]");
			ClientManager?.RemoveClient(peer.ID);
		}

		protected override void OnMessageReceived(Event e)
		{
			NetworkLogger.Log($"Packet from [{e.Peer.IP}] ({e.Peer.ID}) " +
				$"on Channel [{e.ChannelID}] Length [{e.Packet.Length}]");

			if (!ClientManager.TryGetClient(e.Peer.ID, out FlareClientShell client))
			{
				NetworkLogger.Log($"Message received from an unregistered client with ID [{e.Peer.ID}]", LogLevel.Warning);
			}

			ProcessMessage(e, client);
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
			packet.Create(message.GetBufferArray(), PacketFlags.Reliable);
			Host.Broadcast(channel, ref packet);
		}

		/// <summary>
		/// Send a message to an array of clients.
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <param name="clients">The clients to send to</param>
		/// <param name="channel">The channel to send the message through</param>
		public override void SendMessage(Message message, byte channel = 0, params IClient[] clients)
		{
			// Extract the array of peers from the clients
			// TODO: Figure out a more efficient way for this.
			int length = clients.Length;
			Peer[] peers = new Peer[length];
			for (int i = 0; i < length; ++i)
			{
				ClientManager.TryGetClient(clients[i].Id, out var client);
				peers[i] = client.Peer;
			}

			// Create packet and send to selected clients
			Packet packet = default;
			packet.Create(message.GetBufferArray(), PacketFlags.Reliable);
			Host.Broadcast(channel, ref packet, peers);
		}

		#endregion

		#region Client Manager

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

		public int ClientCount => ClientManager.Count;

		#endregion

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
		}
	}
}
