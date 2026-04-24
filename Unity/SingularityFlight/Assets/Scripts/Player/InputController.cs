using UnityEngine;
using UnityEngine.InputSystem;

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

        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null || touchscreen.touches.Count <= 0)
        {
            if (pointerOwnedByTouch)
            {
                pointerOwnedByTouch = false;
                pointerActive = false;
            }
            return false;
        }

        UnityEngine.InputSystem.Controls.TouchControl touch = touchscreen.primaryTouch;
        if (touch == null || !touch.press.isPressed)
        {
            if (pointerOwnedByTouch)
            {
                pointerOwnedByTouch = false;
                pointerActive = false;
            }
            return false;
        }

        TouchPhase phase = touch.phase.ReadValue();
        Vector2 touchPosition = touch.position.ReadValue();
        if (phase == TouchPhase.Began)
        {
            pointerPosition = touchPosition;
            pointerActive = true;
            pointerOwnedByTouch = true;
            return false;
        }

        if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
        {
            pointerActive = false;
            pointerOwnedByTouch = false;
            return false;
        }

        if (!pointerActive)
        {
            pointerPosition = touchPosition;
            pointerActive = true;
            pointerOwnedByTouch = true;
            return false;
        }

        dragDelta = touchPosition - pointerPosition;
        pointerPosition = touchPosition;
        return true;
    }

    private bool TryGetMouseDrag(out Vector2 dragDelta)
    {
        dragDelta = Vector2.zero;
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return false;
        }

        bool mousePressed = mouse.leftButton.isPressed;
        if (mousePressed && !pointerActive && !pointerOwnedByTouch)
        {
            pointerPosition = mouse.position.ReadValue();
            pointerActive = true;
            pointerOwnedByTouch = false;
            return false;
        }

        if (!mousePressed)
        {
            if (!pointerOwnedByTouch)
            {
                pointerActive = false;
            }
            return false;
        }

        if (!pointerActive)
        {
            return false;
        }

        Vector2 currentPosition = mouse.position.ReadValue();
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
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        float vertical = 0f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        return new Vector2(horizontal, vertical).normalized;
    }
}
