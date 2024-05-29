using System;
using System.Collections.Generic;
using System.Linq;

namespace SwapChains.Runtime.ScenesManagement
{
    public enum SceneType
    {
        ActiveScene,
        MainMenu,
        UserInterface,
        HUD,
        Cinematic,
        Environment,
        Tooling
    }

    [Serializable]
    public struct SceneGroup
    {
        public string GroupName;
        public List<SceneData> Scenes;

        public readonly string FindSceneNameByType(SceneType sceneType)
        => Scenes.FirstOrDefault(scene => scene.SceneType.Equals(sceneType)).Reference.Name;
    }
}
