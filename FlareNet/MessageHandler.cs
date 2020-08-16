using FlareNet.Debug;
using System.Collections.Generic;

namespace FlareNet
{
	public delegate void FlareMessageCallback(Message message, IClient client);

	internal static class MessageHandler
	{
		private static readonly Dictionary<ushort, FlareMessageCallback> callbacks = new Dictionary<ushort, FlareMessageCallback>();

		/// <summary>
		/// Adds a callback associated with a tag to the dictionary
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="callback"></param>
		public static void RegisterCallback(ushort tag, FlareMessageCallback callback)
		{
			if (!callbacks.ContainsKey(tag))
			{
				callbacks.Add(tag, callback);
			}
			else
			{
				NetworkLogger.Log("Cannot register callback - an entry with the same tag already exists!", LogLevel.Error);
			}
		}

		/// <summary>
		/// Remove the callback associated with a tag.
		/// </summary>
		/// <param name="tag">The tag to check</param>
		public static void RemoveCalback(ushort tag)
		{
			if (callbacks.ContainsKey(tag))
			{
				callbacks.Remove(tag);
			}
			else
			{
				NetworkLogger.Log("Cannot remove callback - no entry with the tag exists!", LogLevel.Error);
			}
		}

		/// <summary>
		/// Process an incoming message and invoke the registered callback.
		/// </summary>
		/// <param name="message">The message to process</param>
		/// <param name="client">The originating client</param>
		public static void ProcessMessage(Message message, IClient client)
		{
			if (callbacks.TryGetValue(message.Tag, out var callback))
			{
				callback?.Invoke(message, client);
			}
			else
			{
				NetworkLogger.Log($"A callback with the tag [{message.Tag}] has not been registered!", LogLevel.Error);
			}
		}
	}
}