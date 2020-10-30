using FlareNet;
using FlareNet.Client;
using FlareNet.Server;
using NUnit.Framework;
using System.Threading;
using Timer = System.Timers.Timer;
using static FlareNetTests.TestConstants;

namespace FlareNetTests
{
	public class FlareClientTests
	{
		FlareClient client;
		string ip;

		[SetUp]
		public void Setup()
		{
			// Search for a UDP broadcast
			//LANBroadcast.BindToPort(TestPort);
			//while (ip == null)
			//{
			//	LANBroadcast.Listen(ProcessMessage);
			//	Thread.Sleep(10);

			//	void ProcessMessage(Message m)
			//	{
			//		m.Process(ref ip);
			//	}
			//}
		}
		
		[Test]
		public void InitiateConnection()
		{
			//Assert.IsNotNull(ip);
			//Assert.IsNotEmpty(ip);

			client = ENetLibrary.Connect("10.59.190.146", TestPort);
			bool isRunning = true;

			// Timer set for a minute
			Timer timer = new Timer(60000);
			timer.Elapsed += (a, b) => isRunning = false;
			timer.Start();

			while (isRunning)
			{
				ENetLibrary.Update();

				if (client.IsUnableToConnect)
					break;
			}

			Assert.IsTrue(client.IsConnected);
		}
	}
}
