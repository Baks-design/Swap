using SwapChains.Runtime.Entities.Player;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Environment
{
    [RequireComponent(typeof(Rigidbody))]
    public class DealDamage : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
                player.ReceiveDamage(1);
        }
    }
}