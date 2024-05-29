#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace SwapChains.Runtime.ScenesManagement
{
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var sceneLoader = (SceneLoader)target;

            if (EditorApplication.isPlaying)
            {
                if (sceneLoader.SceneGroups != null)
                {
                    for (var i = 0; i < sceneLoader.SceneGroups.Length; i++)
                    {
                        var group = sceneLoader.SceneGroups[i];

                        if (GUILayout.Button($"Load {group.GroupName ?? $"Scene Group {i + 1}"} "))
                            LoadSceneGroup(sceneLoader, i);
                    }
                }
            }
        }

        static async void LoadSceneGroup(SceneLoader sceneLoader, int index)
        => await sceneLoader.LoadSceneGroup(index);
    }
}
#endif