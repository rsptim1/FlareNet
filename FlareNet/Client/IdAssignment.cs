namespace FlareNet
{
	[NetworkTag(NetworkTags.IdAssignment)]
	internal struct IdAssignment : INetworkPayload
	{
		internal uint id;

		void ISerializable.Sync(Message message)
		{
			message.Process(ref id);
		}
	}

	[NetworkTag(NetworkTags.IdAssignment)]
	internal struct ClientAssigned : INetworkPayload
	{
		internal uint id;

		void ISerializable.Sync(Message message)
		{
			message.Process(ref id);
		}
	}
}
