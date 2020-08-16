namespace FlareNet.Server
{
	public class ServerConfig
	{
		/// <summary>
		/// The maximum amount of strikes before a client is banned.
		/// </summary>
		public byte MaxStrikes = 3;

		/// <summary>
		/// Auto-ban a client when it reaches MaxStrikes?
		/// </summary>
		public bool BanOnMaxStrikes = true;

		/// <summary>
		/// The time in seconds that a client can be banned from connecting to the server.
		/// </summary>
		public int BanTimeAmount = 1440;

		/// <summary>
		/// The maximum connections a server can have.
		/// </summary>
		public int MaxConnections = 16;
	}
}
