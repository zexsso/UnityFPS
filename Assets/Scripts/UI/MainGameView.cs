using PurrNet;
using TMPro;
using UnityEngine;

public class MainGameView : View
{
    [Header("Health UI")]
    [SerializeField] private TMP_Text healthText;

    [Header("Timer UI")]
    [SerializeField] private TMP_Text roundTimerText;
    [SerializeField] private GameObject timerContainer;

    [Header("Respawn UI")]
    [SerializeField] private GameObject respawnContainer;
    [SerializeField] private TMP_Text respawnTimerText;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<MainGameView>();
    }

    public override void OnHide()
    {
    }

    public override void OnShow()
    {
    }

    public void UpdateHealth(int health)
    {
        if (healthText == null) return;

        if (health <= 0) health = 0;
        healthText.text = health.ToString();
    }

    /// <summary>
    /// Shows or hides the round timer UI
    /// </summary>
    public void ShowRoundTimer(bool show)
    {
        if (timerContainer != null)
            timerContainer.SetActive(show);
    }

    /// <summary>
    /// Updates the round timer display
    /// </summary>
    public void UpdateRoundTimer(float remainingSeconds)
    {
        if (roundTimerText == null) return;

        int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
        roundTimerText.text = $"{minutes:00}:{seconds:00}";

        // Change color when time is running low
        if (remainingSeconds <= 30f)
        {
            roundTimerText.color = Color.red;
        }
        else if (remainingSeconds <= 60f)
        {
            roundTimerText.color = Color.yellow;
        }
        else
        {
            roundTimerText.color = Color.white;
        }
    }

    /// <summary>
    /// Shows the respawn countdown UI
    /// </summary>
    public void ShowRespawnTimer(bool show)
    {
        if (respawnContainer != null)
            respawnContainer.SetActive(show);
    }

    /// <summary>
    /// Updates the respawn timer display
    /// </summary>
    public void UpdateRespawnTimer(float remainingSeconds)
    {
        if (respawnTimerText == null) return;

        respawnTimerText.text = $"Respawning in {Mathf.CeilToInt(remainingSeconds)}...";
    }
}
