using NetStack.Compression;
using NetStack.Serialization;
using System;
using System.Collections.Generic;

namespace FlareNet
{
	/// <summary>
	/// Write to or read from a byte buffer sent over the network.
	/// </summary>
	public class Message : IDisposable
	{
		/// <summary>
		/// The length of the message.
		/// </summary>
		public int Length => Buffer.Length;

		public readonly ushort Tag;
		internal readonly BitBuffer Buffer;

		// True when writing to the buffer, false when reading
		private readonly bool isWriting;

		public bool IsWriting => isWriting;
		public bool IsReading => !IsWriting;

		/// <summary>
		/// Create a Message to read from a buffer.
		/// </summary>
		/// <param name="buffer">The buffer to read from</param>
		/// <param name="length">The length of the buffer to read</param>
		internal Message(byte[] buffer, int length)
		{
			Buffer = new BitBuffer();
			Buffer.FromArray(buffer, length);
			Tag = Buffer.ReadUShort();

			isWriting = false;
		}

		/// <summary>
		/// Create a Message to write to a buffer.
		/// </summary>
		/// <param name="tag">The tag the message will be sent with</param>
		public Message(ushort tag)
		{
			Buffer = new BitBuffer();
			Buffer.Add(Tag = tag);

			isWriting = true;
		}

		internal byte[] GetBufferArray()
		{
			byte[] result = new byte[Length + 4];
			Buffer.ToArray(result);
			return result;
		}

		#region Processing

		public void Process<T>(ref T serializable) where T : ISerializable
		{
			// If we're reading from the buffer, expect it to be null
			if (serializable == null && IsReading)
				serializable = Activator.CreateInstance<T>();

			serializable.Sync(this);
		}

		public void Process<T>(ref T[] serializables) where T : ISerializable
		{
			int length = IsReading ? Buffer.ReadInt() : serializables.Length;

			if (serializables == null && IsReading) // If we're reading, expect the array to be null
				serializables = Array.CreateInstance(typeof(T), length) as T[];
			if (IsWriting)
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref serializables[i]);
		}

		public void Process<T>(ref List<T> serializables) where T : ISerializable
		{
			int length = IsReading ? Buffer.ReadInt() : serializables.Count;

			if (serializables == null && IsReading) // If we're reading, expect the array to be null
				serializables = Activator.CreateInstance(typeof(List<T>), length) as List<T>;
			if (IsWriting)
				Process(ref length);

			for (int i = 0; i < length; ++i)
			{
				T item = serializables[i];
				Process(ref item);

				if (IsReading)
					serializables[i] = item;
			}
		}

		public void Process<T>(ref int width, ref int height, ref T[,] serializables) where T : ISerializable
		{
			Process(ref width);
			Process(ref height);

			if (serializables == null && IsReading)
				serializables = Array.CreateInstance(typeof(T), width, height) as T[,];

			for (int x = 0; x < width; ++x)
				for (int y = 0; y < height; ++y)
					Process(ref serializables[x, y]);
		}

		public void Process(ref byte value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadByte();
		}

		public void Process(ref sbyte value)
		{
			// TODO: Handle sbyte in BitBuffer
			if (IsWriting)
				Buffer.Add(value);
			else
				value = (sbyte)Buffer.ReadShort();
		}

		public void Process(ref byte[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new byte[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref short value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadShort();
		}

		public void Process(ref short[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new short[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref ushort value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadUShort();
		}

		public void Process(ref ushort[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new ushort[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref int value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadInt();
		}

		public void Process(ref int[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new int[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref uint value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadUInt();
		}

		public void Process(ref uint[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new uint[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref long value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadLong();
		}

		public void Process(ref long[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new long[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref ulong value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadULong();
		}

		public void Process(ref ulong[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new ulong[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref string value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadString();
		}

		public void Process(ref string[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new string[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref bool value)
		{
			if (isWriting)
				Buffer.Add(value);
			else
				value = Buffer.ReadBool();
		}

		public void Process(ref bool[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new bool[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		public void Process(ref float value)
		{
			if (isWriting)
				Buffer.Add(HalfPrecision.Compress(value));
			else
				value = HalfPrecision.Decompress(Buffer.ReadUShort());
		}

		public void Process(ref float[] values)
		{
			int length = IsReading ? Buffer.ReadInt() : values.Length;

			if (IsReading) // If we're reading, expect the array to be null
				values = new float[length];
			else
				Process(ref length);

			for (int i = 0; i < length; ++i)
				Process(ref values[i]);
		}

		#endregion

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}
