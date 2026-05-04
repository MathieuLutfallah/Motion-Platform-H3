using UnityEngine;
using System;

public class Telemetry : MonoBehaviour
{
    string apiMode = "api";

    public string game = "Unity Simulation";
    public string vehicle = "Toyota Hilux";
    public string location = "Offroad Test Track";

    uint apiVersion = 102;

    Rigidbody vehicleBody;
    Vector3 lastVelocity;

    void Awake()
    {
        Application.runInBackground = true;
    }

    void Start()
    {
        vehicleBody = GetComponent<Rigidbody>();

        if (vehicleBody != null)
            lastVelocity = vehicleBody.linearVelocity;
    }

    void FixedUpdate()
    {
        if (vehicleBody == null) return;

        float dt = Time.fixedDeltaTime;
        if (dt <= 0f) return;

        // SPEED
        // Convert velocity from m/s to km/h
        float speed = vehicleBody.linearVelocity.magnitude * 3.6f;

        // ACCELERATION
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

        float lateralAcceleration = latG;
        float verticalAcceleration = vertG;
        float longitudinalAcceleration = longG;

        // ORIENTATION
        Vector3 forwardOnPlane =
            Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        // Calculate pitch angle based on slope
        float slopePitch =
            Vector3.SignedAngle(forwardOnPlane, transform.forward, transform.right);

        // Calculate roll angle based on tilt
        float slopeRoll =
            -Mathf.Asin(transform.right.y) * Mathf.Rad2Deg;

        // Use only vehicle orientation, no mixing with acceleration
        float pitch = -slopePitch;
        float roll = slopeRoll;

        // Normalize yaw angle
        float yaw =
            NormalizeAngle(vehicleBody.rotation.eulerAngles.y);

        // RPM
        // Simple RPM estimation based on speed
        float rpm = 800f + speed * 30f;
        float maxRpm = 4500f;
        int gear = 1;

        // PLACEHOLDER
        float lateralVelocity = 0f;

        // No suspension data
        float suspensionFL = 0f;
        float suspensionFR = 0f;
        float suspensionRL = 0f;
        float suspensionRR = 0f;

        // Constant terrain type
        uint terrainFL = 2;
        uint terrainFR = 2;
        uint terrainRL = 2;
        uint terrainRR = 2;

        // SEND TELEMETRY
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