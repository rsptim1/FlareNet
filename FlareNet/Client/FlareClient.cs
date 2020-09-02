using ENet;
using FlareNet.Debug;

namespace FlareNet.Client
{
	public class FlareClient : FlareClientBase
	{
		/// <summary>
		/// Buffer for the incoming packets to copy their data to.
		/// </summary> Note: value is roughly twice maximum safe packet size.
		protected readonly byte[] receivePacketBuffer = new byte[1024];

		protected Host Host { get; set; }
		protected Address Address { get; set; }
		internal readonly MessageHandler MessageHandler = new MessageHandler();

		public FlareClient(string ip, ushort port)
		{
			FlareNetwork.InitializeLibrary();
			// Setup the address
			Address = new Address() { Port = port };
			Address.SetHost(ip);

			// Setup the host
			Host = new Host();
			Host.Create(1, 2);

			// Subscribe for network updates
			FlareNetwork.ClientUpdate += Update;

			// Setup the peer
			Peer = Host.Connect(Address, 2);
		}

		protected FlareClient() { }

		#region Updates

		internal void Update()
		{
			// Handle the next network event
			while (Host.CheckEvents(out Event networkEvent) > 0 || Host.Service(2, out networkEvent) > 0)
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

		public virtual void SendMessage(Message message, byte channel)
		{
			// Create and send packet
			Packet packet = default;
			packet.Create(message.GetBufferArray(), PacketFlags.Reliable);
			Peer.Send(channel, ref packet);
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
			FlareNetwork.ClientUpdate -= Update;

			if (Host != null)
			{
				Host.Flush();
				Host.Dispose();
			}

			FlareNetwork.DeinitializeLibrary();
		}
	}
}
