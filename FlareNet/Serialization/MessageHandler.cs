using FlareNet.Debug;
using System.Collections.Generic;

namespace FlareNet
{
	public delegate void FlareMessageCallback(Message message, IClient client);

	internal class MessageHandler
	{
		private readonly Dictionary<ushort, FlareMessageCallback> callbacks = new Dictionary<ushort, FlareMessageCallback>();

		/// <summary>
		/// Adds a callback associated with a tag to the dictionary.
		/// </summary>
		/// <param name="tag">The tag the callback will be invoked on</param>
		/// <param name="callback">The function or delegate to be invoked</param>
		public void RegisterCallback(ushort tag, FlareMessageCallback callback)
		{
			if (callbacks.TryGetValue(tag, out var registeredCallback))
			{
				// Add the new function to the existing callback
				registeredCallback -= callback;
				registeredCallback += callback;

				// Remove the existing from the dictionary, then add the new
				callbacks.Remove(tag);
				callbacks.Add(tag, registeredCallback);
			}
			else
			{
				// Create and add the callback
				callbacks.Add(tag, callback);
			}
		}

		/// <summary>
		/// Remove all callbacks associated with a tag.
		/// </summary>
		/// <param name="tag">The tag to check</param>
		public void RemoveCallback(ushort tag)
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
		/// Removes a callback from a tag entry.
		/// </summary>
		/// <param name="tag">The tag to remove the callback from</param>
		/// <param name="callback">The function or delegate to remove</param>
		public void RemoveCallback(ushort tag, FlareMessageCallback callback)
		{
			if (callbacks.TryGetValue(tag, out var registeredCallback))
			{
				registeredCallback -= callback;
				callbacks.Remove(tag);
				callbacks.Add(tag, registeredCallback);
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
		public void ProcessMessage(Message message, IClient client)
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