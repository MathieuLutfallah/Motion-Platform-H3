using UnityEngine;

public class DistanceSession : MonoBehaviour
{
    private static float savedDistance = 0f;
    private static float savedTime = 0f;

    private Vector3 lastPosition;
    private float totalDistance = 0f;
    private float sessionTime = 0f;
    private float logTimer = 0f;

    void Start()
    {
        totalDistance = savedDistance;
        sessionTime = savedTime;

        lastPosition = transform.position;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);

        totalDistance += distance;
        lastPosition = transform.position;

        sessionTime += Time.deltaTime;
        logTimer += Time.deltaTime;

        if (logTimer >= 1f)
        {
            Debug.Log("Distanz: " + totalDistance.ToString("F1") + " m | Zeit: " + sessionTime.ToString("F1") + " s");
            logTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetSession();
        }
    }

    void OnDisable()
    {
        savedDistance = totalDistance;
        savedTime = sessionTime;
    }

    void ResetSession()
    {
        totalDistance = 0f;
        sessionTime = 0f;

        savedDistance = 0f;
        savedTime = 0f;

        lastPosition = transform.position;

        Debug.Log("Session reset");
    }
}