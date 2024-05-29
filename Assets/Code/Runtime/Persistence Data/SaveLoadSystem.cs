using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapChains.Runtime.PersistenceData
{
    public class SaveLoadSystem : MonoBehaviour
    {
        [SerializeField] GameData gameData;
        IDataService dataService;

        public GameData GameData => gameData;

        void Awake() => dataService = new FileDataService(new JsonSerializer());

        void Start() => NewGame();

        public void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (entity != null)
            {
                data ??= new TData { Id = entity.Id };
                entity.Bind(data);
            }
        }

        public void Bind<T, TData>(List<TData> datas) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entities = FindObjectsByType<T>(FindObjectsSortMode.None);
            for (var i = 0; i < entities.Length; i++)
            {
                var data = datas.FirstOrDefault(d => d.Id.Equals(entities[i].Id));
                if (data is null)
                {
                    data = new TData { Id = entities[i].Id };
                    datas.Add(data);
                }
                entities[i].Bind(data);
            }
        }

        public void NewGame()
        {
            gameData = new GameData { Name = "Game", CurrentLevelName = "Demo" };
            SceneManager.LoadScene(gameData.CurrentLevelName);
        }

        public void SaveGame() => dataService.Save(gameData);

        public void LoadGame(string gameName)
        {
            gameData = dataService.Load(gameName);
            if (string.IsNullOrWhiteSpace(gameData.CurrentLevelName))
                gameData.CurrentLevelName.Equals("Demo");
            SceneManager.LoadScene(gameData.CurrentLevelName);
        }

        public void ReloadGame() => LoadGame(gameData.Name);

        public void DeleteGame(string gameName) => dataService.Delete(gameName);
    }
}
