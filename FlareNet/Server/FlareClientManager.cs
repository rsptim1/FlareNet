using FlareNet.Client;
using FlareNet.Debug;
using System.Collections.Generic;

namespace FlareNet.Server
{
	/// <summary>
	/// Handles adding, removing and management of clients that are connected to the server.
	/// </summary>
	public class FlareClientManager
	{
		private readonly Dictionary<uint, IClient> connectedClients;

		public delegate void OnClientConnected(IClient client);
		public delegate void OnClientDisconnected(IClient client);

		/// <summary>
		/// Invoked when a client connects to the server.
		/// </summary>
		public event OnClientConnected ClientConnected;

		/// <summary>
		/// Invoked when a client disconnects from the server.
		/// </summary>
		public event OnClientConnected ClientDisconnected;

		public FlareClientManager(int maxConnections)
		{
			// Initialize the dictionary for clients
			connectedClients = new Dictionary<uint, IClient>(maxConnections);
		}

		/// <summary>
		/// Add a client to the client manager.
		/// </summary>
		/// <param name="client"></param>
		public void AddClient(FlareClient client)
		{
			uint id = client.Peer.ID;

			if (!connectedClients.ContainsKey(id))
			{
				// Add client to the connect clients dictionary
				connectedClients.Add(id, client);
				ClientConnected?.Invoke(client);
			}
			else
			{
				NetworkLogger.Log($"Unable to add already existing peer with ID [{id}] to ClientManager!", LogLevel.Error);
			}
		}


		/// <summary>
		/// Remove a client from the client manager by ID.
		/// </summary>
		/// <param name="id">The ID of the client to remove</param>
		public void RemoveClient(uint id)
		{
			if (TryGetClient(id, out var client))
			{
				ClientDisconnected?.Invoke(client);
				connectedClients.Remove(id);
			}
			else
			{
				NetworkLogger.Log($"Unable to remove nonexistent peer with ID [{id}] from ClientManager!", LogLevel.Error);
			}
		}

		/// <summary>
		/// Get all connected clients as an array.
		/// </summary>
		/// <returns>Connected clients as an array</returns>
		public IClient[] GetAllClients()
		{
			var clients = new IClient[connectedClients.Count];

			connectedClients.Values.CopyTo(clients, 0);

			return clients;
		}

		/// <summary>
		/// Get a client by ID.
		/// </summary>
		/// <param name="clientId">The ID of the client</param>
		/// <returns></returns>
		public bool TryGetClient(uint clientId, out IClient client)
		{
			return connectedClients.TryGetValue(clientId, out client);
		}
	}
}
