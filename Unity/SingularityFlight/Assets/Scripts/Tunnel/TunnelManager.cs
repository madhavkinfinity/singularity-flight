using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TunnelManager
/// Purpose: Maintain a pooled ring of tunnel segments that recycles ahead of the player.
/// Responsibilities:
/// - Keep an active segment window of 20-25 pieces.
/// - Spawn/reposition segments ahead of the player.
/// - Despawn/recycle segments behind the player.
/// </summary>
public sealed class TunnelManager : MonoBehaviour
{
    private const int MinSegmentCount = 20;
    private const int MaxSegmentCount = 25;

    [Header("Segment Settings")]
    [SerializeField, Range(MinSegmentCount, MaxSegmentCount)] private int activeSegmentCount = 22;
    [SerializeField, Min(5f)] private float segmentLength = 25f;
    [SerializeField, Min(0f)] private float despawnBuffer = 5f;

    [Header("References")]
    [SerializeField] private Transform tunnelRoot;
    [SerializeField] private DroneController droneController;
    [SerializeField] private GameObject[] segmentPrefabs;

    private readonly Queue<SegmentHandle> activeSegments = new();

    private System.Random seededRandom;
    private float traveledDistance;
    private float nextSpawnDistance;
    private int createdSegmentCount;

    public int ActiveSegments => activeSegmentCount;
    public float SegmentLength => segmentLength;
    public int DailySeed => ComputeDailySeed();

    private void Awake()
    {
        activeSegmentCount = Mathf.Clamp(activeSegmentCount, MinSegmentCount, MaxSegmentCount);
        seededRandom = new System.Random(DailySeed);

        if (tunnelRoot == null)
        {
            tunnelRoot = transform;
        }

        BuildInitialSegments();
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
        if (forward.sqrMagnitude <= Mathf.Epsilon)
        {
            forward = Vector3.forward;
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
            throw new InvalidOperationException("[TunnelManager] At least one segment prefab must be assigned.");
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

        int index = seededRandom.Next(0, segmentPrefabs.Length);
        return segmentPrefabs[index];
    }

    private void PlaceSegment(Transform segment, float startDistance)
    {
        Vector3 localPosition = Vector3.forward * startDistance;
        segment.SetLocalPositionAndRotation(localPosition, Quaternion.identity);
    }

    private static int ComputeDailySeed()
    {
        DateTime utcDate = DateTime.UtcNow.Date;
        string seedText = utcDate.ToString("yyyyMMdd");
        return seedText.GetHashCode();
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
