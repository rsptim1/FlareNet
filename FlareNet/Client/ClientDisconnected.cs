namespace FlareNet
{
	[NetworkTag(NetworkTags.ClientDisconnected)]
	public struct ClientDisconnected : INetworkPayload
	{
		public uint ClientId;

		void ISerializable.Sync(Message message)
		{
			// Do nothing - this never gets sent
		}
	}
}
