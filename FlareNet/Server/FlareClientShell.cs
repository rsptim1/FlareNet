using ENet;
using FlareNet.Client;

namespace FlareNet.Server
{
	internal class FlareClientShell : FlareClientBase
	{
		public FlareClientShell(Peer peer)
		{
			Peer = peer;
		}

		public override void Disconnect()
		{
			Peer.DisconnectNow(0);
		}

		public void Send(byte channel, Packet packet)
		{
			Peer.Send(channel, ref packet);
		}
	}
}
