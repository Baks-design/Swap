using KBCore.Refs;
using R3;
using UnityEngine;

namespace SwapChains.Runtime.Entities
{
    public abstract class PlayerSignals : ValidatedMonoBehaviour
    {
        public abstract float StrideLength { get; }
        public abstract Observable<Vector3> Walked { get; }
        public abstract Observable<Unit> Landed { get; }
        public abstract Observable<Unit> Jumped { get; }
        public abstract Observable<Unit> Stepped { get; }
    }
}