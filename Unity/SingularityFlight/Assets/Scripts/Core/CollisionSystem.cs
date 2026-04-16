using UnityEngine;

/// <summary>
/// CollisionSystem
/// Purpose: Relay collision events from the drone collider to the GameManager.
/// Responsibilities:
/// - Detect trigger/collision contacts.
/// - Notify GameManager to crash current run.
/// </summary>
public sealed class CollisionSystem : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled || gameManager == null || !gameManager.IsPlaying)
        {
            return;
        }

        gameManager.CrashGame();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enabled || gameManager == null || !gameManager.IsPlaying)
        {
            return;
        }

        gameManager.CrashGame();
    }
}
