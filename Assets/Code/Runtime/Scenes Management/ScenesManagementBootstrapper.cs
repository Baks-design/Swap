using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SwapChains.Runtime.ScenesManagement
{
    public class ScenesManagementBootstrapper : MonoBehaviour
    {
        // NOTE: This script is intended to be placed in your first scene included in the build settings.
        static readonly int sceneIndex = 0;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitEditor()
        {
            Debug.Log("Bootstrapper...");

            // Set the bootstrapper scene to be the play mode start scene when running in the editor
            // This will cause the bootstrapper scene to be loaded first (and only once) when entering
            // play mode from the Unity Editor, regardless of which scene is currently active.
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[sceneIndex].path);
        }
#elif UNITY_STANDALONE
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void InitBuild()
        {
            Debug.Log("Bootstrapper...");

            await SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
        }
#endif
    }
}