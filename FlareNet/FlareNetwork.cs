using FlareNet.Client;
using FlareNet.Debug;
using FlareNet.Server;
using System;

namespace FlareNet
{
	public static class FlareNetwork
	{
		internal static FlareClient flareClient;

		public static FlareClientManager FlareClientManager { get; private set; }

		/// <summary>
		/// Create a FlareClient and connect to a server.
		/// </summary>
		/// <param name="ip">The server IP address</param>
		/// <param name="port">The port to connect with</param>
		/// <param name="onCompleted">A callback handling if the client was able to connect or not</param>
		public static void Connect(string ip, ushort port, Action<bool> onCompleted = null)
		{
			flareClient = new LocalFlareClient(ip, port);
			onCompleted?.Invoke(flareClient.IsConnected);
		}

		/// <summary>
		/// Start a server with a port.
		/// </summary>
		/// <param name="port">The port to open</param>
		public static void Create(ushort port, ServerConfig config = null)
		{
			if (config == null)
			{
				config = new ServerConfig();
			}

			FlareServer server = new FlareServer(config, port);
			FlareClientManager = server.ClientManager;
			flareClient = server;
		}

		/// <summary>
		/// Shutdown or disconnect the current FlareClient.
		/// </summary>
		public static void Shutdown()
		{
			flareClient?.Disconnect();
			FlareClientManager = null;
			flareClient = null;
		}

		/// <summary>
		/// Update the current FlareClient.
		/// </summary>
		public static void Update()
		{
			if (flareClient == null)
			{
				NetworkLogger.Log("Unable to update FlareNet - no client is running!", LogLevel.Error);
				return;
			}

			flareClient.Update();
		}

		public static void RegisterCallback(ushort tag, FlareMessageCallback callback)
		{
			MessageHandler.RegisterCallback(tag, callback);
		}

		public static void RemoveCallback(ushort tag)
		{
			MessageHandler.RemoveCalback(tag);
		}
	}
}
