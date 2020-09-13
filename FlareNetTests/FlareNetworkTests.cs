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
			server = FlareNetwork.Create(TestPort);
		}

		[Test]
		public void ConnectLocal()
		{
			Assert.IsNotNull(server, "Server is null");
			FlareNetwork.Update();

			client = FlareNetwork.Connect(Loopback, TestPort);

			while (client.ClientState == ENet.PeerState.Connecting)
			{
				FlareNetwork.Update();
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
