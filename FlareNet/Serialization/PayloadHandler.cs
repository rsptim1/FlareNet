using System;
using System.Collections.Generic;

namespace FlareNet
{
	public delegate void FlarePayloadCallback<T>(T payload) where T : ISerializable;

	/// <summary>
	/// TODO: Clean up this class
	/// </summary>
	internal class PayloadHandler
	{
		private readonly Dictionary<ushort, Callback> payloadCallbacks = new Dictionary<ushort, Callback>();
		private readonly Queue<(Callback, ISerializable)> pollQueue = new Queue<(Callback, ISerializable)>();

		public void AddCallback<T>(FlarePayloadCallback<T> callback) where T : ISerializable
		{
			var type = typeof(T);
			var tag = (NetworkTagAttribute)Attribute.GetCustomAttribute(type, typeof(NetworkTagAttribute));

			if (tag != null)
			{
				//FlarePayloadCallback<ISerializable> l = x => callback((T)x);

				// Add the callback to the dictionary
				if (payloadCallbacks.TryGetValue(tag.Tag, out var value))
					value += callback;
				else
				{
					value = new Callback(type);
					value += callback;
					payloadCallbacks.Add(tag.Tag, value);

				}
			}
		}

		public void RemoveCallback<T>(FlarePayloadCallback<T> callback) where T : ISerializable
		{
			var tag = (NetworkTagAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(NetworkTagAttribute));

			if (tag != null) // Remove the callback from the dictionary
				if (payloadCallbacks.TryGetValue(tag.Tag, out var value))
					value -= callback;
		}

		public void ProcessMessage(Message message)
		{
			if (payloadCallbacks.TryGetValue(message.Tag, out var callback))
			{
				var p = (ISerializable)Activator.CreateInstance(callback.Type);
				pollQueue.Enqueue((callback, p));
			}

			message.Dispose();
		}

		public void Poll()
		{
			while (pollQueue.Count > 0)
			{
				var callback = pollQueue.Dequeue();
				foreach (var c in callback.Item1.Values)
					c.Invoke(callback.Item2);
			}
		}

		/// <summary>
		/// TODO: More performant way of saving these
		/// </summary>
		private class Callback
		{
			public Type Type;
			public readonly List<FlarePayloadCallback<ISerializable>> Values;
			public readonly List<Delegate> Originals = new List<Delegate>();

			public Callback(Type t)
			{
				Type = t;
			}

			public static Callback operator +(Callback c, Delegate o)
			{
				if (!c.Originals.Contains(o))
				{
					c.Originals.Add(o);
					c.Values.Add(x => o.DynamicInvoke(x));
				}

				return c;
			}

			public static Callback operator -(Callback c, Delegate o)
			{
				if (c.Originals.Contains(o))
				{
					int index = c.Originals.IndexOf(o);
					c.Originals.RemoveAt(index);
					c.Values.RemoveAt(index); // We assume it's the same index
				}

				return c;
			}
		}
	}
}