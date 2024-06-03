using System.Collections.Generic;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Player.Movement
{
    public class CameraWaterCheck : MonoBehaviour
    {
        readonly List<Collider> triggers = new();

        void OnTriggerEnter(Collider other)
        {
            if (!triggers.Contains(other))
                triggers.Add(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (triggers.Contains(other))
                triggers.Remove(other);
        }

        public bool IsUnderwater()
        {
            for (var i = 0; i < triggers.Count; i++)
                if (triggers[i].GetComponentInParent<Water>())
                    return true;
            return false;
        }
    }
}