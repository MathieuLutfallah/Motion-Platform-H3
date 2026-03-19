using UnityEngine;
using UnityEngine.InputSystem;

namespace Ezereal
{
    public class EzerealCameraController : MonoBehaviour
    {
        [SerializeField] CameraViews currentCameraView = CameraViews.cockpit;

        [SerializeField] private GameObject[] cameras;

        private int cameraCount;

        void Awake()
        {
            cameraCount = cameras.Length;

            currentCameraView = CameraViews.cockpit;
            SetCameraView(currentCameraView);
        }

        public void OnSwitchCamera(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            int nextIndex = ((int)currentCameraView + 1) % cameraCount;

            currentCameraView = (CameraViews)nextIndex;

            SetCameraView(currentCameraView);
        }

        void SetCameraView(CameraViews view)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].SetActive(i == (int)view);
            }
        }
    }
}