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

    // =========================
    // GAINS (adjustable in Inspector)
    // =========================

    // Scale factors for acceleration signals
    public float lateralGain = 1f;
    public float verticalGain = 1f;
    public float accelGain = 1f;
    public float brakeGain = 1f;

    // Mixing factors for combining effects
    public float pitchFromAccel = 0f;
    public float rollFromLateral = 0f;
    public float slopePitchGain = 1f;
    public float slopeRollGain = 1f;

    // Clamp limits to restrict output ranges
    public float maxLat = 20f;
    public float maxVert = 10f;
    public float maxLong = 20f;

    void Awake()
    {
        // Keep application running in background
        Application.runInBackground = true;
    }

    void Start()
    {
        // Get Rigidbody component
        vehicleBody = GetComponent<Rigidbody>();

        // Initialize last velocity
        if (vehicleBody != null)
            lastVelocity = vehicleBody.linearVelocity;
    }

    void FixedUpdate()
    {
        // Stop if no Rigidbody found
        if (vehicleBody == null) return;

        // Get fixed timestep
        float dt = Time.fixedDeltaTime;
        if (dt <= 0f) return;

        // =========================
        // SPEED
        // =========================

        // Convert velocity from m/s to km/h
        float speed = vehicleBody.linearVelocity.magnitude * 3.6f;

        // =========================
        // ACCELERATION
        // =========================

        // Calculate acceleration from velocity change
        Vector3 accel =
            (vehicleBody.linearVelocity - lastVelocity) / dt;

        // Store current velocity for next frame
        lastVelocity = vehicleBody.linearVelocity;

        // Convert world acceleration into local vehicle space
        Vector3 localAccel =
            transform.InverseTransformDirection(accel);

        // Convert to G-forces
        float latG = -localAccel.x / 9.81f;
        float vertG = localAccel.y / 9.81f;
        float longG = localAccel.z / 9.81f;

        // =========================
        // ACCELERATION OUTPUT
        // =========================

        // Apply gain and clamp lateral acceleration
        float lateralAcceleration =
            Mathf.Clamp(latG * lateralGain, -maxLat, maxLat);

        // Apply gain and clamp vertical acceleration
        float verticalAcceleration =
            Mathf.Clamp(vertG * verticalGain, -maxVert, maxVert);

        // Apply different gains for acceleration and braking, then clamp
        float longitudinalAcceleration =
            longG < 0f
            ? Mathf.Clamp(longG * brakeGain, -maxLong, maxLong)
            : Mathf.Clamp(longG * accelGain, -maxLong, maxLong);

        // =========================
        // ORIENTATION
        // =========================

        // Project forward vector onto horizontal plane
        Vector3 forwardOnPlane =
            Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        // Calculate pitch angle based on slope
        float slopePitch =
            Vector3.SignedAngle(forwardOnPlane, transform.forward, transform.right);

        // Calculate roll angle based on tilt
        float slopeRoll =
            -Mathf.Asin(transform.right.y) * Mathf.Rad2Deg;

        // Combine slope and acceleration influence for pitch
        float pitch =
            -slopePitch * slopePitchGain
            + longitudinalAcceleration * pitchFromAccel;

        // Combine slope and lateral acceleration for roll
        float roll =
            slopeRoll * slopeRollGain
            + lateralAcceleration * rollFromLateral;

        // Clamp pitch and roll to realistic limits
        pitch = Mathf.Clamp(pitch, -45f, 45f);
        roll = Mathf.Clamp(roll, -45f, 45f);

        // Normalize yaw angle
        float yaw =
            NormalizeAngle(vehicleBody.rotation.eulerAngles.y);

        // =========================
        // RPM
        // =========================

        // Simple RPM estimation based on speed
        float rpm = 800f + speed * 30f;
        float maxRpm = 4500f;
        int gear = 1;

        // =========================
        // PLACEHOLDER VALUES
        // =========================

        // No real traction data available
        float lateralVelocity = 0f;

        // No suspension data available
        float suspensionFL = 0f;
        float suspensionFR = 0f;
        float suspensionRL = 0f;
        float suspensionRR = 0f;

        // Constant terrain type
        uint terrainFL = 2;
        uint terrainFR = 2;
        uint terrainRL = 2;
        uint terrainRR = 2;

        // =========================
        // SEND TELEMETRY
        // =========================

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
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}