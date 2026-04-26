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
            cameraFollow.ApplyThirdPersonPreset();
        }

        TunnelManager tunnelManager = FindAnyObjectByType<TunnelManager>();
        if (tunnelManager == null)
        {
            GameObject tunnelRoot = new("TunnelRoot");
            tunnelManager = tunnelRoot.AddComponent<TunnelManager>();
        }

        GameObject segmentPrefab = RuntimeTunnelSegmentFactory.CreateSegmentPrefab();
        tunnelManager.InitializeForRuntime(tunnelManager.transform, droneController, new[] { segmentPrefab });

        ObstacleSystem obstacleSystem = FindAnyObjectByType<ObstacleSystem>();
        if (obstacleSystem == null)
        {
            GameObject obstacleRoot = new("ObstacleSystem");
            obstacleSystem = obstacleRoot.AddComponent<ObstacleSystem>();
        }

        ObstacleBase[] obstaclePrefabs = CreateObstaclePrefabs();
        obstacleSystem.InitializeForRuntime(droneController, tunnelManager, obstacleSystem.transform, obstaclePrefabs);
    }

    private static DroneController CreateDefaultDrone(InputController inputController, GameManager gameManager)
    {
        GameObject drone = new("Drone");
        drone.name = "Drone";
        drone.transform.position = Vector3.zero;

        DroneController droneController = drone.AddComponent<DroneController>();
        droneController.Initialize(inputController);
        drone.AddComponent<DroneHybridVisual>();

        CollisionSystem collisionSystem = drone.AddComponent<CollisionSystem>();
        collisionSystem.Initialize(gameManager);

        SphereCollider sphereCollider = drone.AddComponent<SphereCollider>();
        sphereCollider.radius = 0.35f;
        sphereCollider.isTrigger = true;

        Rigidbody body = drone.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        return droneController;
    }

    private static ObstacleBase[] CreateObstaclePrefabs()
    {
        ObstacleBase[] prefabs =
        {
            CreateSolidShapePrefab("RuntimeCubeObstaclePrefab", PrimitiveType.Cube, new Vector3(1.15f, 1.15f, 1.15f), new Color(0.96f, 0.31f, 0.25f)),
            CreateSolidShapePrefab("RuntimeSphereObstaclePrefab", PrimitiveType.Sphere, new Vector3(1.25f, 1.25f, 1.25f), new Color(0.16f, 0.76f, 0.97f)),
            CreateSolidShapePrefab("RuntimeCapsuleObstaclePrefab", PrimitiveType.Capsule, new Vector3(0.95f, 1.45f, 0.95f), new Color(1f, 0.72f, 0.2f)),
            CreateSolidShapePrefab("RuntimeCylinderObstaclePrefab", PrimitiveType.Cylinder, new Vector3(1.05f, 1.1f, 1.05f), new Color(0.4f, 0.91f, 0.54f))
        };

        return prefabs;
    }

    private static ObstacleBase CreateSolidShapePrefab(string prefabName, PrimitiveType primitiveType, Vector3 localScale, Color baseColor)
    {
        GameObject obstacle = GameObject.CreatePrimitive(primitiveType);
        obstacle.name = prefabName;
        obstacle.transform.localScale = localScale;

        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = baseColor;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", baseColor * 0.75f);
            renderer.sharedMaterial = material;
        }

        SolidShapeObstacle shapeObstacle = obstacle.AddComponent<SolidShapeObstacle>();
        obstacle.SetActive(false);
        return shapeObstacle;
    }
}
