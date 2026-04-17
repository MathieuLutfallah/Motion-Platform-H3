using UnityEngine;

namespace Ezereal
{
    public class EngineSoundController : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource idle;

        public AudioSource low_on;
        public AudioSource medium_on;
        public AudioSource high_on;

        public AudioSource low_off;
        public AudioSource medium_off;
        public AudioSource high_off;

        [Header("References")]
        public EzerealCarController car;

        [Header("Settings")]
        public float maxRPM = 2000f;
        public float smoothSpeed = 5f;

        float throttleSmooth;

        void Update()
        {
            float rpm = GetAverageRPM();
            float normalized = Mathf.Clamp01(rpm / maxRPM);

            // throttle weich glätten
            throttleSmooth = Mathf.Lerp(throttleSmooth, car.ThrottleInput, Time.deltaTime * smoothSpeed);

            float onFactor = throttleSmooth;
            float offFactor = 1f - throttleSmooth;

            // Idle
            float idleTarget = Mathf.Clamp01(1f - normalized * 3f);
            idle.volume = Mathf.Lerp(idle.volume, idleTarget * 0.25f, Time.deltaTime * smoothSpeed);

            // LOW
            float lowBase = Mathf.Clamp01(normalized * 2f);
            low_on.volume = Mathf.Lerp(low_on.volume, lowBase * onFactor, Time.deltaTime * smoothSpeed);
            low_off.volume = Mathf.Lerp(low_off.volume, lowBase * offFactor, Time.deltaTime * smoothSpeed);

            // MEDIUM
            float medBase = Mathf.Clamp01((normalized - 0.25f) * 2f);
            medium_on.volume = Mathf.Lerp(medium_on.volume, medBase * onFactor, Time.deltaTime * smoothSpeed);
            medium_off.volume = Mathf.Lerp(medium_off.volume, medBase * offFactor, Time.deltaTime * smoothSpeed);

            // HIGH
            float highBase = Mathf.Clamp01((normalized - 0.55f) * 2f);
            high_on.volume = Mathf.Lerp(high_on.volume, highBase * onFactor, Time.deltaTime * smoothSpeed);
            high_off.volume = Mathf.Lerp(high_off.volume, highBase * offFactor, Time.deltaTime * smoothSpeed);

            // Pitch
            float pitch = 0.8f + normalized * 1.5f;

            SetPitch(low_on, medium_on, high_on, pitch);
            SetPitch(low_off, medium_off, high_off, pitch);
        }

        void SetPitch(AudioSource a, AudioSource b, AudioSource c, float pitch)
        {
            a.pitch = pitch;
            b.pitch = pitch;
            c.pitch = pitch;
        }

        float GetAverageRPM()
        {
            float rpmL = Mathf.Abs(car.rearLeftWheelCollider.rpm);
            float rpmR = Mathf.Abs(car.rearRightWheelCollider.rpm);

            return (rpmL + rpmR) * 0.5f;
        }
    }
}