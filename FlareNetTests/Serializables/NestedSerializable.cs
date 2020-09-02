using FlareNet;

namespace FlareNetTests
{
	public class NestedSerializable : ISerializable
	{
		public byte ExampleByte;
		public bool ExampleBool;

		public void Sync(Message message)
		{
			message.Process(ref ExampleByte);
			message.Process(ref ExampleBool);
		}
	}
}
