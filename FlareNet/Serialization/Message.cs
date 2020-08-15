using NetStack.Serialization;
using System;

namespace FlareNet
{
	public class Message : IDisposable
	{
		public readonly ushort Tag;
		public readonly BitBuffer Buffer;

		// We're writing to the buffer
		private bool writing;

		/// <summary>
		/// Create a Message to read from a buffer.
		/// </summary>
		/// <param name="buffer">The buffer to read from</param>
		public Message(BitBuffer buffer)
		{
			Buffer = buffer;
			Tag = buffer.ReadUShort();

			writing = false;
		}

		/// <summary>
		/// Create a Message to write to a buffer.
		/// </summary>
		/// <param name="tag">The tag the message will be sent with</param>
		public Message(ushort tag)
		{
			Buffer = new BitBuffer();
			Buffer.Add(Tag = tag);

			writing = true;
		}

		// TODO: Clean up this region into something less cancer
		#region Processing

		public void Process(ISerializable serializable)
		{
			serializable.Sync(this);
		}

		public void Process<T>(ref T value) where T : struct
		{
			if (writing)
			{
				// Write the variable to the buffer
			}
			else
			{
				// Read the variable from the buffer and assign ref
			}
		}

		#endregion

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}
