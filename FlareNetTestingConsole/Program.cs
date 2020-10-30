using System;
using System.Threading;
using FlareNet;

namespace FlareNetTestingConsole
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// This actually works
			var server = new FlareServer(34377, null);
			var client = new FlareClient("127.0.0.1", 34377);
			Console.ReadKey();
			client.Disconnect();
			server.Disconnect();
		}
	}
}
