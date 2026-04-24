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

        ObstacleSystem obstacleSystem = FindAnyObjectByType<ObstacleSystem>();
        if (obstacleSystem == null)
        {
            GameObject obstacleRoot = new("ObstacleSystem");
            obstacleSystem = obstacleRoot.AddComponent<ObstacleSystem>();
        }

        CenterPillar centerPillarPrefab = CreateCenterPillarPrefab();
        obstacleSystem.InitializeForRuntime(droneController, tunnelManager, obstacleSystem.transform, centerPillarPrefab);
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

    private static GameObject CreateSegmentPrefab()
    {
        const float segmentLength = 25f;
        const float wallDistance = 3f;
        const float wallThickness = 0.5f;
        const float tunnelHeight = 6f;
        const float tunnelWidth = 6f;

        GameObject segment = new("RuntimeSegmentPrefab");
        BuildWall("LeftWall", segment.transform, new Vector3(-wallDistance, 0f, 0f), new Vector3(wallThickness, tunnelHeight, segmentLength));
        BuildWall("RightWall", segment.transform, new Vector3(wallDistance, 0f, 0f), new Vector3(wallThickness, tunnelHeight, segmentLength));
        BuildWall("TopWall", segment.transform, new Vector3(0f, wallDistance, 0f), new Vector3(tunnelWidth, wallThickness, segmentLength));
        BuildWall("BottomWall", segment.transform, new Vector3(0f, -wallDistance, 0f), new Vector3(tunnelWidth, wallThickness, segmentLength));

        segment.SetActive(false);
        return segment;
    }

    private static void BuildWall(string name, Transform parent, Vector3 localPosition, Vector3 localScale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = localPosition;
        wall.transform.localScale = localScale;

        Collider collider = wall.GetComponent<Collider>();
        if (collider is BoxCollider boxCollider)
        {
            boxCollider.isTrigger = false;
        }
    }

    private static CenterPillar CreateCenterPillarPrefab()
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "RuntimeCenterPillarPrefab";

        Renderer renderer = pillar.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.95f, 0.45f, 0.2f);
        }

        CenterPillar centerPillar = pillar.AddComponent<CenterPillar>();
        pillar.SetActive(false);
        return centerPillar;
    }
}
