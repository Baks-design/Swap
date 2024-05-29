using SwapChains.Runtime.Utilities.Helpers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SwapChains.Runtime.UserInterface
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField] float range = 50f;
        [SerializeField] LayerMask entityMask;
        [SerializeField] Image image;
        Camera mainCam;
        readonly RaycastHit[] hits = new RaycastHit[1];

        void Start()
        {
            mainCam = Camera.main;
            image.gameObject.SetActive(true);
            image.color = new Color(1f, 1f, 1f, 0.5f);
        }

        void Update() => ChangeCursorOnEntityCollision();

        void ChangeCursorOnEntityCollision()
        {
            var (isCollider, _) = GameHelper.CheckInteraction(
                mainCam, hits, range, entityMask, QueryTriggerInteraction.Ignore);
            image.color = isCollider ? Color.red : Color.white;
        }
    }
}
