using System;

namespace SwapChains.Runtime.PersistenceData
{
    [Serializable]
    public struct GameData
    {
        public string Name;
        public string CurrentLevelName;
        public PlayerData PlayerData;
    }
}
