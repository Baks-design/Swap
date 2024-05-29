using KBCore.Refs;
using UnityEngine;

namespace SwapChains.Runtime.Entities
{
    public class Interactable : ValidatedMonoBehaviour
    {
        [SerializeField] int currentHealth = 3;
        [SerializeField, Self] Rigidbody body;

        public void Damage(int damageAmount)
        {
            currentHealth -= damageAmount;
            if (currentHealth <= 0)
                gameObject.SetActive(false);
        }
    }
}