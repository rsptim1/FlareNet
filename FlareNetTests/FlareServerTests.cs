using FlareNet;
using FlareNet.Server;
using NUnit.Framework;
using System.Timers;
using static FlareNetTests.TestConstants;


namespace FlareNetTests
{
	public class FlareServerTests
	{
		FlareServer server;

		[SetUp]
		public void Setup()
		{
			server = ENetLibrary.Create(TestPort);
		}

		[Test]
		public void WaitForConnection()
		{
			bool isRunning = true;
			string ip = server.IpAddress;
			LANBroadcast.BindToPort(TestPort);

			// Timer set for a minute
			Timer timer = new Timer(60000);
			timer.Elapsed += (a, b) => isRunning = false;
			timer.Start();

			while (isRunning)
			{
				ENetLibrary.Update();

				if (server.ClientCount > 0)
					break;
				else
				{
					Message m = new Message(0);
					m.Process(ref ip);

					LANBroadcast.Send(m, TestPort);
				}
			}

			Assert.Greater(server.ClientCount, 0);
		}
	}
}
