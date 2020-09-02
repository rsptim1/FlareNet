using FlareNet;

namespace FlareNetTests
{
	public class TestSerializable : ISerializable
	{
		public int ExampleInt;
		public NestedSerializable ExampleNested;
		public string ExampleString;

		public void Sync(Message message)
		{
			message.Process(ref ExampleInt);
			message.Process(ref ExampleNested);
			message.Process(ref ExampleString);
		}
	}
}
