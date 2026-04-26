using UnityEngine;

/// <summary>
/// RuntimeMechanicalRotator
/// Purpose: Rotate runtime tunnel decoration parts to convey mechanical danger.
/// Responsibilities:
/// - Rotate around a configured local axis.
/// - Support per-instance speed values.
/// </summary>
public sealed class RuntimeMechanicalRotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float rotationSpeedDegrees = 180f;

    public void SetRotationAxisAndSpeed(Vector3 axis, float speedDegrees)
    {
        rotationAxis = axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.forward;
        rotationSpeedDegrees = speedDegrees;
    }

    private void Update()
    {
        float deltaAngle = rotationSpeedDegrees * Time.deltaTime;
        transform.Rotate(rotationAxis, deltaAngle, Space.Self);
    }
}
