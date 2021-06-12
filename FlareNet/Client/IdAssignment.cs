namespace FlareNet
{
	[NetworkTag(NetworkTags.IdAssignment)]
	internal struct IdAssignment : INetworkPayload
	{
		internal uint id;

		public void Sync(Message message)
		{
			message.Process(ref id);
		}
	}
}
