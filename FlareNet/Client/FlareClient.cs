using ENet;
using FlareNet.Debug;
using System.Threading;

namespace FlareNet
{
	public class FlareClient : FlareClientBase
	{
		protected internal Host Host { get; set; }
		protected internal Address Address { get; set; }

		internal readonly PayloadHandler PayloadHandler = new PayloadHandler();
		private Thread updateThread;
		private bool isRunning;

		protected FlareClient()
		{
			ENetLibrary.InitializeLibrary();
			Host = new Host(); // New host is created anyway, do it with init
		}

		/// <summary>
		/// Create a FlareClient and connect by IP and port.
		/// </summary>
		/// <param name="ip">The address to connnect to</param>
		/// <param name="port">The port to connect with</param>
		/// <param name="channelLimit">The number of channels</param>
		public FlareClient(string ip, ushort port, int channelLimit) : this()
		{
			// Create the host and address
			Host.Create();
			var Address = new Address() { Port = port };
			Address.SetHost(ip);

			// Set up listener for ID
			AddCallback<IdAssignment>(AssignID);

			// Create the peer
			Peer = Host.Connect(Address, channelLimit);

			StartUpdateThread();

			NetworkLogger.Log(NetworkLogEvent.ClientStart);
		}

		/// <summary>
		/// Create a FlareClient and connect by IP and port.
		/// </summary>
		/// <param name="ip">The address to connnect to</param>
		/// <param name="port">The port to connect with</param>
		public FlareClient(string ip, ushort port) : this(ip, port, ServerConfig.DefaultChannelCount)
		{
		}

		protected void StartUpdateThread()
		{
			isRunning = true;
			updateThread = new Thread(Update);
			updateThread.Start();
		}

		private void AssignID(IdAssignment p)
		{
			// Don't listen for the ID anymore
			RemoveCallback<IdAssignment>(AssignID);

			Id = p.id;

			NetworkLogger.Log($"Client initialized with ID [{Id}]");
			PayloadHandler.PushPayload(new ClientConnected { Client = this });
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
		}

		protected virtual void OnDisconnect(Event e)
		{
			NetworkLogger.Log(NetworkLogEvent.ClientDisconnect);
			PayloadHandler.PushPayload(new ClientDisconnected { ClientId = Id });
		}

		protected virtual void OnTimeout(Event e)
		{
			NetworkLogger.Log(NetworkLogEvent.ClientTimeout);
			PayloadHandler.PushPayload(new ClientDisconnected { ClientId = Id });
		}

		protected virtual void OnMessageReceived(Event e)
		{
			NetworkLogger.Log($"Packet from server on channel [{e.ChannelID}] with length [{e.Packet.Length}]");

			PayloadHandler.ProcessPacket(e.Packet);
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Register a method to be invoked when a payload of a certain type is received.
		/// </summary>
		/// <typeparam name="T">The payload type to listen for</typeparam>
		/// <param name="c">The method to call</param>
		public void AddCallback<T>(FlarePayloadCallback<T> c) where T : INetworkPayload => PayloadHandler.AddCallback(c);

		/// <summary>
		/// Remove a function from being invoked when a payload of a certain type is received.
		/// </summary>
		/// <typeparam name="T">The payload type being listened for</typeparam>
		/// <param name="c">The method to remove</param>
		public void RemoveCallback<T>(FlarePayloadCallback<T> c) where T : INetworkPayload => PayloadHandler.RemoveCallback(c);

		/// <summary>
		/// Clear all listeners for a payload type.
		/// </summary>
		/// <typeparam name="T">The payload type to clear</typeparam>
		public void ClearCallbacks<T>() where T : INetworkPayload => PayloadHandler.ClearCallbacks<T>();

		/// <summary>
		/// Poll the client for received payloads.
		/// </summary>
		public void PollMessages() => PayloadHandler.Poll();

		#endregion

		#region Messages

		public virtual void SendMessage<T>(T value, byte channel = 0) where T : INetworkPayload
		{
			var tag = NetworkTagAttribute.GetTag(typeof(T));

			if (tag != null)
			{
				Message m = new Message(tag.Value);
				m.Process(ref value);
				SendMessage(m, tag.PacketFlags, channel);
			}
			else
				NetworkLogger.Log("Cannot send a NetworkPayload with no NetworkTag!");
		}

		protected virtual void SendMessage(Message message, byte channel = 0)
		{
			SendMessage(message, PacketFlags.Reliable, channel);
		}

		protected internal virtual void SendMessage(Message message, PacketFlags flags, byte channel)
		{
			// Create and send packet
			Packet packet = default;
			packet.Create(message.GetBufferArray(), flags);
			Peer.Send(channel, ref packet);
		}

		public virtual void SendMessage<T>(T value, byte channel = 0, params IClient[] clients) where T : ISerializable
		{
			SendMessage(value, channel);
		}

		#endregion

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
