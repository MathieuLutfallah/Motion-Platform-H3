using UnityEngine;

namespace Ezereal
{
    public class SteeringWheelVisual : MonoBehaviour
    {
        [SerializeField] EzerealCarController car;

        [Header("Settings")]
        public float maxWheelRotation = 450f;

        float currentRotation;

        void Update()
        {
            if (car == null) return;

            float steerPercent = car.frontLeftWheelCollider.steerAngle / car.maxSteerAngle;

            float targetRotation = steerPercent * maxWheelRotation;

            currentRotation = Mathf.Lerp(currentRotation, targetRotation, Time.deltaTime * 10f);

            transform.localRotation = Quaternion.Euler(0f, 0f, -currentRotation);
        }
    }
}