using ENet;

namespace FlareNet
{
	public interface IClient
	{
		uint Id { get; }
		string IpAddress { get; }
		ushort Port { get; }
		ulong TotalDataIn { get; }
		ulong TotalDataOut { get; }
		uint Ping { get; }

		Peer Peer { get; }
	}
}
