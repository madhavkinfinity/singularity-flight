using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObstacleSystem
/// Purpose: Spawn and recycle deterministic obstacles ahead of the player.
/// Responsibilities:
/// - Compute spawn pacing from distance-based difficulty.
/// - Spawn pooled center-pillar obstacles using deterministic random.
/// - Despawn obstacles that move behind the player.
/// </summary>
public sealed class ObstacleSystem : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField, Min(0.5f)] private float minSpawnInterval = 0.5f;
    [SerializeField, Min(0.5f)] private float maxSpawnInterval = 1.5f;
    [SerializeField, Min(100f)] private float maxDifficultyDistance = 3000f;
    [SerializeField, Min(10f)] private float spawnDistanceAhead = 90f;
    [SerializeField, Min(5f)] private float despawnDistanceBehind = 15f;

    [Header("Pool")]
    [SerializeField, Min(1)] private int poolSize = 16;
    [SerializeField] private CenterPillar centerPillarPrefab;

    [Header("References")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private TunnelManager tunnelManager;
    [SerializeField] private Transform obstacleRoot;

    private readonly Queue<CenterPillar> pooledPillars = new();
    private readonly List<CenterPillar> activePillars = new();

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

    public void InitializeForRuntime(DroneController runtimeDroneController, TunnelManager runtimeTunnelManager, Transform runtimeObstacleRoot, CenterPillar runtimeCenterPillarPrefab)
    {
        droneController = runtimeDroneController;
        tunnelManager = runtimeTunnelManager;
        obstacleRoot = runtimeObstacleRoot;
        centerPillarPrefab = runtimeCenterPillarPrefab;

        if (pooledPillars.Count == 0)
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
            TrySpawnPillar();
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
        if (centerPillarPrefab == null)
        {
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            CenterPillar pillar = Instantiate(centerPillarPrefab, obstacleRoot);
            pillar.gameObject.SetActive(false);
            pooledPillars.Enqueue(pillar);
        }
    }

    private void TrySpawnPillar()
    {
        if (pooledPillars.Count == 0)
        {
            return;
        }

        CenterPillar pillar = pooledPillars.Dequeue();

        Vector3 forward = droneController.transform.forward.normalized;
        Vector3 spawnPosition = droneController.transform.position + (forward * spawnDistanceAhead);

        float lateralOffset = seededRandom.NextFloat(-1.5f, 1.5f);
        spawnPosition += droneController.transform.right * lateralOffset;

        pillar.Spawn(spawnPosition, Quaternion.identity, obstacleRoot);
        activePillars.Add(pillar);
    }

    private void DespawnBehindPlayer()
    {
        Vector3 playerPosition = droneController.transform.position;
        Vector3 playerForward = droneController.transform.forward;

        for (int i = activePillars.Count - 1; i >= 0; i--)
        {
            CenterPillar pillar = activePillars[i];
            Vector3 toObstacle = pillar.transform.position - playerPosition;
            float forwardDistance = Vector3.Dot(toObstacle, playerForward);

            if (forwardDistance >= -despawnDistanceBehind)
            {
                continue;
            }

            pillar.Despawn();
            activePillars.RemoveAt(i);
            pooledPillars.Enqueue(pillar);
        }
    }
}
