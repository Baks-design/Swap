using System;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.Helpers
{
    [Serializable]
    public struct Timer
    {
        [field: SerializeField] public float Duration { get; private set; }
        public float CurrentTime { get; private set; }
        public readonly float NormalizedTime => CurrentTime / Duration;
        public readonly float NormalizedTimePingPong
        {
            get
            {
                var t = NormalizedTime;
                return t > 0.5f ? (1f - t) / 0.5f : t / 0.5f;
            }
        }

        public Timer(float duration)
        {
            Duration = duration;
            CurrentTime = 0f;
        }

        public bool Update(float dt)
        {
            CurrentTime = Mathf.Clamp(CurrentTime + dt, 0f, Duration);
            return IsDone();
        }

        public readonly bool IsDone() => Duration - CurrentTime < Mathf.Epsilon;

        public void Reset() => CurrentTime = 0f;

        public override readonly string ToString() => $"{CurrentTime}/{Duration} = {NormalizedTime}";
    }
}