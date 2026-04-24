using UnityEngine;

/// <summary>
/// TunnelManager
/// Purpose: Manage deterministic seed data and move tunnel geometry relative to a near-origin drone.
/// Responsibilities:
/// - Track active segment budget.
/// - Expose daily deterministic seed for world systems.
/// - Move tunnel root backward using drone forward speed.
/// </summary>
public sealed class TunnelManager : MonoBehaviour
{
    [Header("Segments")]
    [SerializeField, Min(5)] private int activeSegments = 20;
    [SerializeField, Min(5f)] private float segmentLength = 25f;

    [Header("Movement")]
    [SerializeField] private Transform tunnelRoot;
    [SerializeField] private DroneController droneController;
    [SerializeField] private Vector3 fallbackForward = Vector3.forward;

    public int ActiveSegments => activeSegments;
    public float SegmentLength => segmentLength;
    public int DailySeed => ComputeDailySeed();

    private void Update()
    {
        if (tunnelRoot == null || droneController == null)
        {
            return;
        }

        Vector3 droneForward = droneController.transform.forward;
        if (droneForward.sqrMagnitude <= Mathf.Epsilon)
        {
            droneForward = fallbackForward.normalized;
        }

        float travelDistance = droneController.ForwardSpeed * Time.deltaTime;
        tunnelRoot.position -= droneForward.normalized * travelDistance;
    }

    private static int ComputeDailySeed()
    {
        System.DateTime utcDate = System.DateTime.UtcNow.Date;
        string seedText = utcDate.ToString("yyyyMMdd");
        return seedText.GetHashCode();
    }
}
