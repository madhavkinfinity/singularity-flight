using UnityEngine;

/// <summary>
/// GameManager
/// Purpose: Manages the high-level game flow and state transitions.
/// Responsibilities:
/// - Expose a singleton instance.
/// - Track current game state.
/// - Start and crash the game.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Crashed
    }

    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public bool IsPlaying => CurrentState == GameState.Playing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Duplicate instance detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[GameManager] Singleton initialized.");
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Debug.Log("[GameManager] Game started. State = Playing.");
    }

    public void CrashGame()
    {
        if (CurrentState == GameState.Crashed)
        {
            Debug.Log("[GameManager] CrashGame called, but game is already crashed.");
            return;
        }

        CurrentState = GameState.Crashed;
        Debug.Log("[GameManager] Game crashed. State = Crashed.");
    }
}
