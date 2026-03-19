using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct telemetryPacket
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public char[] apiMode;
    public uint version;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
    public char[] game;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
    public char[] vehicleName;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
    public char[] location;
    public float speed;
    public float rpm;
    public float maxRpm;
    public int gear;
    public float pitch;
    public float roll;
    public float yaw;
    public float lateralVelocity;
    public float lateralAcceleration;
    public float verticalAcceleration;
    public float longitudinalAcceleration;
    public float suspensionTravelFrontLeft;
    public float suspensionTravelFrontRight;
    public float suspensionTravelRearLeft;
    public float suspensionTravelRearRight;
    public uint wheelTerrainFrontLeft;
    public uint wheelTerrainFrontRight;
    public uint wheelTerrainRearLeft;
    public uint wheelTerrainRearRight;

    public telemetryPacket(char[] pmode, uint pversion, char[] pgame, char[] pvehicleName, char[] plocation, float pspeed, float prpm, float pmaxRpm, int pgear, float ppitch, float proll, float pyaw, float plateralVelocity, float plateralAcceleration, float pverticalAcceleration, float plongitudinalAcceleration, float psuspensionTravelFrontLeft, float psuspensionTravelFrontRight, float psuspensionTravelRearLeft, float psuspensionTravelRearRight, uint pwheelTerrainFrontLeft, uint pwheelTerrainFrontRight, uint pwheelTerrainRearLeft, uint pwheelTerrainRearRight)
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

public class SimRacingStudio : MonoBehaviour
{
    public string srsHostIP = "127.0.0.1";
    public int srsHostPort = 33001;

    IPEndPoint remoteEndPoint;
    static UdpClient udpClient;
    static telemetryPacket tp;

    void Awake()
    {
        // Wichtig für Hintergrundbetrieb
        Application.runInBackground = true;
    }

    void Start()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(srsHostIP), srsHostPort);

        udpClient = new UdpClient();
        udpClient.Client.Blocking = false;

        tp = new telemetryPacket();

        StartCoroutine(SendLoop());
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Application.runInBackground = true;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Application.runInBackground = true;
    }

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

    IEnumerator SendLoop()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (true)
        {
            int size = Marshal.SizeOf(tp);
            byte[] packet = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(tp, ptr, true);
            Marshal.Copy(ptr, packet, 0, size);
            Marshal.FreeHGlobal(ptr);

            if (udpClient != null)
            {
                udpClient.Send(packet, packet.Length, remoteEndPoint);
            }

            yield return wait;
        }
    }
}