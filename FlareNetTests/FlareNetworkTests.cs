using FlareNet;
using FlareNet.Client;
using FlareNet.Debug;
using FlareNet.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace FlareNetTests
{
	[TestClass]
	public class FlareNetworkTests
	{
		private Thread networkUpdateThread;
		private FlareServer server;
		private FlareClient client;

		private const int ExampleInt = 1800;
		private const byte ExampleByte = 18;
		private const string ExampleString = "FlareNet";
		private const bool ExampleBool = true;

		private const ushort port = 34377;

		private int countdown;

		[TestMethod]
		public void LocalHostTest()
		{
			server = FlareNetwork.Create(port);
			Assert.IsNotNull(server);

			client = FlareNetwork.Connect("127.0.0.1", port);
			Assert.IsNotNull(client);

			server.RegisterCallback(1, TestCallback);
			server.RegisterCallback(1, ServerTestCallback);
			client.RegisterCallback(1, TestCallback);
			client.RegisterCallback(1, ClientTestCallback);

			//networkUpdateThread = new Thread(FlareNetwork.Update);

			countdown = 8;
			SendTestMessage(server);

			while (countdown > 0)
			{
				FlareNetwork.Update();
			}
		}

		private void SendTestMessage(FlareClient client)
		{
			TestSerializable test = new TestSerializable
			{
				ExampleInt = ExampleInt,
				ExampleString = ExampleString,
				ExampleNested = new NestedSerializable
				{
					ExampleByte = ExampleByte,
					ExampleBool = ExampleBool
				}
			};

			Message m = new Message(1);
			m.Process(ref test);
			client.SendMessage(m);
			NetworkLogger.Log("sent");
		}

		private void TestCallback(Message message, IClient client)
		{
			Assert.IsNotNull(message);
			Assert.IsNotNull(client);

			NetworkLogger.Log("Reading message");

			TestSerializable test = default;
			message.Process(ref test);

			Assert.Equals(test.ExampleInt, ExampleInt);
			Assert.Equals(test.ExampleString, ExampleString);
			Assert.IsNotNull(test.ExampleNested);
			Assert.Equals(test.ExampleNested.ExampleByte, ExampleByte);
			Assert.Equals(test.ExampleNested.ExampleBool, ExampleBool);
		}

		private void ServerTestCallback(Message message, IClient client)
		{
			NetworkLogger.Log("Server received");
			if (--countdown > 0)
			{
				SendTestMessage(this.server);
			}
		}

		private void ClientTestCallback(Message message, IClient client)
		{
			NetworkLogger.Log("Client received");
			if (--countdown > 0)
			{
				SendTestMessage(this.client);
			}
		}
	}
}
