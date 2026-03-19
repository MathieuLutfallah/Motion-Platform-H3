using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Ezereal
{
    // Defines available gear states
    public enum Gear
    {
        Reverse,
        Neutral,
        Drive
    }

    // Main vehicle controller handling input, physics, and UI
    public class EzerealCarController : MonoBehaviour
    {
        // =========================
        // REFERENCES
        // =========================

        // Rigidbody for physics simulation
        public Rigidbody vehicleRB;

        // Wheel colliders for physics
        public WheelCollider frontLeftWheelCollider;
        public WheelCollider frontRightWheelCollider;
        public WheelCollider rearLeftWheelCollider;
        public WheelCollider rearRightWheelCollider;

        // Visual wheel meshes
        [SerializeField] Transform frontLeftWheelMesh;
        [SerializeField] Transform frontRightWheelMesh;
        [SerializeField] Transform rearLeftWheelMesh;
        [SerializeField] Transform rearRightWheelMesh;

        // UI elements for speed display
        [SerializeField] TMP_Text currentSpeedTMP_UI;
        [SerializeField] TMP_Text currentSpeedTMP_Dashboard;

        // =========================
        // SETTINGS
        // =========================

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

        // =========================
        // RUNTIME VALUES
        // =========================

        float currentSpeed;
        float throttleInput;
        float brakeInput;
        float targetSteerAngle;
        float currentSteerAngle;

        // True if vehicle is not moving
        public bool stationary = true;

        // Current gear state
        public Gear currentGear = Gear.Drive;

        // Cached wheel array for checks
        WheelCollider[] wheels;

        // Called once at startup
        void Awake()
        {
            // Keep running in background
            Application.runInBackground = true;

            // Apply drag settings to Rigidbody
            vehicleRB.linearDamping = drag;
            vehicleRB.angularDamping = angularDrag;

            // Store all wheels in array for easy iteration
            wheels = new WheelCollider[]
            {
                frontLeftWheelCollider,
                frontRightWheelCollider,
                rearLeftWheelCollider,
                rearRightWheelCollider
            };
        }

        // =========================
        // INPUT HANDLING
        // =========================

        // Steering input
        public void OnSteer(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();

            // Ignore small input noise
            if (Mathf.Abs(value) < 0.01f)
                value = 0f;

            // Apply non-linear curve for smoother steering
            float curved = Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), 1.4f);

            // Reduce steering at higher speeds
            float speedFactor = Mathf.InverseLerp(0, 120f, Mathf.Abs(currentSpeed));
            float dynamic = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.35f, speedFactor);

            targetSteerAngle = curved * dynamic;
        }

        // Throttle input
        public void OnThrottle(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();

            float throttle = Mathf.Clamp01(value);

            // Ignore small input
            if (throttle < 0.02f)
                throttle = 0f;

            throttleInput = throttle;
        }

        // Brake input
        public void OnBrake(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();

            float brake = Mathf.Clamp01(value);

            // Ignore small input
            if (brake < 0.02f)
                brake = 0f;

            brakeInput = brake;
        }

        // =========================
        // GEAR SHIFT
        // =========================

        // Shift up (Reverse -> Neutral -> Drive)
        public void OnUpShift(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            if (currentGear == Gear.Reverse)
                currentGear = Gear.Neutral;
            else if (currentGear == Gear.Neutral)
                currentGear = Gear.Drive;
        }

        // Shift down (Drive -> Neutral -> Reverse)
        public void OnDownShift(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            if (currentGear == Gear.Drive)
                currentGear = Gear.Neutral;
            else if (currentGear == Gear.Neutral)
                currentGear = Gear.Reverse;
        }

        // =========================
        // RESTART
        // =========================

        // Reload current scene
        public void OnRestart(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // =========================
        // PHYSICS UPDATE
        // =========================

        // Called at fixed intervals for physics
        void FixedUpdate()
        {
            UpdateSpeed();
            ApplyMotor();
            ApplyBrakes();
            ApplySteering();
            UpdateStationary();
        }

        // Apply engine torque to wheels
        void ApplyMotor()
        {
            // Reduce torque at higher speeds
            float speedFactor = Mathf.InverseLerp(0, maxForwardSpeed, Mathf.Abs(currentSpeed));
            float torque = Mathf.Lerp(horsePower, horsePower * 0.25f, speedFactor);

            float finalTorque = 0f;

            // Apply torque based on gear
            if (currentGear == Gear.Drive)
            {
                finalTorque = torque * throttleInput;
            }
            else if (currentGear == Gear.Reverse)
            {
                finalTorque = -torque * throttleInput;
            }
            else if (currentGear == Gear.Neutral)
            {
                finalTorque = 0f;
            }

            // Disable engine when braking
            if (brakeInput > 0.1f)
                finalTorque = 0f;

            // Apply torque to rear wheels
            rearLeftWheelCollider.motorTorque = finalTorque;
            rearRightWheelCollider.motorTorque = finalTorque;
        }

        // Apply braking force
        void ApplyBrakes()
        {
            float brakeTorque = brakeInput * brakePower;

            // Stronger braking on front wheels
            frontLeftWheelCollider.brakeTorque = brakeTorque;
            frontRightWheelCollider.brakeTorque = brakeTorque;

            // Reduced braking on rear wheels
            rearLeftWheelCollider.brakeTorque = brakeTorque * 0.5f;
            rearRightWheelCollider.brakeTorque = brakeTorque * 0.5f;
        }

        // Apply steering and update wheel visuals
        void ApplySteering()
        {
            // Smooth steering transition
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * steeringSpeed);

            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle;

            // Update visual wheel positions
            UpdateWheel(frontLeftWheelCollider, frontLeftWheelMesh);
            UpdateWheel(frontRightWheelCollider, frontRightWheelMesh);
            UpdateWheel(rearLeftWheelCollider, rearLeftWheelMesh);
            UpdateWheel(rearRightWheelCollider, rearRightWheelMesh);
        }

        // Sync wheel mesh with collider
        void UpdateWheel(WheelCollider col, Transform mesh)
        {
            col.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.SetPositionAndRotation(pos, rot);
        }

        // Calculate and display speed
        void UpdateSpeed()
        {
#if UNITY_6000_0_OR_NEWER
            currentSpeed = Vector3.Dot(transform.forward, vehicleRB.linearVelocity) * 3.6f;
#else
            currentSpeed = Vector3.Dot(transform.forward, vehicleRB.velocity) * 3.6f;
#endif

            float displaySpeed = Mathf.Abs(currentSpeed);

            // Update UI text
            currentSpeedTMP_UI.text = displaySpeed.ToString("F0");
            currentSpeedTMP_Dashboard.text = displaySpeed.ToString("F0");
        }

        // Check if vehicle is stationary
        void UpdateStationary()
        {
            if (
                Mathf.Abs(frontLeftWheelCollider.rpm) < 1f &&
                Mathf.Abs(frontRightWheelCollider.rpm) < 1f &&
                Mathf.Abs(rearLeftWheelCollider.rpm) < 1f &&
                Mathf.Abs(rearRightWheelCollider.rpm) < 1f
            )
            {
                stationary = true;
            }
            else
            {
                stationary = false;
            }
        }

        // Returns true if all wheels are off the ground
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