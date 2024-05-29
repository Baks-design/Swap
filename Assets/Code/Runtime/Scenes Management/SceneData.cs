using System;
using Eflatun.SceneReference;

namespace SwapChains.Runtime.ScenesManagement
{
    [Serializable]
    public struct SceneData
    {
        public SceneReference Reference;
        public SceneType SceneType;

        public readonly string Name => Reference.Name;
    }
}
