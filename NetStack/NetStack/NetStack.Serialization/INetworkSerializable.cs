namespace NetStack.Serialization
{
    public interface INetworkSerializable
    {
        void Serialize(BitBuffer buffer);
        void Deserialize(BitBuffer buffer);
    }
}