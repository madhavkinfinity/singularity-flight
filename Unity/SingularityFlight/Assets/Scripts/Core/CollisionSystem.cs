using UnityEngine;

/// <summary>
/// CollisionSystem
/// Purpose: Detect drone contacts and keep gameplay running while enforcing tunnel boundaries.
/// Responsibilities:
/// - Detect trigger/collision contacts.
/// - Push the drone back inside tunnel bounds when touching walls.
/// - Optionally relay crash requests to the GameManager.
/// </summary>
public sealed class CollisionSystem : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DroneController droneController;
    [SerializeField] private bool endRunOnCollision;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindAnyObjectByType<GameManager>();
        }

        if (droneController == null)
        {
            droneController = GetComponent<DroneController>();
        }
    }


    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnforceTunnelBoundary();
        TryCrash();
    }

    private void OnCollisionEnter(Collision collision)
    {
        EnforceTunnelBoundary();
        TryCrash();
    }

    private void EnforceTunnelBoundary()
    {
        if (droneController == null)
        {
            return;
        }

        droneController.ConstrainToTunnelBoundary();
    }

    private void TryCrash()
    {
        if (!enabled || !endRunOnCollision || gameManager == null || !gameManager.IsPlaying)
        {
            return;
        }

        gameManager.CrashGame();
    }
}
