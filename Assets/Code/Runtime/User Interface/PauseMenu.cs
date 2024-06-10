using SwapChains.Runtime.GameplayManagement;
using UnityEngine;
using SwapChains.Runtime.Utilities.ServicesLocator;
using SwapChains.Runtime.Entities.Player;

namespace SwapChains.Runtime.UserInterface
{
    public class PauseMenu : MonoBehaviour //TODO: CHECK 
    {
        [SerializeField] GameObject container;
        [SerializeField] IPlayerInput playerInput;

        GameStateManager gameStateManager;

        void Awake() => ServiceLocator.Global.Register(playerInput);

        void Update()
        {
            ServiceLocator.For(this).Get(out playerInput);
            if (playerInput.GetPause())
                OpenMenu();
            if (playerInput.GetUnpause())
                CloseMenu();
        }

        void OpenMenu()
        {
            ServiceLocator.Global.Get(out gameStateManager);
            gameStateManager.UpdateGameState(GameStateManager.GameState.Paused);
            container.SetActive(true);
        }

        void CloseMenu()
        {
            ServiceLocator.Global.Get(out gameStateManager);
            gameStateManager.UpdateGameState(GameStateManager.GameState.Gameplay);
            container.SetActive(false);
        }
    }
}
