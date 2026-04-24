using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainSceneBootstrap
/// Purpose: Auto-wire a playable baseline setup for Main.unity when no gameplay objects exist.
/// Responsibilities:
/// - Ensure GameManager, InputController, DroneController, and CameraFollow exist.
/// - Build a default tunnel segment prefab at runtime.
/// - Initialize TunnelManager so Play mode immediately shows motion.
/// </summary>
public sealed class MainSceneBootstrap : MonoBehaviour
{
    private const string MainSceneName = "Main";


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBootstrapObject()
    {
        if (SceneManager.GetActiveScene().name != MainSceneName)
        {
            return;
        }

        if (FindAnyObjectByType<MainSceneBootstrap>() != null)
        {
            return;
        }

        new GameObject("MainSceneBootstrap").AddComponent<MainSceneBootstrap>();
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != MainSceneName)
        {
            return;
        }

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            gameManager = new GameObject("GameManager").AddComponent<GameManager>();
        }

        InputController inputController = FindAnyObjectByType<InputController>();
        if (inputController == null)
        {
            inputController = new GameObject("InputController").AddComponent<InputController>();
        }

        DroneController droneController = FindAnyObjectByType<DroneController>();
        if (droneController == null)
        {
            droneController = CreateDefaultDrone(inputController, gameManager);
        }

        CameraFollow cameraFollow = FindAnyObjectByType<CameraFollow>();
        if (cameraFollow == null && Camera.main != null)
        {
            cameraFollow = Camera.main.gameObject.AddComponent<CameraFollow>();
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(droneController.transform);
        }

        TunnelManager tunnelManager = FindAnyObjectByType<TunnelManager>();
        if (tunnelManager == null)
        {
            GameObject tunnelRoot = new("TunnelRoot");
            tunnelManager = tunnelRoot.AddComponent<TunnelManager>();
        }

        GameObject segmentPrefab = CreateSegmentPrefab();
        tunnelManager.InitializeForRuntime(tunnelManager.transform, droneController, new[] { segmentPrefab });
    }

    private static DroneController CreateDefaultDrone(InputController inputController, GameManager gameManager)
    {
        GameObject drone = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        drone.name = "Drone";
        drone.transform.position = Vector3.zero;

        DroneController droneController = drone.AddComponent<DroneController>();
        droneController.Initialize(inputController);

        CollisionSystem collisionSystem = drone.AddComponent<CollisionSystem>();
        collisionSystem.Initialize(gameManager);

        SphereCollider sphereCollider = drone.AddComponent<SphereCollider>();
        sphereCollider.radius = 0.35f;

        return droneController;
    }

    private static GameObject CreateSegmentPrefab()
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.name = "RuntimeSegmentPrefab";
        segment.transform.localScale = new Vector3(6f, 6f, 25f);
        Collider collider = segment.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        segment.SetActive(false);
        return segment;
    }
}
