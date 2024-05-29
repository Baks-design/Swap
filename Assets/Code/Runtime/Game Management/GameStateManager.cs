using SwapChains.Runtime.Utilities.ServicesLocator;
using SwapChains.Runtime.Utilities.StatesMachine;
using UnityEngine;

namespace SwapChains.Runtime.GameplayManagement
{
    public class GameStateManager : MonoBehaviour
    {
        public enum GameState
        {
            Gameplay,
            Paused,
            GameOver,
            Loading
        }

        StateMachine stateMachine;
        GameState currentGameState;

        public GameState CurrentGameState => currentGameState;

        void Awake()
        {
            ServiceLocator.Global.Register(this);
            UpdateGameState(GameState.Gameplay);
            SetupStateMachine();
        }

        void FixedUpdate() => stateMachine.FixedUpdate();

        void Update() => stateMachine.Update();

        #region STATE MACHINE
        void SetupStateMachine()
        {
            stateMachine = new StateMachine();

            var pausedState = new GamePausedMenuState(this);
            var gameplayState = new GameGameplayState(this);

            stateMachine.AddAnyTransition(gameplayState, new FuncPredicate(ReturnToGameplayState));
            stateMachine.AddTransition(gameplayState, pausedState, new FuncPredicate(() => currentGameState == GameState.Paused));

            stateMachine.SetState(gameplayState);
        }

        bool ReturnToGameplayState() => currentGameState == GameState.Gameplay;
        #endregion

        public void UpdateGameState(GameState newGameState)
        {
            if (newGameState == CurrentGameState)
                return;

            currentGameState = newGameState;
        }
    }
}
