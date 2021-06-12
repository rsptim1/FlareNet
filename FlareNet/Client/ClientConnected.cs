namespace FlareNet
{
	[NetworkTag(NetworkTags.ClientConnected)]
	public struct ClientConnected : INetworkPayload
	{
		public IClient Client;

		void ISerializable.Sync(Message message)
		{
			// Do nothing - this never gets sent
		}
	}
}
