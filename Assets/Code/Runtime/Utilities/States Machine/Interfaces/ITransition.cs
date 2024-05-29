namespace SwapChains.Runtime.Utilities.StatesMachine
{
    public interface ITransition
    {
        IState To { get; }
        IPredicate Condition { get; }
    }
}