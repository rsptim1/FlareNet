using NetStack.Compression;
using NetStack.Serialization;
using System;

namespace FlareNet
{
	public class Message : IDisposable
	{
		public readonly ushort Tag;
		public readonly BitBuffer Buffer;

		// We're writing to the buffer
		private readonly bool writing;

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

		public void Process(ref byte value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadByte();
			}
		}

		public void Process(ref short value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadShort();
			}
		}

		public void Process(ref ushort value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadUShort();
			}
		}

		public void Process(ref int value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadInt();
			}
		}

		public void Process(ref uint value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadUInt();
			}
		}

		public void Process(ref long value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadLong();
			}
		}

		public void Process(ref ulong value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadULong();
			}
		}

		public void Process(ref string value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadString();
			}
		}

		public void Process(ref bool value)
		{
			if (writing)
			{
				Buffer.Add(value);
			}
			else
			{
				value = Buffer.ReadBool();
			}
		}

		public void Process(ref float value)
		{
			if (writing)
			{
				Buffer.Add(HalfPrecision.Compress(value));
			}
			else
			{
				value = HalfPrecision.Decompress(Buffer.ReadUShort());
			}
		}

		#endregion

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}
