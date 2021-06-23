using FlareNet.Debug;
using System;
using System.Collections.Generic;

namespace FlareNet
{
	public delegate void FlarePayloadCallback<T>(T payload) where T : INetworkPayload;

	public interface INetworkPayload : ISerializable { }

	internal class PayloadHandler
	{
		private readonly Dictionary<ushort, Callback> payloadCallbacks = new Dictionary<ushort, Callback>();
		private readonly Queue<MessagePayload> pollQueue = new Queue<MessagePayload>();
		private readonly Queue<Action> registrationQueue = new Queue<Action>();
		private bool isInvoking;

		/// <summary>
		/// Add a callback to listen for payloads of a given type.
		/// </summary>
		/// <typeparam name="T">The payload type to listen for</typeparam>
		/// <param name="callback">The delegate to call</param>
		public void AddCallback<T>(FlarePayloadCallback<T> callback) where T : INetworkPayload
		{
			if (!isInvoking)
				Add(callback);
			else
				registrationQueue.Enqueue(() => Add(callback));

			void Add<P>(FlarePayloadCallback<P> c) where P : INetworkPayload
			{
				var type = typeof(P);
				var tag = NetworkTagAttribute.GetTag(type);

				if (tag != null)
				{
					// Add the callback to the dictionary
					if (!payloadCallbacks.TryGetValue(tag.Value, out var value))
					{
						// Create callback entry if it doesn't exist
						value = new Callback(type);
						payloadCallbacks.Add(tag.Value, value);
					}

					value.Add(c);
				}
				else
					NetworkLogger.Log("Cannot add callbacks for types with no NetworkTag!");
			}
		}

		/// <summary>
		/// Remove a callback from listening for payloads of a given type.
		/// </summary>
		/// <typeparam name="T">The payload type being listened for</typeparam>
		/// <param name="callback">The delegate to remove</param>
		public void RemoveCallback<T>(FlarePayloadCallback<T> callback) where T : INetworkPayload
		{
			if (!isInvoking)
				Remove(callback);
			else
				registrationQueue.Enqueue(() => Remove(callback));

			void Remove<P>(FlarePayloadCallback<P> c) where P : INetworkPayload
			{
				var tag = NetworkTagAttribute.GetTag(typeof(P));

				// Remove the callback from the dictionary
				if (tag != null && payloadCallbacks.TryGetValue(tag.Value, out var value))
					value.Remove(c);
				else
					NetworkLogger.Log("Cannot remove callback - no entry for this payload exists");
			}
		}

		/// <summary>
		/// Remove all registered callbacks for a given payload type.
		/// </summary>
		/// <typeparam name="T">The type to remove callbacks for</typeparam>
		public void ClearCallbacks<T>() where T : INetworkPayload
		{
			if (!isInvoking)
				Clear<T>();
			else
				registrationQueue.Enqueue(() => Clear<T>());

			void Clear<P>() where P : INetworkPayload
			{
				var tag = NetworkTagAttribute.GetTag(typeof(P));

				if (tag != null && payloadCallbacks.ContainsKey(tag.Value))
					payloadCallbacks.Remove(tag.Value);
			}
		}

		/// <summary>
		/// Buffer for the incoming packets to copy their data to.
		/// </summary> Note: value is roughly twice maximum safe packet size.
		protected internal readonly byte[] receivePacketBuffer = new byte[PacketBufferSize];
		protected internal const int PacketBufferSize = 1024;

		/// <summary>
		/// Turn a packet into a message.
		/// </summary>
		/// <param name="p">The packet to process</param>
		public void ProcessPacket(Packet p)
		{
			byte[] buffer = p.Length > PacketBufferSize ? new byte[p.Length] : receivePacketBuffer;
			p.CopyTo(buffer);

			// Process the message and invoke any listeners
			using (var message = new Message(buffer, p.Length))
				ProcessMessage(message);
		}

		/// <summary>
		/// Turn a message into a payload if a listener for it exists.
		/// </summary>
		/// <param name="message">The message to process</param>
		public void ProcessMessage(Message message)
		{
			if (payloadCallbacks.TryGetValue(message.Tag, out var callback))
			{
				var value = (INetworkPayload)Activator.CreateInstance(callback.Type);
				message.Process(ref value);
				pollQueue.Enqueue(new MessagePayload { Value = value, Callback = callback });
			}
			else
				NetworkLogger.Log("There are no listeners for messages with tag " + message.Tag, LogLevel.Warning);

			message.Dispose();
		}

		/// <summary>
		/// Manually push a new payload into the queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="payload">The payload to add</param>
		internal void PushPayload<T>(T payload) where T : INetworkPayload
		{
			var tag = NetworkTagAttribute.GetTag(typeof(T));

			if (tag != null && payloadCallbacks.TryGetValue(tag.Value, out var c))
				pollQueue.Enqueue(new MessagePayload { Value = payload, Callback = c });
		}

		/// <summary>
		/// Poll this payload handler and process any payloads in the queue.
		/// </summary>
		public void Poll()
		{
			while (pollQueue.Count > 0)
			{
				isInvoking = true;

				var payload = pollQueue.Dequeue();
				payload.Callback.Invoke(payload.Value);

				isInvoking = false;
			}

			while (registrationQueue.Count > 0)
			{
				var action = registrationQueue.Dequeue();
				action.Invoke();
			}
		}

		private struct MessagePayload
		{
			public INetworkPayload Value;
			public Callback Callback;
		}

		private class Callback
		{
			public Type Type;

			private readonly Dictionary<int, FlarePayloadCallback<INetworkPayload>> Values = new Dictionary<int, FlarePayloadCallback<INetworkPayload>>();

			public Callback(Type t) => Type = t;

			internal void Add<T>(FlarePayloadCallback<T> callback) where T : INetworkPayload
			{
				int key = callback.Method.GetHashCode();

				if (!Values.ContainsKey(key))
					Values.Add(key, x => callback((T)x));
			}

			internal void Remove<T>(FlarePayloadCallback<T> callback) where T : INetworkPayload
			{
				int key = callback.Method.GetHashCode();

				if (Values.ContainsKey(key))
					Values.Remove(key);
			}

			internal void Invoke(INetworkPayload value)
			{
				foreach (var v in Values.Values)
					v.Invoke(value);
			}
		}
	}
}
