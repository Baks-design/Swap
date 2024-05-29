namespace SwapChains.Runtime.Utilities.StatesMachine
{
    public interface IState
    {
        void OnEnter();
        void FixedUpdate();
        void Update();
        void OnExit();
    }
}