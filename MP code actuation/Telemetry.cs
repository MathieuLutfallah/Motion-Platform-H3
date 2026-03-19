using UnityEngine;
using System;

// Unity component that calculates vehicle telemetry and sends it to SimRacingStudio
public class Telemetry : MonoBehaviour
{
    // API identifier string, must be exactly 3 characters
    string apiMode = "api";

    // Metadata about the simulation
    public string game = "Unity Simulation";
    public string vehicle = "Toyota Hilux";
    public string location = "Offroad Test Track";

    // Version of the telemetry format
    uint apiVersion = 102;

    // Reference to the vehicle physics body
    Rigidbody vehicleBody;

    // Stores velocity from previous frame to calculate acceleration
    Vector3 lastVelocity;

    void Awake()
    {
        // Keep application running even when not focused
        Application.runInBackground = true;
    }

    void Start()
    {
        // Get Rigidbody component attached to this object
        vehicleBody = GetComponent<Rigidbody>();

        // Store initial velocity for later acceleration calculation
        if (vehicleBody != null)
            lastVelocity = vehicleBody.linearVelocity;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Ensure simulation continues when focus changes
        Application.runInBackground = true;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Ensure simulation continues when paused
        Application.runInBackground = true;
    }

    void FixedUpdate()
    {
        // Stop execution if no Rigidbody exists
        if (vehicleBody == null) return;

        // Get fixed timestep
        float deltaTime = Time.fixedDeltaTime;

        // Avoid division by zero
        if (deltaTime <= 0f) return;


        // -------------------------------------------------------
        // VEHICLE SPEED
        // -------------------------------------------------------

        // Convert velocity magnitude from m/s to km/h
        float speed = vehicleBody.linearVelocity.magnitude * 3.6f;


        // -------------------------------------------------------
        // ACCELERATION CALCULATION
        // -------------------------------------------------------

        // Calculate acceleration based on velocity change over time
        Vector3 acceleration =
            (vehicleBody.linearVelocity - lastVelocity) / deltaTime;

        // Store current velocity for next frame
        lastVelocity = vehicleBody.linearVelocity;

        // Convert world acceleration into local vehicle space
        Vector3 localAccel = transform.InverseTransformDirection(acceleration);

        // Convert acceleration into G-forces
        float latG = -localAccel.x / 9.81f;
        float vertG = localAccel.y / 9.81f;
        float longG = localAccel.z / 9.81f;


        // -------------------------------------------------------
        // MOTION GAINS
        // -------------------------------------------------------

        // Scale factors to tune motion intensity
        float lateralGain = 8f;
        float verticalGain = 5f;
        float accelGain = 10f;
        float brakeGain = 12f;


        // -------------------------------------------------------
        // LATERAL ACCELERATION
        // -------------------------------------------------------

        // Clamp lateral G-force to safe range
        float lateralAcceleration =
            Mathf.Clamp(latG * lateralGain, -20f, 20f);


        // -------------------------------------------------------
        // VERTICAL ACCELERATION
        // -------------------------------------------------------

        // Clamp vertical G-force
        float verticalAcceleration =
            Mathf.Clamp(vertG * verticalGain, -10f, 10f);


        // -------------------------------------------------------
        // LONGITUDINAL ACCELERATION
        // -------------------------------------------------------

        float longitudinalAcceleration;

        // Use stronger gain for braking than acceleration
        if (longG < 0f)
        {
            longitudinalAcceleration =
                Mathf.Clamp(longG * brakeGain, -10f, 10f);
        }
        else
        {
            longitudinalAcceleration =
                Mathf.Clamp(longG * accelGain, -10f, 10f);
        }


        // -------------------------------------------------------
        // VEHICLE ORIENTATION
        // -------------------------------------------------------

        // Project forward vector onto horizontal plane
        Vector3 forwardOnPlane =
            Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        // Calculate pitch angle from slope
        float slopePitch =
            Vector3.SignedAngle(forwardOnPlane, transform.forward, transform.right);

        // Calculate roll angle from slope
        float slopeRoll =
            -Mathf.Asin(transform.right.y) * Mathf.Rad2Deg * 0.4f;

        // Add roll effect from lateral acceleration
        float swayRoll =
            lateralAcceleration * 2.5f;

        // Add pitch effect from acceleration and braking
        float accelPitch =
            longitudinalAcceleration * 1.2f;

        // Scale slope contribution
        float slopeGain = 1.5f;

        // Combine pitch components
        float pitch =
            -slopePitch * slopeGain + accelPitch;

        // Combine roll components
        float roll =
            slopeRoll + swayRoll;

        // Limit pitch and roll to realistic angles
        pitch = Mathf.Clamp(pitch, -45f, 45f);
        roll = Mathf.Clamp(roll, -45f, 45f);

        // Normalize yaw angle from rotation
        float yaw =
            NormalizeAngle(vehicleBody.rotation.eulerAngles.y);


        // -------------------------------------------------------
        // RPM ESTIMATION
        // -------------------------------------------------------

        // Simple RPM estimation based on speed
        float rpm = 800f + speed * 30f;

        // Define maximum RPM
        float maxRpm = 4500f;

        // Fixed gear value
        int gear = 1;


        // -------------------------------------------------------
        // TRACTION LOSS
        // -------------------------------------------------------

        // Placeholder value for lateral slip
        float lateralVelocity = 0f;


        // -------------------------------------------------------
        // SUSPENSION (DISABLED)
        // -------------------------------------------------------

        // Suspension values are not calculated, set to zero
        float suspensionFL = 0f;
        float suspensionFR = 0f;
        float suspensionRL = 0f;
        float suspensionRR = 0f;


        // -------------------------------------------------------
        // TERRAIN TYPE
        // -------------------------------------------------------

        // Constant terrain type for all wheels
        uint terrainFL = 2;
        uint terrainFR = 2;
        uint terrainRL = 2;
        uint terrainRR = 2;


        // -------------------------------------------------------
        // SEND TELEMETRY DATA
        // -------------------------------------------------------

        // Send all calculated values to SimRacingStudio
        SimRacingStudio.SimRacingStudio_SendTelemetry(
            apiMode.PadRight(3).ToCharArray(),
            apiVersion,
            game.PadRight(50).ToCharArray(),
            vehicle.PadRight(50).ToCharArray(),
            location.PadRight(50).ToCharArray(),
            speed,
            rpm,
            maxRpm,
            gear,
            pitch,
            roll,
            yaw,
            lateralVelocity,
            lateralAcceleration,
            verticalAcceleration,
            longitudinalAcceleration,
            suspensionFL,
            suspensionFR,
            suspensionRL,
            suspensionRR,
            terrainFL,
            terrainFR,
            terrainRL,
            terrainRR
        );
    }

    // Normalize angle to range -180 to 180 degrees
    float NormalizeAngle(float angle)
    {
        angle = angle % 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}