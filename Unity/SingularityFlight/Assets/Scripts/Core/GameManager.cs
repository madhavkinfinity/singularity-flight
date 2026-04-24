using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager
/// Purpose: Manages high-level game flow, crash handling, and run restart timing.
/// Responsibilities:
/// - Expose a singleton instance.
/// - Track game state transitions.
/// - Trigger crash and restart current run after a short delay.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Crashed
    }

    private const float RestartDelaySeconds = 1f;

    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public bool IsPlaying => CurrentState == GameState.Playing;

    private Coroutine restartCoroutine;

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

        if (restartCoroutine != null)
        {
            StopCoroutine(restartCoroutine);
        }

        restartCoroutine = StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(RestartDelaySeconds);

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }
}
