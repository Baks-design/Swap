using SwapChains.Runtime.Utilities.Helpers;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SwapChains.Runtime.UserInterface
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject menuContainer;
        [SerializeField, Child] Button quitButton;

        void OnValidate() => this.ValidateRefs();

        void Awake() => menuContainer.SetActive(SceneManager.GetActiveScene().name.Equals("Menu"));

        void OnEnable() => quitButton.onClick.AddListener(() => GameHelper.QuitGame());

        void OnDisable() => quitButton.onClick.RemoveListener(() => GameHelper.QuitGame());
    }
}
