using FlareNet.Client;
using FlareNet.Server;
using System;

namespace FlareNet
{
	/// <summary>
	/// A main interface and helper class for FlareNet, this class provides
	/// functions for creating a server and connecting to an existing one,
	/// as well as also exposing the main Update function.
	/// This update function must be called for FlareNet to function.
	/// </summary>
	public static class FlareNetwork
	{
		internal static event Action ClientUpdate;
		internal static bool LibraryInitialized = false;

		/// <summary>
		/// Create a FlareClient and connect to a server.
		/// </summary>
		/// <param name="ip">The server IP address</param>
		/// <param name="port">The port to connect with</param>
		public static FlareClient Connect(string ip, ushort port)
		{
			return new FlareClient(ip, port);
		}

		/// <summary>
		/// Start a FlareServer with a port.
		/// </summary>
		/// <param name="port">The port to open</param>
		public static FlareServer Create(ushort port, ServerConfig config = null)
		{
			return new FlareServer(config ?? new ServerConfig(), port);
		}

		/// <summary>
		/// Update all active FlareClients.
		/// </summary>
		public static void Update()
		{
			ClientUpdate?.Invoke();
		}

		internal static void InitializeLibrary()
		{
			// If the library has not been initialized yet
			if (!LibraryInitialized)
			{
				if (!ENet.Library.Initialize())
				{
					throw new System.Exception();
				}
				
				LibraryInitialized = true;
			}
		}

		internal static void DeinitializeLibrary()
		{
			// If it's initialized and there's no clients registered to update
			if (LibraryInitialized && ClientUpdate == null)
			{
				ENet.Library.Deinitialize();
				LibraryInitialized = false;
			}
		}
	}
}
