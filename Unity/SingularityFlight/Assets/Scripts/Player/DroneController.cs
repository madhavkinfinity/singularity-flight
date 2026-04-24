using UnityEngine;

/// <summary>
/// DroneController
/// Purpose: Move the drone forward at a constant speed and apply smoothed steering from InputController.
/// Responsibilities:
/// - Apply constant forward movement each frame.
/// - Consume normalized steering input from InputController.
/// - Limit turning by max turn rate and smooth orientation changes.
/// </summary>
public sealed class DroneController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputController inputController;

    [Header("Movement")]
    [SerializeField, Min(1f)] private float forwardSpeed = 30f;
    [SerializeField, Min(1f)] private float maxTurnRateDegreesPerSecond = 120f;
    [SerializeField, Range(0.01f, 0.5f)] private float steeringSmoothing = 0.1f;

    private const float MinDirectionSqrMagnitude = 0.0001f;

    private Vector2 smoothedInput;
    private bool movementEnabled = true;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

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

        UpdateSmoothedInput();
        UpdateRotation();
        UpdatePosition();
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

    private void UpdateSmoothedInput()
    {
        Vector2 targetInput = inputController != null ? inputController.SteeringInput : Vector2.zero;
        float interpolation = 1f - Mathf.Exp(-Time.deltaTime / steeringSmoothing);
        smoothedInput = Vector2.Lerp(smoothedInput, targetInput, interpolation);
    }

    private void UpdateRotation()
    {
        Vector3 steeringDirection = transform.forward + (transform.right * smoothedInput.x) + (transform.up * smoothedInput.y);
        if (steeringDirection.sqrMagnitude < MinDirectionSqrMagnitude)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(steeringDirection.normalized, transform.up);
        float maxStep = maxTurnRateDegreesPerSecond * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxStep);
    }

    private void UpdatePosition()
    {
        transform.position += transform.forward * (forwardSpeed * Time.deltaTime);
    }
}
