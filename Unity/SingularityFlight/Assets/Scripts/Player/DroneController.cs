using UnityEngine;

/// <summary>
/// DroneController
/// Purpose: Move the drone forward constantly while applying smoothed steering.
/// Responsibilities:
/// - Apply constant forward speed.
/// - Rotate using normalized steering input.
/// </summary>
public sealed class DroneController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputController inputController;

    [Header("Movement")]
    [SerializeField, Min(1f)] private float forwardSpeed = 30f;
    [SerializeField, Min(10f)] private float maxTurnRateDegrees = 120f;
    [SerializeField, Range(0.01f, 0.5f)] private float steeringSmoothing = 0.1f;

    private bool movementEnabled = true;
    private Quaternion spawnRotation;
    private Vector3 spawnPosition;
    private Vector2 smoothedInput;

    private void Awake()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    private void Update()
    {
        if (!movementEnabled)
        {
            return;
        }

        Vector2 targetInput = inputController != null ? inputController.SteeringInput : Vector2.zero;
        float interpolation = 1f - Mathf.Exp(-Time.deltaTime / steeringSmoothing);
        smoothedInput = Vector2.Lerp(smoothedInput, targetInput, interpolation);

        float pitchDelta = -smoothedInput.y * maxTurnRateDegrees * Time.deltaTime;
        float yawDelta = smoothedInput.x * maxTurnRateDegrees * Time.deltaTime;

        transform.rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(pitchDelta, yawDelta, 0f));
        transform.position += transform.forward * (forwardSpeed * Time.deltaTime);
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
    }

    public void ResetTransform()
    {
        smoothedInput = Vector2.zero;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
    }
}
