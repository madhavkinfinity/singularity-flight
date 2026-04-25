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
    private DroneController droneController;

    [Header("Follow")]
    [SerializeField] private Vector3 followOffset = new(0f, 1.7f, -6.1f);
    [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.18f;

    [Header("Look")]
    [SerializeField, Min(0f)] private float lookAheadDistance = 6f;
    [SerializeField, Min(0f)] private float lookAtHeight = 0.45f;
    [SerializeField, Min(1f)] private float rotationLerpSpeed = 8f;

    private Vector3 followVelocity;

    public void ApplyThirdPersonPreset()
    {
        followOffset = new Vector3(0f, 1.0f, -3.15f);
        lookAheadDistance = 8.5f;
        lookAtHeight = 0.4f;
        positionSmoothTime = 0.08f;
        rotationLerpSpeed = 10f;

        SnapToFollowPose();
    }

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        droneController = followTarget != null ? followTarget.GetComponent<DroneController>() : null;
        SnapToFollowPose();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Quaternion frameRotation = droneController != null ? droneController.TravelRotation : target.rotation;
        Vector3 desiredPosition = target.position
            + (frameRotation * Vector3.right * followOffset.x)
            + (frameRotation * Vector3.up * followOffset.y)
            + (frameRotation * Vector3.forward * followOffset.z);
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            positionSmoothTime);

        Vector3 lookDirectionBasis = droneController != null ? droneController.TravelDirection : target.forward;
        Vector3 lookPoint = target.position + (Vector3.up * lookAtHeight) + (lookDirectionBasis * lookAheadDistance);
        Vector3 lookDirection = lookPoint - transform.position;
        if (lookDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        float rotationStep = rotationLerpSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationStep);
    }

    private void SnapToFollowPose()
    {
        if (target == null)
        {
            return;
        }

        Quaternion frameRotation = droneController != null ? droneController.TravelRotation : target.rotation;
        transform.position = target.position
            + (frameRotation * Vector3.right * followOffset.x)
            + (frameRotation * Vector3.up * followOffset.y)
            + (frameRotation * Vector3.forward * followOffset.z);

        Vector3 lookDirectionBasis = droneController != null ? droneController.TravelDirection : target.forward;
        Vector3 lookDirection = (target.position + (Vector3.up * lookAtHeight) + (lookDirectionBasis * lookAheadDistance)) - transform.position;
        if (lookDirection.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }
    }
}
