using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUDController
/// Purpose: Update run distance text and toggle death overlay visibility.
/// Responsibilities:
/// - Present distance score.
/// - Show/hide crash state UI.
/// </summary>
public sealed class HUDController : MonoBehaviour
{
    [SerializeField] private DistanceTracker distanceTracker;
    [SerializeField] private Text distanceText;
    [SerializeField] private GameObject deathOverlay;

    private void Update()
    {
        if (distanceTracker == null || distanceText == null)
        {
            return;
        }

        distanceText.text = $"{Mathf.FloorToInt(distanceTracker.CurrentDistanceMeters)} m";
    }

    public void ShowDeathOverlay(float finalDistanceMeters)
    {
        if (deathOverlay != null)
        {
            deathOverlay.SetActive(true);
        }

        if (distanceText != null)
        {
            distanceText.text = $"{Mathf.FloorToInt(finalDistanceMeters)} m";
        }
    }

    public void HideDeathOverlay()
    {
        if (deathOverlay != null)
        {
            deathOverlay.SetActive(false);
        }
    }
}
