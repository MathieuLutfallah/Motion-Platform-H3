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

    void Start()
    {
        // Cache the vehicle rigidbody reference
        vehicleBody = GetComponent<Rigidbody>();

        // Store initial velocity for acceleration calculation
        lastVelocity = vehicleBody.linearVelocity;
    }

    void FixedUpdate()
    {
        // Exit if no rigidbody exists
        if (vehicleBody == null) return;


        // -------------------------------------------------------
        // VEHICLE SPEED
        // -------------------------------------------------------

        // Convert velocity magnitude from m/s to km/h
        float speed = vehicleBody.linearVelocity.magnitude * 3.6f;


        // -------------------------------------------------------
        // ACCELERATION CALCULATION
        // -------------------------------------------------------

        // Calculate world acceleration based on velocity change
        Vector3 acceleration =
            (vehicleBody.linearVelocity - lastVelocity) / Time.fixedDeltaTime;

        // Store velocity for next frame
        lastVelocity = vehicleBody.linearVelocity;

        // Convert acceleration to local vehicle space
        Vector3 localAccel = transform.InverseTransformDirection(acceleration);

        // Convert acceleration to G forces
        // Lateral axis is inverted so the seat moves opposite to acceleration
        float latG = -localAccel.x / 9.81f;
        float vertG = localAccel.y / 9.81f;
        float longG = localAccel.z / 9.81f;


        // -------------------------------------------------------
        // MOTION GAINS
        // -------------------------------------------------------

        // Lateral cornering force multiplier
        float lateralGain = 8f;

        // Vertical bumps and jumps multiplier
        float verticalGain = 5f;

        // Acceleration force multiplier
        float accelGain = 10f;

        // Braking force multiplier
        float brakeGain = 12f;


        // -------------------------------------------------------
        // LATERAL ACCELERATION
        // -------------------------------------------------------

        // Calculate lateral motion value
        float lateralAcceleration =
            Mathf.Clamp(latG * lateralGain, -20f, 20f);


        // -------------------------------------------------------
        // VERTICAL ACCELERATION
        // -------------------------------------------------------

        // Calculate vertical motion value
        float verticalAcceleration =
            Mathf.Clamp(vertG * verticalGain, -10f, 10f);


        // -------------------------------------------------------
        // LONGITUDINAL ACCELERATION
        // -------------------------------------------------------

        float longitudinalAcceleration;

        // Use different gain for braking and acceleration
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

        // ---- STABLE SLOPE CALCULATION ----

        // Project forward vector onto ground plane
        Vector3 forwardOnPlane =
            Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        // Calculate actual vehicle pitch relative to ground slope
        float slopePitch =
            Vector3.SignedAngle(forwardOnPlane, transform.forward, transform.right);


        // Calculate roll caused by lateral slope
        float slopeRoll =
            -Mathf.Asin(transform.right.y) * Mathf.Rad2Deg * 0.4f;


        // Amplify roll during cornering
        float swayRoll =
            lateralAcceleration * 2.5f;


        // Pitch contribution from acceleration
        float accelPitch =
            longitudinalAcceleration * 1.2f;

        // Increase slope effect for stronger motion feedback
        float slopeGain = 1.5f;

        float pitch =
            -slopePitch * slopeGain + accelPitch;

        float roll =
            slopeRoll + swayRoll;


        // Clamp orientation values to safe limits
        pitch = Mathf.Clamp(pitch, -45f, 45f);
        roll = Mathf.Clamp(roll, -45f, 45f);

        // Normalize yaw angle
        float yaw =
            NormalizeAngle(vehicleBody.rotation.eulerAngles.y);


        // -------------------------------------------------------
        // RPM ESTIMATION
        // -------------------------------------------------------

        // Estimate engine RPM based on speed
        float rpm = 800f + speed * 30f;

        float maxRpm = 4500f;

        int gear = 1;


        // -------------------------------------------------------
        // TRACTION LOSS
        // -------------------------------------------------------

        // Lateral slip velocity placeholder
        float lateralVelocity = 0f;


        // -------------------------------------------------------
        // SUSPENSION (DISABLED)
        // -------------------------------------------------------

        // Suspension travel values for each wheel
        float suspensionFL = 0f;
        float suspensionFR = 0f;
        float suspensionRL = 0f;
        float suspensionRR = 0f;


        // -------------------------------------------------------
        // TERRAIN TYPE
        // -------------------------------------------------------

        // Terrain identifiers for each wheel
        uint terrainFL = 2;
        uint terrainFR = 2;
        uint terrainRL = 2;
        uint terrainRR = 2;


        // -------------------------------------------------------
        // SEND TELEMETRY DATA
        // -------------------------------------------------------

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


    // -------------------------------------------------------
    // ANGLE NORMALIZATION
    // -------------------------------------------------------

    float NormalizeAngle(float angle)
    {
        // Normalize angle into range -180 to 180 degrees
        angle = angle % 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

}