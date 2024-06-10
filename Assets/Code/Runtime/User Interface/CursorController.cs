using System.Buffers;
using KBCore.Refs;
using SwapChains.Runtime.Entities.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SwapChains.Runtime.UserInterface
{
    [RequireComponent(typeof(Image))]
    public class CursorController : MonoBehaviour
    {
        [SerializeField] float range = 50f;
        [SerializeField] LayerMask entityMask;
        [SerializeField, Self] Image cursor;

        void OnValidate() => this.ValidateRefs();

        void Start()
        {
            cursor.enabled = true;
            cursor.color = new Color(1f, 1f, 1f, 0.5f);
        }

        void Update() => ChangeCursorOnEntityCollision();

        void ChangeCursorOnEntityCollision()
        {
            Ray ray = default;
            ray.origin = Camera.main.transform.position;
            ray.direction = Camera.main.transform.forward;
            var raycastHitBuffer = ArrayPool<RaycastHit>.Shared.Rent(2);

            var hitCount = Physics.RaycastNonAlloc(ray, raycastHitBuffer, range, entityMask);
            for (var i = 0; i < hitCount; i++)
                cursor.color = Color.red;
            if (hitCount == 0)
                cursor.color = Color.white;
        }
    }
}
