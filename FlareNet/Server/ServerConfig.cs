namespace FlareNet
{
	public class ServerConfig
	{
		internal const int DefaultChannelCount = 1;

		/// <summary>
		/// The maximum connections a server can have.
		/// </summary>
		public int MaxConnections = 32;

		/// <summary>
		/// The amount of channels available for sending messages.
		/// </summary>
		public int ChannelCount = DefaultChannelCount;
	}
}
