using UnityEngine;

/// <summary>
/// RuntimeEmergencyStrobe
/// Purpose: Pulse point lights to create red emergency strobe beats.
/// Responsibilities:
/// - Modulate a Light intensity between baseline and peak values.
/// - Keep pulse deterministic and framerate independent.
/// </summary>
[RequireComponent(typeof(Light))]
public sealed class RuntimeEmergencyStrobe : MonoBehaviour
{
    [SerializeField, Min(0f)] private float minIntensity = 0.05f;
    [SerializeField, Min(0f)] private float maxIntensity = 0.6f;
    [SerializeField, Min(0.01f)] private float pulseFrequency = 8f;

    private Light cachedLight;

    public void Configure(float minimum, float maximum, float frequency)
    {
        minIntensity = Mathf.Max(0f, minimum);
        maxIntensity = Mathf.Max(minIntensity, maximum);
        pulseFrequency = Mathf.Max(0.01f, frequency);

        if (cachedLight != null)
        {
            cachedLight.intensity = minIntensity;
        }
    }

    private void Awake()
    {
        cachedLight = GetComponent<Light>();
        cachedLight.intensity = minIntensity;
    }

    private void Update()
    {
        float pulse = Mathf.Abs(Mathf.Sin(Time.time * pulseFrequency));
        cachedLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
    }
}
