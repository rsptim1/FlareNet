using System.Net;

namespace FlareNetTests
{
	public static class TestConstants
	{
		public const ushort TestPort = 34377;
		public static string Loopback => IPAddress.Loopback.ToString();
	}
}
