using UnityEngine;

/// <summary>
/// CameraFollow
/// Purpose: Follow the drone with smooth offset and stable orientation.
/// Responsibilities:
/// - Maintain chase-camera position.
/// - Look at the drone smoothly each frame.
/// </summary>
public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset = new(0f, 1.5f, -7f);
    [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.TransformPoint(followOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(target.position - transform.position, Vector3.up),
            Time.deltaTime * 10f);
    }
}
