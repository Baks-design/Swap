using System;

namespace SwapChains.Runtime.Utilities.StatesMachine
{
    public readonly struct FuncPredicate : IPredicate
    {
        readonly Func<bool> func;

        public FuncPredicate(Func<bool> func) => this.func = func;

        public bool Evaluate() => func.Invoke();
    }
}