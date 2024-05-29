#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SwapChains.Runtime.PersistenceData
{
    [CustomEditor(typeof(SaveLoadSystem))]
    public class SaveManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var saveLoadSystem = (SaveLoadSystem)target;
            var gameName = saveLoadSystem.GameData.Name;

            DrawDefaultInspector();

            if (GUILayout.Button("New Game"))
                saveLoadSystem.NewGame();
            if (GUILayout.Button("Save Game"))
                saveLoadSystem.SaveGame();
            if (GUILayout.Button("Load Game"))
                saveLoadSystem.LoadGame(gameName);
            if (GUILayout.Button("Delete Game"))
                saveLoadSystem.DeleteGame(gameName);
        }
    }
}
#endif
