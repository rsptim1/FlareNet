using System.Net;
using System.Net.Sockets;

namespace FlareNet.Server
{
	public static class LANBroadcast
	{
		public delegate void BroadcastMessageRecieved(Message m);
		private static UdpClient UdpClient = new UdpClient();
		private static IPEndPoint from = new IPEndPoint(0, 0);

		public static void BindToPort(ushort port)
		{
			UdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
		}

		public static void Listen(BroadcastMessageRecieved callback)
		{
			var recvBuffer = UdpClient.Receive(ref from);

			if (recvBuffer != null && recvBuffer.Length > 0)
			{
				Message message = new Message(recvBuffer);

				callback?.Invoke(message);
			}
		}

		public static void Send(Message m, ushort port)
		{
			var data = m.GetBufferArray();
			UdpClient.Send(data, data.Length, "255.255.255.255", port);
		}
	}
}
