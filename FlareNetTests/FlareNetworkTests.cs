using FlareNet;
using FlareNet.Client;
using FlareNet.Server;
using NUnit.Framework;
using static FlareNetTests.TestConstants;

namespace FlareNetTests
{
	public class FlareNetworkTests
	{
		FlareServer server;
		FlareClient client;

		[SetUp]
		public void Setup()
		{
			server = ENetLibrary.Create(TestPort);
		}

		[Test]
		public void ConnectLocal()
		{
			Assert.IsNotNull(server, "Server is null");
			ENetLibrary.Update();

			client = ENetLibrary.Connect(Loopback, TestPort);

			while (client.ClientState == ENet.PeerState.Connecting)
			{
				ENetLibrary.Update();
			}

			Assert.IsFalse(client.IsUnableToConnect);

			client.Disconnect();
			server.Disconnect();
		}

		[Test]
		public void Disconnect()
		{
			Assert.IsNotNull(server);

			server.Disconnect();
		}
	}
}
