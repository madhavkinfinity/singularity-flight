using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObstacleSystem
/// Purpose: Spawn and recycle deterministic geometric obstacles ahead of the player.
/// Responsibilities:
/// - Compute spawn pacing from distance-based difficulty.
/// - Spawn deterministic obstacle patterns that preserve at least one safe lane.
/// - Recycle pooled obstacles that move behind the player.
/// </summary>
public sealed class ObstacleSystem : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField, Min(0.35f)] private float minSpawnInterval = 0.45f;
    [SerializeField, Min(0.5f)] private float maxSpawnInterval = 1.35f;
    [SerializeField, Min(100f)] private float maxDifficultyDistance = 3000f;
    [SerializeField, Min(10f)] private float spawnDistanceAhead = 90f;
    [SerializeField, Min(5f)] private float despawnDistanceBehind = 15f;

    [Header("Pattern")]
    [SerializeField, Min(0.5f)] private float laneRadius = 1.6f;
    [SerializeField, Min(0f)] private float laneJitter = 0.28f;
    [SerializeField, Min(0.2f)] private float minScale = 0.65f;
    [SerializeField, Min(0.2f)] private float maxScale = 1.35f;

    [Header("Pool")]
    [SerializeField, Min(6)] private int poolSize = 24;
    [SerializeField] private ObstacleBase[] obstaclePrefabs;

    [Header("Palette")]
    [SerializeField] private Color[] obstacleColors =
    {
        new(0.97f, 0.34f, 0.24f),
        new(0.15f, 0.79f, 0.95f),
        new(1f, 0.76f, 0.18f),
        new(0.36f, 0.93f, 0.56f),
        new(0.72f, 0.46f, 0.98f)
    };

    [Header("References")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private TunnelManager tunnelManager;
    [SerializeField] private Transform obstacleRoot;

    private static readonly Vector2[] LaneOffsets =
    {
        new(0f, 0f),
        new(1f, 0f),
        new(-1f, 0f),
        new(0f, 1f),
        new(0f, -1f),
        new(0.72f, 0.72f),
        new(-0.72f, 0.72f),
        new(0.72f, -0.72f),
        new(-0.72f, -0.72f)
    };

    private readonly Queue<ObstacleBase> pooledObstacles = new();
    private readonly List<ObstacleBase> activeObstacles = new();
    private readonly MaterialPropertyBlock materialPropertyBlock = new();

    private SeedGenerator.DeterministicRandom seededRandom;
    private float spawnTimer;

    private void Awake()
    {
        seededRandom = SeedGenerator.CreateDeterministicRandom(SeedGenerator.GetDailySeedUtc());

        if (obstacleRoot == null)
        {
            obstacleRoot = transform;
        }

        BuildPool();
    }

    public void InitializeForRuntime(DroneController runtimeDroneController, TunnelManager runtimeTunnelManager, Transform runtimeObstacleRoot, ObstacleBase[] runtimeObstaclePrefabs)
    {
        droneController = runtimeDroneController;
        tunnelManager = runtimeTunnelManager;
        obstacleRoot = runtimeObstacleRoot;
        obstaclePrefabs = runtimeObstaclePrefabs;

        if (pooledObstacles.Count == 0)
        {
            BuildPool();
        }
    }

    private void Update()
    {
        if (droneController == null || tunnelManager == null)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnPattern();
            spawnTimer = GetSpawnInterval(tunnelManager.TraveledDistance);
        }

        DespawnBehindPlayer();
    }

    public float GetSpawnInterval(float distanceMeters)
    {
        float t = Mathf.Clamp01(distanceMeters / maxDifficultyDistance);
        return Mathf.Lerp(maxSpawnInterval, minSpawnInterval, t);
    }

    private void BuildPool()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            ObstacleBase prefab = obstaclePrefabs[i % obstaclePrefabs.Length];
            if (prefab == null)
            {
                continue;
            }

            ObstacleBase obstacle = Instantiate(prefab, obstacleRoot);
            obstacle.gameObject.SetActive(false);
            pooledObstacles.Enqueue(obstacle);
        }
    }

    private void SpawnPattern()
    {
        if (pooledObstacles.Count == 0)
        {
            return;
        }

        float difficulty = Mathf.Clamp01(tunnelManager.TraveledDistance / maxDifficultyDistance);
        int maxCount = difficulty < 0.33f ? 1 : (difficulty < 0.75f ? 2 : 3);
        int spawnCount = seededRandom.NextInt(1, maxCount + 1);

        int safeLaneIndex = seededRandom.NextInt(0, LaneOffsets.Length);
        HashSet<int> usedLanes = new() { safeLaneIndex };

        for (int i = 0; i < spawnCount; i++)
        {
            if (pooledObstacles.Count == 0)
            {
                return;
            }

            int laneIndex = ChooseLane(usedLanes);
            if (laneIndex < 0)
            {
                return;
            }

            usedLanes.Add(laneIndex);
            SpawnSingleObstacle(LaneOffsets[laneIndex]);
        }
    }

    private int ChooseLane(HashSet<int> usedLanes)
    {
        List<int> availableLanes = new();
        for (int i = 0; i < LaneOffsets.Length; i++)
        {
            if (!usedLanes.Contains(i))
            {
                availableLanes.Add(i);
            }
        }

        if (availableLanes.Count == 0)
        {
            return -1;
        }

        int chosen = seededRandom.NextInt(0, availableLanes.Count);
        return availableLanes[chosen];
    }

    private void SpawnSingleObstacle(Vector2 laneOffset)
    {
        ObstacleBase obstacle = pooledObstacles.Dequeue();

        Vector3 forward = droneController.TravelDirection.normalized;
        Vector3 spawnPosition = droneController.transform.position + (forward * spawnDistanceAhead);

        float jitterX = seededRandom.NextFloat(-laneJitter, laneJitter);
        float jitterY = seededRandom.NextFloat(-laneJitter, laneJitter);
        Vector3 lateralOffset = droneController.TravelRight * ((laneOffset.x * laneRadius) + jitterX);
        Vector3 verticalOffset = droneController.TravelUp * ((laneOffset.y * laneRadius) + jitterY);
        spawnPosition += lateralOffset + verticalOffset;

        float yaw = seededRandom.NextFloat(0f, 360f);
        float pitch = seededRandom.NextFloat(0f, 360f);
        Quaternion spawnRotation = Quaternion.Euler(pitch, yaw, 0f);

        obstacle.Spawn(spawnPosition, spawnRotation, obstacleRoot);
        float scale = seededRandom.NextFloat(minScale, maxScale);
        obstacle.transform.localScale = Vector3.one * scale;

        TintObstacle(obstacle);
        activeObstacles.Add(obstacle);
    }

    private void TintObstacle(ObstacleBase obstacle)
    {
        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer == null || obstacleColors == null || obstacleColors.Length == 0)
        {
            return;
        }

        Color color = obstacleColors[seededRandom.NextInt(0, obstacleColors.Length)];
        materialPropertyBlock.Clear();
        materialPropertyBlock.SetColor("_BaseColor", color);
        materialPropertyBlock.SetColor("_Color", color);
        materialPropertyBlock.SetColor("_EmissionColor", color * 0.9f);
        renderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void DespawnBehindPlayer()
    {
        Vector3 playerPosition = droneController.transform.position;
        Vector3 playerForward = droneController.TravelDirection;

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            ObstacleBase obstacle = activeObstacles[i];
            Vector3 toObstacle = obstacle.transform.position - playerPosition;
            float forwardDistance = Vector3.Dot(toObstacle, playerForward);

            if (forwardDistance >= -despawnDistanceBehind)
            {
                continue;
            }

            obstacle.Despawn();
            activeObstacles.RemoveAt(i);
            pooledObstacles.Enqueue(obstacle);
        }
    }
}
