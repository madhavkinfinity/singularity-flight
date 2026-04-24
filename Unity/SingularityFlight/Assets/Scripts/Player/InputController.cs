using UnityEngine;

/// <summary>
/// InputController
/// Purpose: Capture drag input from touch and mouse, then provide smoothed normalized steering.
/// Responsibilities:
/// - Read touch and mouse drag deltas.
/// - Normalize steering output to range [-1, 1].
/// - Smooth steering to improve control feel.
/// </summary>
public sealed class InputController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField, Min(1f)] private float dragNormalizationPixels = 120f;
    [SerializeField, Range(0.01f, 0.5f)] private float inputSmoothing = 0.1f;

    private Vector2 pointerPosition;
    private Vector2 smoothedInput;
    private bool pointerActive;
    private bool pointerOwnedByTouch;

    public Vector2 SteeringInput => smoothedInput;

    public Vector2 GetSteeringInput()
    {
        return smoothedInput;
    }

    private void Update()
    {
        Vector2 targetInput = ReadDragInput();
        float interpolation = 1f - Mathf.Exp(-Time.deltaTime / inputSmoothing);
        smoothedInput = Vector2.Lerp(smoothedInput, targetInput, interpolation);
    }

    private Vector2 ReadDragInput()
    {
        if (TryGetTouchDrag(out Vector2 touchDrag))
        {
            return NormalizeDrag(touchDrag);
        }

        if (TryGetMouseDrag(out Vector2 mouseDrag))
        {
            return NormalizeDrag(mouseDrag);
        }

        return ReadKeyboardInput();
    }

    private bool TryGetTouchDrag(out Vector2 dragDelta)
    {
        dragDelta = Vector2.zero;

        if (Input.touchCount <= 0)
        {
            if (pointerOwnedByTouch)
            {
                pointerOwnedByTouch = false;
                pointerActive = false;
            }
            return false;
        }

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            pointerPosition = touch.position;
            pointerActive = true;
            pointerOwnedByTouch = true;
            return false;
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            pointerActive = false;
            pointerOwnedByTouch = false;
            return false;
        }

        if (!pointerActive)
        {
            pointerPosition = touch.position;
            pointerActive = true;
            pointerOwnedByTouch = true;
            return false;
        }

        dragDelta = touch.position - pointerPosition;
        pointerPosition = touch.position;
        return true;
    }

    private bool TryGetMouseDrag(out Vector2 dragDelta)
    {
        dragDelta = Vector2.zero;

        if (Input.GetMouseButtonDown(0))
        {
            pointerPosition = Input.mousePosition;
            pointerActive = true;
            pointerOwnedByTouch = false;
            return false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!pointerOwnedByTouch)
            {
                pointerActive = false;
            }
            return false;
        }

        if (!pointerActive || !Input.GetMouseButton(0))
        {
            return false;
        }

        Vector2 currentPosition = Input.mousePosition;
        dragDelta = currentPosition - pointerPosition;
        pointerPosition = currentPosition;
        return true;
    }

    private Vector2 NormalizeDrag(Vector2 dragDelta)
    {
        Vector2 normalized = dragDelta / dragNormalizationPixels;
        normalized.x = Mathf.Clamp(normalized.x, -1f, 1f);
        normalized.y = Mathf.Clamp(normalized.y, -1f, 1f);
        return normalized;
    }

    private static Vector2 ReadKeyboardInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical).normalized;
    }
}
