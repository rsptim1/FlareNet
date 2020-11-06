using System;
using FlareNet;

namespace FlareNetTestingConsole
{
	public class Program
	{
		private const ushort Port = 34377;

		// TODO: Write this into an actual test
		public static void Main(string[] args)
		{
			// This actually works
			var server = new FlareServer(Port);
			var client = new FlareClient("127.0.0.1", Port);

			// Setup callbacks
			server.RegisterCallback(1, ServerRecTest);
			client.RegisterCallback(1, ClientRecTest);

			// Send tests
			var serverExample = new TestSerializable
			{
				exampleString = "Hello, I am server",
				exampleInt = 18
			};

			var clientExample = new TestSerializable
			{
				exampleInt = 3,
				exampleString = "Greetings, I am client"
			};

			var clientMessage = new Message(1);
			clientMessage.Process(ref clientExample);
			client.SendMessage(clientMessage);

			var serverMessage = new Message(1);
			serverMessage.Process(ref serverExample);
			server.SendMessage(serverMessage);

			Console.ReadKey();
			client.Disconnect();
			server.Disconnect();
		}

		private static void ServerRecTest(Message m, IClient c)
		{
			ReadContents(m);
		}

		private static void ClientRecTest(Message m, IClient c)
		{
			ReadContents(m);
		}

		private static void ReadContents(Message m)
		{
			TestSerializable r = new TestSerializable();
			m.Process(ref r);

			NetworkLogger.Log(r.exampleString);
		}
	}

	public class TestSerializable : ISerializable
	{
		public int exampleInt;
		public string exampleString;

		public void Sync(Message message)
		{
			message.Process(ref exampleInt);
			message.Process(ref exampleString);
		}
	}
}
