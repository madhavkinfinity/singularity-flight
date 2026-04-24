using UnityEngine;

/// <summary>
/// CollisionSystem
/// Purpose: Detect drone contacts and relay crash requests to the GameManager.
/// Responsibilities:
/// - Detect trigger/collision contacts.
/// - Resolve a GameManager reference automatically when possible.
/// - Notify GameManager to crash current run.
/// </summary>
public sealed class CollisionSystem : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindFirstObjectByType<GameManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCrash();
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryCrash();
    }

    private void TryCrash()
    {
        if (!enabled || gameManager == null || !gameManager.IsPlaying)
        {
            return;
        }

        gameManager.CrashGame();
    }
}
