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
        // Wichtig: läuft auch ohne Fokus weiter
        Application.runInBackground = true;
    }

    void Start()
    {
        // Cache the vehicle rigidbody reference
        vehicleBody = GetComponent<Rigidbody>();

        // Store initial velocity for acceleration calculation
        if (vehicleBody != null)
            lastVelocity = vehicleBody.linearVelocity;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Keine Pause bei Fokusverlust
        Application.runInBackground = true;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Sicherstellen dass nichts stoppt
        Application.runInBackground = true;
    }

    void FixedUpdate()
    {
        // Exit if no rigidbody exists
        if (vehicleBody == null) return;

        float deltaTime = Time.fixedDeltaTime;
        if (deltaTime <= 0f) return;


        // -------------------------------------------------------
        // VEHICLE SPEED
        // -------------------------------------------------------

        float speed = vehicleBody.linearVelocity.magnitude * 3.6f;


        // -------------------------------------------------------
        // ACCELERATION CALCULATION
        // -------------------------------------------------------

        Vector3 acceleration =
            (vehicleBody.linearVelocity - lastVelocity) / deltaTime;

        lastVelocity = vehicleBody.linearVelocity;

        Vector3 localAccel = transform.InverseTransformDirection(acceleration);

        float latG = -localAccel.x / 9.81f;
        float vertG = localAccel.y / 9.81f;
        float longG = localAccel.z / 9.81f;


        // -------------------------------------------------------
        // MOTION GAINS
        // -------------------------------------------------------

        float lateralGain = 8f;
        float verticalGain = 5f;
        float accelGain = 10f;
        float brakeGain = 12f;


        // -------------------------------------------------------
        // LATERAL ACCELERATION
        // -------------------------------------------------------

        float lateralAcceleration =
            Mathf.Clamp(latG * lateralGain, -20f, 20f);


        // -------------------------------------------------------
        // VERTICAL ACCELERATION
        // -------------------------------------------------------

        float verticalAcceleration =
            Mathf.Clamp(vertG * verticalGain, -10f, 10f);


        // -------------------------------------------------------
        // LONGITUDINAL ACCELERATION
        // -------------------------------------------------------

        float longitudinalAcceleration;

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

        Vector3 forwardOnPlane =
            Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        float slopePitch =
            Vector3.SignedAngle(forwardOnPlane, transform.forward, transform.right);

        float slopeRoll =
            -Mathf.Asin(transform.right.y) * Mathf.Rad2Deg * 0.4f;

        float swayRoll =
            lateralAcceleration * 2.5f;

        float accelPitch =
            longitudinalAcceleration * 1.2f;

        float slopeGain = 1.5f;

        float pitch =
            -slopePitch * slopeGain + accelPitch;

        float roll =
            slopeRoll + swayRoll;

        pitch = Mathf.Clamp(pitch, -45f, 45f);
        roll = Mathf.Clamp(roll, -45f, 45f);

        float yaw =
            NormalizeAngle(vehicleBody.rotation.eulerAngles.y);


        // -------------------------------------------------------
        // RPM ESTIMATION
        // -------------------------------------------------------

        float rpm = 800f + speed * 30f;
        float maxRpm = 4500f;
        int gear = 1;


        // -------------------------------------------------------
        // TRACTION LOSS
        // -------------------------------------------------------

        float lateralVelocity = 0f;


        // -------------------------------------------------------
        // SUSPENSION (DISABLED)
        // -------------------------------------------------------

        float suspensionFL = 0f;
        float suspensionFR = 0f;
        float suspensionRL = 0f;
        float suspensionRR = 0f;


        // -------------------------------------------------------
        // TERRAIN TYPE
        // -------------------------------------------------------

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

    float NormalizeAngle(float angle)
    {
        angle = angle % 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}