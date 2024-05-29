using SwapChains.Runtime.Utilities.Helpers;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SwapChains.Runtime.UserInterface
{
    public class MainMenu : ValidatedMonoBehaviour
    {
        [SerializeField] GameObject menuContainer;
        [SerializeField, Child] Button quitButton;

        void Awake() => menuContainer.SetActive(SceneManager.GetActiveScene().name is "Menu");

        void OnEnable() => quitButton.onClick.AddListener(() => GameHelper.QuitGame());

        void OnDisable() => quitButton.onClick.RemoveListener(() => GameHelper.QuitGame());
    }
}
