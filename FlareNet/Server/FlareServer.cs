﻿using ENet;
using FlareNet.Debug;

namespace FlareNet
{
	public sealed class FlareServer : FlareClient
	{
		internal FlareClientManager ClientManager { get; private set; }

		public ServerConfig Config { get; set; }

		public override string IpAddress => Address.GetIP();
		public override ushort Port => Address.Port;
		public override ulong TotalDataIn => Host.BytesReceived;
		public override ulong TotalDataOut => Host.BytesSent;

		/// <summary>
		/// Start a server on a given port.
		/// </summary>
		/// <param name="port">The port to use</param>
		/// <param name="config">Optional server configuration</param>
		public FlareServer(ushort port, ServerConfig config = null) : base()
		{
			Config = config ?? new ServerConfig();

			StartServer(port);
		}

		private void StartServer(ushort port)
		{
			// Create the address and host
			Address = new Address { Port = port };
			Host.Create(Address, Config.MaxConnections, Config.ChannelCount);

			ClientManager = new FlareClientManager(Config.MaxConnections);
			StartUpdateThread();

			NetworkLogger.Log(NetworkLogEvent.ServerStart);
		}

		#region Updates

		protected override void OnConnect(Event e)
		{
			Peer peer = e.Peer;
			uint id = peer.ID;

			if (peer.IsSet)
			{
				NetworkLogger.Log($"Client [{id}] connected");

				var client = new FlareClientShell(peer);
				ClientManager?.AddClient(client);

				// Send the client its ID manually
				PayloadHandler.AddCallback<ClientAssigned>(PushClientConnected);
				SendMessage(new IdAssignment { id = id }, 0, client);
			}
			else
				NetworkLogger.Log("Unset peer connected. How?", LogLevel.Error);
		}

		private void PushClientConnected(ClientAssigned p)
		{
			NetworkLogger.Log("Client connection finalized");
			PayloadHandler.RemoveCallback<ClientAssigned>(PushClientConnected);

			if (ClientManager.TryGetClient(p.id, out var client))
				PayloadHandler.PushPayload(new ClientConnected { Client = client });
		}

		protected override void OnDisconnect(Event e)
		{
			Peer peer = e.Peer;
			uint id = peer.ID;

			if (peer.IsSet)
			{
				NetworkLogger.Log($"Client [{id}] disconnected");
				ClientManager?.RemoveClient(id);
				PayloadHandler.PushPayload(new ClientDisconnected { ClientId = id });
			}
			else
				NetworkLogger.Log("Unset peer disconnected!", LogLevel.Error);
		}

		protected override void OnTimeout(Event e)
		{
			Peer peer = e.Peer;
			uint id = peer.ID;

			if (peer.IsSet)
			{
				NetworkLogger.Log($"Client [{id}] timed out");
				ClientManager?.RemoveClient(id);
				PayloadHandler.PushPayload(new ClientDisconnected { ClientId = id });
			}
			else
				NetworkLogger.Log("Unset peer timed out!", LogLevel.Error);
		}

		protected override void OnMessageReceived(Event e)
		{
			NetworkLogger.Log($"Packet from [{e.Peer.IP}] ({e.Peer.ID}) " +
				$"on Channel [{e.ChannelID}] with Length [{e.Packet.Length}]");

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
		protected internal override void SendMessage(Message message, PacketFlags flags, byte channel = 0)
		{
			// Create packet and broadcast
			Packet packet = default;
			packet.Create(message.GetBufferArray(), flags);
			Host.Broadcast(channel, ref packet);
		}

		public override void SendMessage<T>(T value, byte channel = 0, params IClient[] clients)
		{
			var tag = NetworkTagAttribute.GetTag(typeof(T));

			if (tag != null)
			{
				Message m = new Message(tag.Value);
				m.Process(ref value);
				SendMessage(m, tag.PacketFlags, channel, clients);
			}
			else
				NetworkLogger.Log("Cannot send a NetworkPayload with no NetworkTag!");
		}

		/// <summary>
		/// Send a message to an array of clients.
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <param name="clients">The clients to send to</param>
		/// <param name="channel">The channel to send the message through</param>
		private void SendMessage(Message message, PacketFlags flags, byte channel, IClient[] clients)
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
			packet.Create(message.GetBufferArray(), flags);
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
			foreach (FlareClientShell client in clients)
				client.Disconnect();

			ClientManager = null;

			// Shut the rest of the client down
			Shutdown();
		}
	}
}
