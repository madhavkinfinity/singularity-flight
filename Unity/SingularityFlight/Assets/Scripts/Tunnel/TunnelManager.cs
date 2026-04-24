using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TunnelManager
/// Purpose: Maintain a pooled ring of tunnel segments that recycles ahead of the player.
/// Responsibilities:
/// - Keep an active segment window of 20-25 pieces.
/// - Spawn/reposition segments ahead of the player.
/// - Despawn/recycle segments behind the player.
/// - Apply deterministic, noise-based curvature for smooth tunnel turns.
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
    [SerializeField, Range(0f, 10f)] private float yawStepDegrees = 4f;
    [SerializeField, Range(0f, 8f)] private float pitchStepDegrees = 2f;
    [SerializeField, Min(0.001f)] private float curvatureNoiseFrequency = 0.04f;
    [SerializeField, Range(0.05f, 1f)] private float turnSmoothing = 0.25f;

    [Header("References")]
    [SerializeField] private Transform tunnelRoot;
    [SerializeField] private DroneController droneController;
    [SerializeField] private GameObject[] segmentPrefabs;

    private readonly Queue<SegmentHandle> activeSegments = new();

    private SeedGenerator.DeterministicRandom seededRandom;
    private float traveledDistance;
    private float nextSpawnDistance;
    private int createdSegmentCount;

    private Vector3 nextSpawnLocalPosition;
    private Quaternion nextSpawnLocalRotation = Quaternion.identity;
    private float currentYawStep;
    private float currentPitchStep;
    private float yawNoiseOffset;
    private float pitchNoiseOffset;

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

        AdvanceTunnelRoot();
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

    private void AdvanceTunnelRoot()
    {
        float stepDistance = droneController.ForwardSpeed * Time.deltaTime;
        traveledDistance += stepDistance;

        Vector3 forward = droneController.transform.forward;
        if (droneController.TravelDirection.sqrMagnitude > Mathf.Epsilon)
        {
            forward = droneController.TravelDirection;
        }

        tunnelRoot.position -= forward.normalized * stepDistance;
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
        Quaternion segmentRotation = nextSpawnLocalRotation;
        Vector3 segmentCenter = nextSpawnLocalPosition + (segmentRotation * Vector3.forward * (segmentLength * 0.5f));
        segment.SetLocalPositionAndRotation(segmentCenter, segmentRotation);
        AdvanceCurvedSpawnPose(startDistance + segmentLength);
    }

    private void AdvanceCurvedSpawnPose(float distanceSample)
    {
        float sample = distanceSample * curvatureNoiseFrequency;

        float targetYawStep = (Mathf.PerlinNoise(yawNoiseOffset, sample) * 2f - 1f) * yawStepDegrees;
        float targetPitchStep = (Mathf.PerlinNoise(pitchNoiseOffset, sample) * 2f - 1f) * pitchStepDegrees;

        float smoothFactor = 1f - Mathf.Exp(-turnSmoothing);
        currentYawStep = Mathf.Lerp(currentYawStep, targetYawStep, smoothFactor);
        currentPitchStep = Mathf.Lerp(currentPitchStep, targetPitchStep, smoothFactor);

        Quaternion incrementalTurn = Quaternion.Euler(currentPitchStep, currentYawStep, 0f);
        nextSpawnLocalRotation = incrementalTurn * nextSpawnLocalRotation;
        nextSpawnLocalPosition += nextSpawnLocalRotation * (Vector3.forward * segmentLength);
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
