using UnityEngine;

/// <summary>
/// ObstacleSystem
/// Purpose: Own obstacle spawn pacing and density scaling controls.
/// Responsibilities:
/// - Track spawn timing parameters.
/// - Provide difficulty-scaled spawn interval.
/// </summary>
public sealed class ObstacleSystem : MonoBehaviour
{
    [SerializeField, Min(0.5f)] private float minSpawnInterval = 0.5f;
    [SerializeField, Min(0.5f)] private float maxSpawnInterval = 1.5f;
    [SerializeField, Min(100f)] private float maxDifficultyDistance = 3000f;

    public float GetSpawnInterval(float distanceMeters)
    {
        float t = Mathf.Clamp01(distanceMeters / maxDifficultyDistance);
        return Mathf.Lerp(maxSpawnInterval, minSpawnInterval, t);
    }
}
