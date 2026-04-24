using UnityEngine;

/// <summary>
/// DroneController
/// Purpose: Rotate the drone from steering input while preserving constant forward-speed semantics.
/// Responsibilities:
/// - Consume normalized steering input from InputController.
/// - Limit turning by max turn rate and smooth orientation changes.
/// - Keep the drone near its spawn position for floating-origin gameplay.
/// </summary>
public sealed class DroneController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputController inputController;

    [Header("Movement")]
    [SerializeField, Min(1f)] private float forwardSpeed = 30f;
    [SerializeField, Min(0.1f)] private float strafeSpeed = 8f;
    [SerializeField, Min(0.1f)] private float maxLateralOffset = 2f;
    [SerializeField, Range(1f, 45f)] private float maxTiltDegrees = 18f;
    [SerializeField, Min(1f)] private float maxTurnRateDegreesPerSecond = 120f;
    [SerializeField, Range(0.01f, 0.5f)] private float steeringSmoothing = 0.1f;

    private Vector2 smoothedInput;
    private bool movementEnabled = true;
    private Vector2 lateralOffset;
    private float traveledDistance;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    public float ForwardSpeed => forwardSpeed;
    public float TraveledDistance => traveledDistance;
    public Vector3 TravelDirection => spawnRotation * Vector3.forward;

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
        UpdatePosition();
        UpdateRotation();
    }


    public void Initialize(InputController controller)
    {
        inputController = controller;
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
    }

    public void ResetTransform()
    {
        smoothedInput = Vector2.zero;
        lateralOffset = Vector2.zero;
        traveledDistance = 0f;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
    }

    private void UpdateSmoothedInput()
    {
        Vector2 targetInput = inputController != null ? inputController.SteeringInput : Vector2.zero;
        float interpolation = 1f - Mathf.Exp(-Time.deltaTime / steeringSmoothing);
        smoothedInput = Vector2.Lerp(smoothedInput, targetInput, interpolation);
    }

    private void UpdatePosition()
    {
        traveledDistance += forwardSpeed * Time.deltaTime;
        lateralOffset += smoothedInput * (strafeSpeed * Time.deltaTime);
        lateralOffset = Vector2.ClampMagnitude(lateralOffset, maxLateralOffset);

        Vector3 worldOffset = (spawnRotation * Vector3.right * lateralOffset.x) + (spawnRotation * Vector3.up * lateralOffset.y);
        Vector3 forwardOffset = spawnRotation * Vector3.forward * traveledDistance;
        transform.position = spawnPosition + forwardOffset + worldOffset;
    }

    private void UpdateRotation()
    {
        Quaternion targetRotation = spawnRotation * Quaternion.Euler(-smoothedInput.y * maxTiltDegrees, smoothedInput.x * maxTiltDegrees, -smoothedInput.x * maxTiltDegrees * 0.5f);
        float maxStep = maxTurnRateDegreesPerSecond * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxStep);
    }
}
