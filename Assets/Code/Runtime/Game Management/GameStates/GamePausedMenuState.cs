using SwapChains.Runtime.Utilities.Helpers;

namespace SwapChains.Runtime.GameplayManagement
{
    public class GamePausedMenuState : GameBaseState
    {
        public GamePausedMenuState(GameStateManager game) : base(game) { }

        public override void OnEnter()
        {
            GameHelper.ShowCursor(true);
            GameHelper.SetTimeScale(0f);
            GameHelper.EnableUIMap();
        }
    }
}
