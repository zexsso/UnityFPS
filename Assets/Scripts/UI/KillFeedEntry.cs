using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual kill feed entry UI element.
/// Displays killer name, weapon icon, and victim name.
/// </summary>
public class KillFeedEntry : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text killerNameText;
    [SerializeField] private TMP_Text victimNameText;

    [Header("Icon References")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image headshotIcon;

    [Header("Colors")]
    [SerializeField] private Color normalKillColor = Color.white;
    [SerializeField] private Color headshotColor = new Color(1f, 0.8f, 0f); // Gold
    [SerializeField] private Color localPlayerColor = new Color(0.3f, 0.8f, 1f); // Light blue

    public void SetData(string killerName, string victimName, bool isHeadshot = false)
    {
        if (killerNameText != null)
        {
            killerNameText.text = killerName;

            // Highlight if local player
            if (PlayerController.LocalPlayer != null)
            {
                // Check if this is the local player (would need player name system)
                killerNameText.color = normalKillColor;
            }
        }

        if (victimNameText != null)
        {
            victimNameText.text = victimName;
            victimNameText.color = normalKillColor;
        }

        // Show/hide headshot icon
        if (headshotIcon != null)
        {
            headshotIcon.gameObject.SetActive(isHeadshot);
        }

        // Apply headshot color styling
        if (isHeadshot)
        {
            if (killerNameText != null)
                killerNameText.color = headshotColor;
        }
    }
}
