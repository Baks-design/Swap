using SwapChains.Runtime.Utilities.StatesMachine;

namespace SwapChains.Runtime.GameplayManagement
{
    public abstract class GameBaseState : IState
    {
        protected readonly GameStateManager game;

        protected GameBaseState(GameStateManager game) => this.game = game;

        public virtual void OnEnter() { }

        public virtual void FixedUpdate() { }

        public virtual void Update() { }

        public virtual void OnExit() { }
    }
}
