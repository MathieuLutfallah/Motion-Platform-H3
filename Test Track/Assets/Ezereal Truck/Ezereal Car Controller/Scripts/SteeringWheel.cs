using UnityEngine;

namespace Ezereal
{
    // steering wheel visual
    public class SteeringWheelVisual : MonoBehaviour
    {
        [SerializeField] EzerealCarController car;

        [Header("Settings")]
        public float maxWheelRotation = 450f;

        float currentRotation;

        void Update()
        {
            if (car == null) return;

            // steer ratio
            float steerPercent = car.frontLeftWheelCollider.steerAngle / car.maxSteerAngle;

            float targetRotation = steerPercent * maxWheelRotation;

            // smooth
            currentRotation = Mathf.Lerp(currentRotation, targetRotation, Time.deltaTime * 10f);

            // apply
            transform.localRotation = Quaternion.Euler(0f, 0f, -currentRotation);
        }
    }
}