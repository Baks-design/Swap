using UnityEngine;

namespace XIV.DesignPatterns.Common.Utils
{
    [System.Serializable]
    public struct Timer
    {
        [field : SerializeField]
        public float duration { get; private set; }
        public float currentTime { get; private set; }

        public float normalizedTime => currentTime / duration;
        public float normalizedTimePingPong
        {
            get
            {
                var t = normalizedTime;
                return t > 0.5f ? (1f - t) / 0.5f : t / 0.5f;
            }
        }

        public Timer(float duration)
        {
            this.duration = duration;
            currentTime = 0f;
        }

        public bool Update(float dt)
        {
            currentTime = Mathf.Clamp(currentTime + dt, 0, duration);
            return IsDone();
        }

        public bool IsDone()
        {
            return duration - currentTime < Mathf.Epsilon;
        }

        public void Reset()
        {
            currentTime = 0f;
        }

        public override string ToString()
        {
            return $"{currentTime}/{duration} = {normalizedTime}";
        }
    }
}