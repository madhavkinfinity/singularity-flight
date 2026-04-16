using UnityEngine;

/// <summary>
/// TunnelManager
/// Purpose: Manage active tunnel segment count and deterministic seed selection.
/// Responsibilities:
/// - Track active segment budget.
/// - Expose daily deterministic seed for world systems.
/// </summary>
public sealed class TunnelManager : MonoBehaviour
{
    [SerializeField, Min(5)] private int activeSegments = 20;
    [SerializeField, Min(5f)] private float segmentLength = 25f;

    public int ActiveSegments => activeSegments;
    public float SegmentLength => segmentLength;

    public int DailySeed => ComputeDailySeed();

    private static int ComputeDailySeed()
    {
        System.DateTime utcDate = System.DateTime.UtcNow.Date;
        string seedText = utcDate.ToString("yyyyMMdd");
        return seedText.GetHashCode();
    }
}
