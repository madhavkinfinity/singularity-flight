using UnityEngine;

/// <summary>
/// DistanceTracker
/// Purpose: Track run distance based on drone forward speed while game is active.
/// Responsibilities:
/// - Increment distance during gameplay.
/// - Provide resettable score value.
/// </summary>
public sealed class DistanceTracker : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField, Min(1f)] private float forwardSpeedMetersPerSecond = 30f;

    public float CurrentDistanceMeters { get; private set; }

    private void Update()
    {
        if (gameManager == null || !gameManager.IsPlaying)
        {
            return;
        }

        CurrentDistanceMeters += forwardSpeedMetersPerSecond * Time.deltaTime;
    }

    public void ResetDistance()
    {
        CurrentDistanceMeters = 0f;
    }
}
