using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TunnelManager
/// Purpose: Maintain a pooled ring of tunnel segments that recycles ahead of the player.
/// Responsibilities:
/// - Keep an active segment window of 20-25 pieces.
/// - Spawn/reposition segments ahead of the player.
/// - Despawn/recycle segments behind the player.
/// - Apply deterministic, noise-driven twist and turning offsets.
/// </summary>
public sealed class TunnelManager : MonoBehaviour
{
    private const int MinSegmentCount = 20;
    private const int MaxSegmentCount = 25;

    [Header("Segment Settings")]
    [SerializeField, Range(MinSegmentCount, MaxSegmentCount)] private int activeSegmentCount = 22;
    [SerializeField, Min(5f)] private float segmentLength = 25f;
    [SerializeField, Min(0f)] private float despawnBuffer = 5f;

    [Header("Curvature")]
    [SerializeField, Range(0f, 10f)] private float yawStepDegrees = 2.8f;
    [SerializeField, Range(0f, 8f)] private float pitchStepDegrees = 2f;
    [SerializeField, Range(0f, 18f)] private float rollStepDegrees = 9f;
    [SerializeField, Min(0.001f)] private float curvatureNoiseFrequency = 0.035f;
    [SerializeField, Min(0.001f)] private float rollNoiseFrequency = 0.05f;
    [SerializeField, Range(0f, 2f)] private float turnSwayAmplitude = 1.15f;

    [Header("References")]
    [SerializeField] private Transform tunnelRoot;
    [SerializeField] private DroneController droneController;
    [SerializeField] private GameObject[] segmentPrefabs;

    private readonly Queue<SegmentHandle> activeSegments = new();

    private SeedGenerator.DeterministicRandom seededRandom;
    private float traveledDistance;
    private float nextSpawnDistance;
    private int createdSegmentCount;

    private float yawNoiseOffset;
    private float pitchNoiseOffset;
    private float rollNoiseOffset;
    private float swayXNoiseOffset;
    private float swayYNoiseOffset;

    public int ActiveSegments => activeSegmentCount;
    public float SegmentLength => segmentLength;
    public int DailySeed => SeedGenerator.GetDailySeedUtc();
    public float TraveledDistance => traveledDistance;

    private void Awake()
    {
        activeSegmentCount = Mathf.Clamp(activeSegmentCount, MinSegmentCount, MaxSegmentCount);
        seededRandom = SeedGenerator.CreateDeterministicRandom(DailySeed);

        yawNoiseOffset = seededRandom.NextFloat(-1000f, 1000f);
        pitchNoiseOffset = seededRandom.NextFloat(-1000f, 1000f);
        rollNoiseOffset = seededRandom.NextFloat(-1000f, 1000f);
        swayXNoiseOffset = seededRandom.NextFloat(-1000f, 1000f);
        swayYNoiseOffset = seededRandom.NextFloat(-1000f, 1000f);

        if (tunnelRoot == null)
        {
            tunnelRoot = transform;
        }

        nextSpawnDistance = 0f;
    }

    private void Start()
    {
        BuildInitialSegments();
    }

    public void InitializeForRuntime(Transform runtimeTunnelRoot, DroneController runtimeDroneController, GameObject[] runtimeSegmentPrefabs)
    {
        tunnelRoot = runtimeTunnelRoot;
        droneController = runtimeDroneController;
        segmentPrefabs = runtimeSegmentPrefabs;
    }

    private void Update()
    {
        if (droneController == null || tunnelRoot == null)
        {
            return;
        }

        traveledDistance = droneController.TraveledDistance;
        RecycleSegmentsBehindPlayer();
    }

    private void BuildInitialSegments()
    {
        for (int i = 0; i < activeSegmentCount; i++)
        {
            Transform segment = CreateSegmentInstance();
            if (segment == null)
            {
                return;
            }

            PlaceSegment(segment, nextSpawnDistance);
            activeSegments.Enqueue(new SegmentHandle(segment, nextSpawnDistance));
            nextSpawnDistance += segmentLength;
        }
    }

    private void RecycleSegmentsBehindPlayer()
    {
        float despawnThreshold = traveledDistance - despawnBuffer;

        while (activeSegments.Count > 0)
        {
            SegmentHandle oldest = activeSegments.Peek();
            float oldestEndDistance = oldest.StartDistance + segmentLength;

            if (oldestEndDistance >= despawnThreshold)
            {
                break;
            }

            SegmentHandle recycled = activeSegments.Dequeue();
            PlaceSegment(recycled.Transform, nextSpawnDistance);
            activeSegments.Enqueue(new SegmentHandle(recycled.Transform, nextSpawnDistance));
            nextSpawnDistance += segmentLength;
        }
    }

    private Transform CreateSegmentInstance()
    {
        GameObject prefab = GetRandomPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("[TunnelManager] No segment prefab assigned. Skipping tunnel generation.");
            return null;
        }

        GameObject instance = Instantiate(prefab, tunnelRoot);
        instance.name = $"TunnelSegment_{createdSegmentCount:D2}";
        createdSegmentCount++;
        instance.SetActive(true);
        return instance.transform;
    }

    private GameObject GetRandomPrefab()
    {
        if (segmentPrefabs == null || segmentPrefabs.Length == 0)
        {
            return null;
        }

        int index = seededRandom.NextInt(0, segmentPrefabs.Length);
        return segmentPrefabs[index];
    }

    private void PlaceSegment(Transform segment, float startDistance)
    {
        float centerDistance = startDistance + (segmentLength * 0.5f);

        Vector3 centerPosition = CalculateTurnOffset(centerDistance);
        Quaternion rotation = CalculateTwistRotation(centerDistance);

        segment.SetLocalPositionAndRotation(centerPosition + (Vector3.forward * centerDistance), rotation);
    }

    private Quaternion CalculateTwistRotation(float distance)
    {
        float curvatureSample = distance * curvatureNoiseFrequency;
        float rollSample = distance * rollNoiseFrequency;

        float yaw = (Mathf.PerlinNoise(yawNoiseOffset, curvatureSample) * 2f - 1f) * yawStepDegrees;
        float pitch = (Mathf.PerlinNoise(pitchNoiseOffset, curvatureSample) * 2f - 1f) * pitchStepDegrees;
        float roll = (Mathf.PerlinNoise(rollNoiseOffset, rollSample) * 2f - 1f) * rollStepDegrees;

        return Quaternion.Euler(pitch, yaw, roll);
    }

    private Vector3 CalculateTurnOffset(float distance)
    {
        float sample = distance * curvatureNoiseFrequency;
        float swayX = (Mathf.PerlinNoise(swayXNoiseOffset, sample) * 2f - 1f) * turnSwayAmplitude;
        float swayY = (Mathf.PerlinNoise(swayYNoiseOffset, sample) * 2f - 1f) * turnSwayAmplitude;
        return new Vector3(swayX, swayY, 0f);
    }

    private readonly struct SegmentHandle
    {
        public SegmentHandle(Transform transform, float startDistance)
        {
            Transform = transform;
            StartDistance = startDistance;
        }

        public Transform Transform { get; }
        public float StartDistance { get; }
    }
}
