using SwapChains.Runtime.GameplayManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using SwapChains.Runtime.Utilities.ServicesLocator;

namespace SwapChains.Runtime.UserInterface
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject container;
        InputAction pauseAction;
        InputAction unpauseAction;
        GameStateManager gameStateManager; 

        void Awake()
        {
            pauseAction = InputSystem.actions.FindAction("Pause");
            unpauseAction = InputSystem.actions.FindAction("Unpause");
        }

        void Update()
        {
            if (pauseAction.WasPressedThisFrame())
                OpenMenu();
            if (unpauseAction.WasPressedThisFrame())
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
