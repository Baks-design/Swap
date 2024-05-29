namespace SwapChains.Runtime.PersistenceData
{
    public interface IBind<TData> where TData : ISaveable
    {
        SerializableGuid Id { get; set; }

        void Bind(TData data);
    }
}
