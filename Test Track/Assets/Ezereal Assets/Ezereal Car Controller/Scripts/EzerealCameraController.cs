using UnityEngine;
using UnityEngine.InputSystem;

namespace Ezereal
{
    // Controls switching between different camera views
    public class EzerealCameraController : MonoBehaviour
    {
        [SerializeField] CameraViews currentCameraView = CameraViews.cockpit;
        [SerializeField] private GameObject[] cameras;

        private int cameraCount;

        // Called once when the object is created
        // Use this to initialize values and set default state
        void Awake()
        {
            cameraCount = cameras.Length;

            currentCameraView = CameraViews.cockpit;

            // Apply initial camera setup
            SetCameraView(currentCameraView);
        }

        // Called when an input action is triggered
        // Used here to switch between cameras
        public void OnSwitchCamera(InputAction.CallbackContext ctx)
        {    
            if (!ctx.performed)
                return;

            // Move to next camera index and wrap around at the end
            int nextIndex = ((int)currentCameraView + 1) % cameraCount;

            currentCameraView = (CameraViews)nextIndex;

            // Apply new camera state
            SetCameraView(currentCameraView);
        }

        // Activates one camera and disables all others
        // Ensures only one view is visible at a time
        void SetCameraView(CameraViews view)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].SetActive(i == (int)view);
            }
        }
    }
}