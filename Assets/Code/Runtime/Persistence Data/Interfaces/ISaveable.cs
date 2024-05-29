namespace SwapChains.Runtime.PersistenceData
{
    public interface ISaveable
    {
        SerializableGuid Id { get; set; }
    }
}
