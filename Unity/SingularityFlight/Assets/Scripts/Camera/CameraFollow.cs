using UnityEngine;

/// <summary>
/// CameraFollow
/// Purpose: Follow the drone using a smooth, slightly lagged, stable chase camera.
/// Responsibilities:
/// - Track the drone with smoothed positional follow.
/// - Smoothly rotate toward a forward look target.
/// - Keep world-up stability to avoid camera roll.
/// </summary>
public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private Vector3 followOffset = new(0f, 1.5f, -7f);
    [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.18f;

    [Header("Look")]
    [SerializeField, Min(0f)] private float lookAheadDistance = 6f;
    [SerializeField, Min(1f)] private float rotationLerpSpeed = 8f;

    private Vector3 followVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.TransformPoint(followOffset);
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            positionSmoothTime);

        Vector3 lookPoint = target.position + (target.forward * lookAheadDistance);
        Vector3 lookDirection = lookPoint - transform.position;
        if (lookDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        float rotationStep = rotationLerpSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationStep);
    }
}
