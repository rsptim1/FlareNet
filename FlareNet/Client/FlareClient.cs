using ENet;
using FlareNet.Client;
using FlareNet.Server;
using System.Threading;

namespace FlareNet
{
	public class FlareClient : FlareClientBase
	{
		/// <summary>
		/// Buffer for the incoming packets to copy their data to.
		/// </summary> Note: value is roughly twice maximum safe packet size.
		protected internal readonly byte[] receivePacketBuffer = new byte[PacketBufferSize];
		protected internal const int PacketBufferSize = 1024;

		protected internal Host Host { get; set; }
		protected internal Address Address { get; set; }

		private readonly MessageHandler MessageHandler = new MessageHandler();
		private Thread updateThread;
		private bool isRunning;

		private OnClientConnected clientConnected;
		public virtual event OnClientConnected ClientConnected
		{
			add => clientConnected += value;
			remove => clientConnected -= value;
		}

		private OnClientDisconnected clientDisconnected;
		public virtual event OnClientDisconnected ClientDisconnected
		{
			add => clientDisconnected += value;
			remove => clientDisconnected -= value;
		}

		protected FlareClient() { ENetLibrary.InitializeLibrary(); }

		/// <summary>
		/// Create a FlareClient and connect by IP and port.
		/// </summary>
		/// <param name="ip">The address to connnect to</param>
		/// <param name="port">The port to connect with</param>
		public FlareClient(string ip, ushort port, int channelLimit) : this()
		{
			// Setup the host
			Host = new Host();
			Host.Create();

			// Setup the address
			var Address = new Address() { Port = port };
			Address.SetHost(ip);

			// Setup the peer
			Peer = Host.Connect(Address, channelLimit);

			StartUpdateThread();

			NetworkLogger.Log(NetworkLogEvent.ClientStart);
		}

		public FlareClient(string ip, ushort port) : this(ip, port, ServerConfig.DefaultChannelCount)
		{
		}

		protected void StartUpdateThread()
		{
			updateThread = new Thread(Update);
			isRunning = true;
			updateThread.Start();
		}

		#region Updates

		internal void Update()
		{
			while (isRunning)
			{
				bool polled = false;

				while (!polled)
				{
					if (Host.CheckEvents(out Event e) <= 0)
					{
						if (Host.Service(15, out e) <= 0)
							break;

						polled = true;
					}

					switch (e.Type)
					{
						case EventType.None:
							break;

						case EventType.Connect:
							OnConnect(e);
							break;

						case EventType.Disconnect:
							OnDisconnect(e);
							break;

						case EventType.Timeout:
							OnTimeout(e);
							break;

						case EventType.Receive:
							OnMessageReceived(e);
							e.Packet.Dispose();
							break;
					}
				}
			}

			// Clean up the host resources
			Host.Flush();
			Host.Dispose();

			ENetLibrary.DeinitializeLibrary();
		}

		protected virtual void OnConnect(Event e)
		{
			NetworkLogger.Log(NetworkLogEvent.ClientConnect);
			clientConnected?.Invoke(this);
		}

		protected virtual void OnDisconnect(Event e)
		{
			NetworkLogger.Log(NetworkLogEvent.ClientDisconnect);
			clientDisconnected?.Invoke(this);
		}

		protected virtual void OnTimeout(Event e)
		{
			NetworkLogger.Log(NetworkLogEvent.ClientTimeout);
		}

		protected virtual void OnMessageReceived(Event e)
		{
			NetworkLogger.Log($"Packet from server on Channel [{e.ChannelID}] with Length [{e.Packet.Length}]");
			ProcessMessage(e, null);
		}

		protected void ProcessMessage(Event e, IClient client)
		{
			// Deserialize message data
			Packet packet = e.Packet;
			byte[] buffer = packet.Length > PacketBufferSize ? new byte[packet.Length] : receivePacketBuffer;

			e.Packet.CopyTo(buffer);
			Message message = new Message(buffer, e.Packet.Length + 4);

			// Process the message and invoke any callback
			MessageHandler.ProcessMessage(message, null);
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Register a function or delegate to be invoked when a message with a tag is received.
		/// </summary>
		/// <param name="tag">The tag to look for</param>
		/// <param name="callback">The callback to invoke</param>
		public void RegisterCallback(ushort tag, FlareMessageCallback callback)
		{
			MessageHandler.RegisterCallback(tag, callback);
		}

		/// <summary>
		/// Remove all callbacks from being invoked when the respective tag is received.
		/// </summary>
		/// <param name="tag">The tag to remove callbacks from</param>
		public void RemoveCallback(ushort tag)
		{
			MessageHandler.RemoveCallback(tag);
		}

		/// <summary>
		/// Remove a callback from a tag.
		/// </summary>
		/// <param name="tag">The tag to remove the callback from</param>
		/// <param name="callback">The callback to remove</param>
		public void RemoveCallback(ushort tag, FlareMessageCallback callback)
		{
			MessageHandler.RemoveCallback(tag, callback);
		}

		#endregion

		public virtual void SendMessage(Message message, byte channel = 0)
		{
			// Create and send packet
			Packet packet = default;
			packet.Create(message.GetBufferArray(), PacketFlags.Reliable);
			Peer.Send(channel, ref packet);
		}

		public virtual void SendMessage(Message message, byte channel = 0, params IClient[] clients)
		{
			SendMessage(message, channel);
		}

		/// <summary>
		/// Disconnect from the server and shut down.
		/// </summary>
		public override void Disconnect()
		{
			Peer.Disconnect(0);
			Shutdown();
		}

		protected void Shutdown()
		{
			isRunning = false;
		}
	}
}
