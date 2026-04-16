using UnityEngine;

/// <summary>
/// GameManager
/// Purpose: Central game-state coordinator for play, crash, and restart flows.
/// Responsibilities:
/// - Own high-level run state.
/// - Coordinate startup and restart sequencing.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    private enum GameState
    {
        Playing,
        Crashed
    }

    [Header("System References")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private DistanceTracker distanceTracker;
    [SerializeField] private HUDController hudController;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float restartDelaySeconds = 1f;

    private GameState currentState = GameState.Playing;
    private float restartAtTime;

    public bool IsPlaying => currentState == GameState.Playing;

    private void Start()
    {
        StartRun();
    }

    private void Update()
    {
        if (currentState == GameState.Crashed && Time.time >= restartAtTime)
        {
            StartRun();
        }
    }

    public void CrashGame()
    {
        if (currentState == GameState.Crashed)
        {
            return;
        }

        currentState = GameState.Crashed;
        restartAtTime = Time.time + restartDelaySeconds;

        if (droneController != null)
        {
            droneController.SetMovementEnabled(false);
        }

        if (hudController != null)
        {
            hudController.ShowDeathOverlay(distanceTracker != null ? distanceTracker.CurrentDistanceMeters : 0f);
        }
    }

    private void StartRun()
    {
        currentState = GameState.Playing;

        if (distanceTracker != null)
        {
            distanceTracker.ResetDistance();
        }

        if (droneController != null)
        {
            droneController.ResetTransform();
            droneController.SetMovementEnabled(true);
        }

        if (hudController != null)
        {
            hudController.HideDeathOverlay();
        }
    }
}
