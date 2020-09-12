using FlareNet.Client;
using FlareNet.Server;
using NUnit.Framework;
using Network = FlareNet.FlareNetwork;

namespace FlareNetTests
{
	public class FlareNetwork
	{
		FlareServer server;
		FlareClient client;

		[SetUp]
		public void Setup()
		{
			server = Network.Create(34377);
		}

		[Test]
		public void LocalHostConnectionTest()
		{
			Assert.IsNotNull(server, "Server is null");
			client = Network.Connect("127.0.0.1", 34377);

			while (client.ClientState == ENet.PeerState.Connecting)
			{
				Network.Update();
			}

			Assert.IsFalse(client.IsUnableToConnect);

			client.Disconnect();
			server.Disconnect();
		}

		[Test]
		public void DisconnectTest()
		{
			Assert.IsNotNull(server);

			server.Disconnect();
		}
	}
}
