using System;
using UnityEngine;

namespace SwapChains.Runtime.PersistenceData
{
    [Serializable]
    public struct PlayerData : ISaveable
    {
        public Vector3 Position;
        public Quaternion Rotation;

        [field: SerializeField] public SerializableGuid Id { get; set; }
    }
}
