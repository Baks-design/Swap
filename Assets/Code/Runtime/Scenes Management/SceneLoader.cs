using System.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

namespace SwapChains.Runtime.ScenesManagement
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] float fillSpeed = 0.5f;
        [SerializeField, Child] Image loadingBar;
        [SerializeField, Child] Canvas loadingCanvas;
        [SerializeField, Child] CinemachineCamera loadingCamera;
        [SerializeField] SceneGroup[] sceneGroups;
        float targetProgress;
        bool isLoading;
        public readonly SceneGroupManager manager = new();

        public SceneGroup[] SceneGroups => sceneGroups;

        void OnValidate() => this.ValidateRefs();

        async void Start() => await LoadSceneGroup(0);

        void Update()
        {
            if (!isLoading)
                return;

            var currentFillAmount = loadingBar.fillAmount;
            var progressDifference = Mathf.Abs(currentFillAmount - targetProgress);

            var dynamicFillSpeed = progressDifference * fillSpeed;

            var delta = Time.deltaTime;
            loadingBar.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, delta * dynamicFillSpeed);
        }

        public async Task LoadSceneGroup(int index)
        {
            loadingBar.fillAmount = 0f;
            targetProgress = 1f;

            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError($"Invalid scene group index: {index}");
                return;
            }

            var progress = new LoadingProgress();
            progress.Progressed += target => targetProgress = Mathf.Max(target, targetProgress);

            EnableLoadingCanvas();
            await manager.LoadScenes(sceneGroups[index], progress);
            EnableLoadingCanvas(false);
        }

        void EnableLoadingCanvas(bool enable = true)
        {
            isLoading = enable;
            loadingCanvas.gameObject.SetActive(enable);
            loadingCamera.gameObject.SetActive(enable);
        }
    }
}
