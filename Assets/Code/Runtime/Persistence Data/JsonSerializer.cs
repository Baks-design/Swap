using UnityEngine;

namespace SwapChains.Runtime.PersistenceData
{
    public struct JsonSerializer : ISerializer
    {
        public readonly string Serialize<T>(T obj) => JsonUtility.ToJson(obj, true);

        public readonly T Deserialize<T>(string json) => JsonUtility.FromJson<T>(json);
    }
}
