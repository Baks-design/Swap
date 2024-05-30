using R3;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Player
{
    public interface IPlayerSignals
    {
        public float StrideLength { get; }
        public Observable<Vector3> Walked { get; }
        public Observable<Unit> Landed { get; }
        public Observable<Unit> Jumped { get; }
        public Observable<Unit> Stepped { get; }
    }
}