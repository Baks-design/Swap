using SwapChains.Runtime.Utilities.Helpers;

namespace SwapChains.Runtime.GameplayManagement
{
    public class GameGameplayState : GameBaseState
    {
        public GameGameplayState(GameStateManager game) : base(game) { }

        public override void OnEnter()
        {
            GameHelper.ShowCursor(false);
            GameHelper.SetTimeScale(1f);
            GameHelper.EnableHumanoidMap();
        }
    }
}
