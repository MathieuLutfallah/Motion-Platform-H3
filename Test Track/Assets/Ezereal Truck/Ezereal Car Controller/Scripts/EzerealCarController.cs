using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Ezereal
{
    public enum Gear
    {
        Reverse,
        Neutral,
        Drive
    }

    public class EzerealCarController : MonoBehaviour
    {
        public Rigidbody vehicleRB;

        public WheelCollider frontLeftWheelCollider;
        public WheelCollider frontRightWheelCollider;
        public WheelCollider rearLeftWheelCollider;
        public WheelCollider rearRightWheelCollider;

        [SerializeField] Transform frontLeftWheelMesh;
        [SerializeField] Transform frontRightWheelMesh;
        [SerializeField] Transform rearLeftWheelMesh;
        [SerializeField] Transform rearRightWheelMesh;

        [SerializeField] TMP_Text currentSpeedTMP_UI;
        [SerializeField] TMP_Text currentSpeedTMP_Dashboard;
        [SerializeField] TMP_Text currentGearTMP_Dashboard;

        [Header("Engine")]
        public float maxForwardSpeed = 140f;
        public float horsePower = 4000f;

        [Header("Brakes")]
        public float brakePower = 4000f;

        [Header("Steering")]
        public float maxSteerAngle = 30f;
        public float steeringSpeed = 5f;

        [Header("Stability")]
        public float drag = 0.1f;
        public float angularDrag = 1.5f;

        float currentSpeed;
        float throttleInput;
        float brakeInput;
        float targetSteerAngle;
        float currentSteerAngle;

        public float ThrottleInput => throttleInput;

        public bool stationary = true;
        public Gear currentGear = Gear.Neutral;

        WheelCollider[] wheels;

        void Awake()
        {
            Application.runInBackground = true;

            vehicleRB.linearDamping = drag;
            vehicleRB.angularDamping = angularDrag;

            wheels = new WheelCollider[]
            {
                frontLeftWheelCollider,
                frontRightWheelCollider,
                rearLeftWheelCollider,
                rearRightWheelCollider
            };
        }

        public void OnSteer(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();

            float curved = Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), 1.4f);

            float speedFactor = Mathf.InverseLerp(0, 120f, Mathf.Abs(currentSpeed));
            float dynamic = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.35f, speedFactor);

            targetSteerAngle = curved * dynamic;
        }

        public void OnThrottle(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();
            throttleInput = Mathf.Clamp01(value);
        }

        public void OnBrake(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();
            brakeInput = Mathf.Clamp01(value);
        }

        public void OnUpShift(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            currentGear = Gear.Drive;
        }

        public void OnDownShift(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            currentGear = Gear.Reverse;
        }

        public void OnRestart(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void FixedUpdate()
        {
            UpdateSpeed();
            UpdateGearDisplay();
            ApplyMotor();
            ApplyBrakes();
            ApplySteering();
            UpdateStationary();
        }

        void ApplyMotor()
        {
            float speedFactor = Mathf.InverseLerp(0, maxForwardSpeed, Mathf.Abs(currentSpeed));
            float torque = Mathf.Lerp(horsePower, horsePower * 0.25f, speedFactor);

            float finalTorque = 0f;

            if (currentGear == Gear.Drive)
                finalTorque = torque * throttleInput;
            else if (currentGear == Gear.Reverse)
                finalTorque = -torque * throttleInput;
            else
                finalTorque = 0f;

            if (brakeInput > 0.1f)
                finalTorque = 0f;

            rearLeftWheelCollider.motorTorque = finalTorque;
            rearRightWheelCollider.motorTorque = finalTorque;
        }

        void ApplyBrakes()
        {
            float brakeTorque = brakeInput * brakePower;

            frontLeftWheelCollider.brakeTorque = brakeTorque;
            frontRightWheelCollider.brakeTorque = brakeTorque;

            rearLeftWheelCollider.brakeTorque = brakeTorque * 0.5f;
            rearRightWheelCollider.brakeTorque = brakeTorque * 0.5f;
        }

        void ApplySteering()
        {
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * steeringSpeed);

            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle;

            UpdateWheel(frontLeftWheelCollider, frontLeftWheelMesh);
            UpdateWheel(frontRightWheelCollider, frontRightWheelMesh);
            UpdateWheel(rearLeftWheelCollider, rearLeftWheelMesh);
            UpdateWheel(rearRightWheelCollider, rearRightWheelMesh);
        }

        void UpdateWheel(WheelCollider col, Transform mesh)
        {
            col.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.SetPositionAndRotation(pos, rot);
        }

        void UpdateSpeed()
        {
#if UNITY_6000_0_OR_NEWER
            currentSpeed = vehicleRB.linearVelocity.magnitude * 3.6f;
#else
            currentSpeed = vehicleRB.velocity.magnitude * 3.6f;
#endif

            int displaySpeed = Mathf.RoundToInt(currentSpeed);
            currentSpeedTMP_Dashboard.text = displaySpeed.ToString();
        }

        void UpdateGearDisplay()
        {
            if (currentGear == Gear.Drive)
                currentGearTMP_Dashboard.text = "D";
            else if (currentGear == Gear.Reverse)
                currentGearTMP_Dashboard.text = "R";
            else
                currentGearTMP_Dashboard.text = "N";
        }

        void UpdateStationary()
        {
            if (
                Mathf.Abs(frontLeftWheelCollider.rpm) < 1f &&
                Mathf.Abs(frontRightWheelCollider.rpm) < 1f &&
                Mathf.Abs(rearLeftWheelCollider.rpm) < 1f &&
                Mathf.Abs(rearRightWheelCollider.rpm) < 1f
            )
                stationary = true;
            else
                stationary = false;
        }

        public bool InAir()
        {
            foreach (WheelCollider wheel in wheels)
            {
                if (wheel.GetGroundHit(out _))
                    return false;
            }
            return true;
        }
    }
}