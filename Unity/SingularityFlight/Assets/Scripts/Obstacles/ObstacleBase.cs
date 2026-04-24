using UnityEngine;

/// <summary>
/// ObstacleBase
/// Purpose: Shared obstacle behavior for spawn/despawn lifecycle and collider safety.
/// Responsibilities:
/// - Provide a standard spawn entry point.
/// - Guarantee a collider is present.
/// - Handle activation/deactivation state.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class ObstacleBase : MonoBehaviour
{
    private Collider cachedCollider;

    protected virtual void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        if (cachedCollider == null)
        {
            cachedCollider = gameObject.AddComponent<BoxCollider>();
        }

        cachedCollider.isTrigger = false;
    }

    public virtual void Spawn(Vector3 worldPosition, Quaternion worldRotation, Transform parent)
    {
        transform.SetParent(parent, true);
        transform.SetPositionAndRotation(worldPosition, worldRotation);
        gameObject.SetActive(true);
    }

    public virtual void Despawn()
    {
        gameObject.SetActive(false);
    }
}
