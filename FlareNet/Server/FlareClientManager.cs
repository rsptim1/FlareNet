using FlareNet.Debug;
using System.Collections.Generic;

namespace FlareNet
{
	/// <summary>
	/// Handles adding, removing and management of clients that are connected to the server.
	/// </summary>
	internal class FlareClientManager
	{
		private readonly Dictionary<uint, FlareClientShell> connectedClients;

		public int Count => connectedClients.Count;

		internal FlareClientManager(int maxConnections)
		{
			// Initialize the dictionary for clients
			connectedClients = new Dictionary<uint, FlareClientShell>(maxConnections);
		}

		/// <summary>
		/// Add a client to the client manager.
		/// </summary>
		/// <param name="client"></param>
		internal void AddClient(FlareClientShell client)
		{
			uint id = client.Peer.ID;

			// Add client to the connect clients dictionary
			if (!connectedClients.ContainsKey(id))
				connectedClients.Add(id, client);
			else
				NetworkLogger.Log($"Unable to add already existing peer with ID [{id}] to ClientManager!", LogCategory.Connections, LogLevel.Error);
		}

		/// <summary>
		/// Remove a client from the client manager by ID.
		/// </summary>
		/// <param name="id">The ID of the client to remove</param>
		internal void RemoveClient(uint id)
		{
			if (TryGetClient(id, out var client))
				connectedClients.Remove(id);
			else
				NetworkLogger.Log($"Unable to remove nonexistent peer with ID [{id}] from ClientManager!", LogCategory.Connections, LogLevel.Error);
		}

		/// <summary>
		/// Get all connected clients as an array.
		/// </summary>
		/// <returns>Connected clients as an array</returns>
		internal FlareClientShell[] GetAllClients()
		{
			var clients = new FlareClientShell[connectedClients.Count];

			connectedClients.Values.CopyTo(clients, 0);

			return clients;
		}

		/// <summary>
		/// Get a client by ID.
		/// </summary>
		/// <param name="clientId">The ID of the client</param>
		/// <returns></returns>
		internal bool TryGetClient(uint clientId, out FlareClientShell client)
		{
			return connectedClients.TryGetValue(clientId, out client);
		}
	}
}
