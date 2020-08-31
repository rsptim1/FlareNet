using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlareNet;

namespace FlareNetTests
{
	[TestClass]
	public class ServerUnitTests
	{
		[TestMethod]
		public void TestServer()
		{
			FlareNetwork.Create(80);
			Assert.IsNotNull(FlareNetwork.FlareClientManager);

			FlareNetwork.RegisterCallback(1, TestCallback);

		}

		private void TestCallback(Message message, IClient client)
		{
			Assert.IsNotNull(message);
			Assert.IsNotNull(client);
		}
	}
}
