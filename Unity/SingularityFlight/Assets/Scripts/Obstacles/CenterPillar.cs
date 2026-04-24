using UnityEngine;

/// <summary>
/// CenterPillar
/// Purpose: Basic center-lane obstacle that blocks the middle of the tunnel.
/// Responsibilities:
/// - Define pillar dimensions.
/// - Ensure capsule collider setup.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public sealed class CenterPillar : ObstacleBase
{
    [SerializeField, Min(0.25f)] private float radius = 0.9f;
    [SerializeField, Min(0.5f)] private float height = 5f;

    protected override void Awake()
    {
        base.Awake();

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        capsule.direction = 1;
        capsule.radius = radius;
        capsule.height = height;
        capsule.center = Vector3.zero;
        capsule.isTrigger = false;
    }
}
