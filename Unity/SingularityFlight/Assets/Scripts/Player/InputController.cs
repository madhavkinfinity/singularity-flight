using UnityEngine;

/// <summary>
/// InputController
/// Purpose: Capture touch/mouse/keyboard input and expose normalized steering.
/// Responsibilities:
/// - Read user input sources.
/// - Output normalized Vector2 steering in range [-1, 1].
/// </summary>
public sealed class InputController : MonoBehaviour
{
    private const string HorizontalAxis = "Horizontal";
    private const string VerticalAxis = "Vertical";

    public Vector2 SteeringInput { get; private set; }

    private void Update()
    {
        SteeringInput = ReadSteeringInput();
    }

    private static Vector2 ReadSteeringInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 normalized = touch.deltaPosition / Mathf.Max(Screen.width, Screen.height);
            return Vector2.ClampMagnitude(normalized * 8f, 1f);
        }

        float horizontal = Input.GetAxisRaw(HorizontalAxis);
        float vertical = Input.GetAxisRaw(VerticalAxis);
        return Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
    }
}
