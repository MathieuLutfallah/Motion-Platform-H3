using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

// Define a struct that represents the telemetry data packet
[StructLayout(LayoutKind.Sequential)]
public struct telemetryPacket
{
    // Fixed-size char array (3 chars) that stores API mode identifier
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public char[] apiMode;

    // Version number of the telemetry format
    public uint version;

    // Fixed-size char array (50 chars) for game name
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
    public char[] game;

    // Fixed-size char array (50 chars) for vehicle name
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
    public char[] vehicleName;

    // Fixed-size char array (50 chars) for track or location name
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
    public char[] location;

    // Current vehicle speed
    public float speed;

    // Current engine RPM
    public float rpm;

    // Maximum engine RPM
    public float maxRpm;

    // Current gear
    public int gear;

    // Vehicle orientation angles
    public float pitch;
    public float roll;
    public float yaw;

    // Vehicle movement and forces
    public float lateralVelocity;
    public float lateralAcceleration;
    public float verticalAcceleration;
    public float longitudinalAcceleration;

    // Suspension travel values for each wheel
    public float suspensionTravelFrontLeft;
    public float suspensionTravelFrontRight;
    public float suspensionTravelRearLeft;
    public float suspensionTravelRearRight;

    // Terrain type under each wheel
    public uint wheelTerrainFrontLeft;
    public uint wheelTerrainFrontRight;
    public uint wheelTerrainRearLeft;
    public uint wheelTerrainRearRight;

    // Constructor to initialize all telemetry fields
    public telemetryPacket(
        char[] pmode, uint pversion, char[] pgame, char[] pvehicleName, char[] plocation,
        float pspeed, float prpm, float pmaxRpm, int pgear,
        float ppitch, float proll, float pyaw,
        float plateralVelocity, float plateralAcceleration,
        float pverticalAcceleration, float plongitudinalAcceleration,
        float psuspensionTravelFrontLeft, float psuspensionTravelFrontRight,
        float psuspensionTravelRearLeft, float psuspensionTravelRearRight,
        uint pwheelTerrainFrontLeft, uint pwheelTerrainFrontRight,
        uint pwheelTerrainRearLeft, uint pwheelTerrainRearRight)
    {
        apiMode = pmode;
        version = pversion;
        game = pgame;
        vehicleName = pvehicleName;
        location = plocation;
        speed = pspeed;
        rpm = prpm;
        maxRpm = pmaxRpm;
        gear = pgear;
        pitch = ppitch;
        roll = proll;
        yaw = pyaw;
        lateralVelocity = plateralVelocity;
        lateralAcceleration = plateralAcceleration;
        verticalAcceleration = pverticalAcceleration;
        longitudinalAcceleration = plongitudinalAcceleration;
        suspensionTravelFrontLeft = psuspensionTravelFrontLeft;
        suspensionTravelFrontRight = psuspensionTravelFrontRight;
        suspensionTravelRearLeft = psuspensionTravelRearLeft;
        suspensionTravelRearRight = psuspensionTravelRearRight;
        wheelTerrainFrontLeft = pwheelTerrainFrontLeft;
        wheelTerrainFrontRight = pwheelTerrainFrontRight;
        wheelTerrainRearLeft = pwheelTerrainRearLeft;
        wheelTerrainRearRight = pwheelTerrainRearRight;
    }
}

// Unity MonoBehaviour that sends telemetry data via UDP
public class SimRacingStudio : MonoBehaviour
{
    // Target IP address of the telemetry receiver
    public string srsHostIP = "127.0.0.1";

    // Target UDP port
    public int srsHostPort = 33001;

    // Remote endpoint for UDP communication
    IPEndPoint remoteEndPoint;

    // UDP client used to send data
    static UdpClient udpClient;

    // Current telemetry packet instance
    static telemetryPacket tp;

    void Awake()
    {
        // Ensure the application keeps running when not focused
        Application.runInBackground = true;
    }

    void Start()
    {
        // Create endpoint from IP and port
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(srsHostIP), srsHostPort);

        // Initialize UDP client
        udpClient = new UdpClient();

        // Set socket to non-blocking mode
        udpClient.Client.Blocking = false;

        // Initialize empty telemetry packet
        tp = new telemetryPacket();

        // Start coroutine that sends data continuously
        StartCoroutine(SendLoop());
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Keep running even if application loses focus
        Application.runInBackground = true;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Keep running even if application is paused
        Application.runInBackground = true;
    }

    // Static method to update telemetry data from other scripts
    public static void SimRacingStudio_SendTelemetry(
        char[] pMode, uint pversion, char[] pgame, char[] pvehicleName,
        char[] plocation, float pspeed, float prpm, float pmaxRpm,
        int pgear, float ppitch, float proll, float pyaw,
        float plateralVelocity, float plateralAcceleration,
        float pverticalAcceleration, float plongitudinalAcceleration,
        float psuspensionTravelFrontLeft, float psuspensionTravelFrontRight,
        float psuspensionTravelRearLeft, float psuspensionTravelRearRight,
        uint pwheelTerrainFrontLeft, uint pwheelTerrainFrontRight,
        uint pwheelTerrainRearLeft, uint pwheelTerrainRearRight)
    {
        // Create a new telemetry packet with the provided data
        tp = new telemetryPacket(
            pMode, pversion, pgame, pvehicleName, plocation,
            pspeed, prpm, pmaxRpm, pgear,
            ppitch, proll, pyaw,
            plateralVelocity, plateralAcceleration,
            pverticalAcceleration, plongitudinalAcceleration,
            psuspensionTravelFrontLeft, psuspensionTravelFrontRight,
            psuspensionTravelRearLeft, psuspensionTravelRearRight,
            pwheelTerrainFrontLeft, pwheelTerrainFrontRight,
            pwheelTerrainRearLeft, pwheelTerrainRearRight
        );
    }

    // Coroutine that sends telemetry data every frame
    IEnumerator SendLoop()
    {
        // Wait object that pauses execution until end of frame
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (true)
        {
            // Get size of the struct in bytes
            int size = Marshal.SizeOf(tp);

            // Create byte array to hold serialized data
            byte[] packet = new byte[size];

            // Allocate unmanaged memory for struct conversion
            IntPtr ptr = Marshal.AllocHGlobal(size);

            // Copy struct data into unmanaged memory
            Marshal.StructureToPtr(tp, ptr, true);

            // Copy unmanaged memory into managed byte array
            Marshal.Copy(ptr, packet, 0, size);

            // Free unmanaged memory
            Marshal.FreeHGlobal(ptr);

            // Send packet via UDP if client exists
            if (udpClient != null)
            {
                udpClient.Send(packet, packet.Length, remoteEndPoint);
            }

            // Wait until end of frame before sending next packet
            yield return wait;
        }
    }
}