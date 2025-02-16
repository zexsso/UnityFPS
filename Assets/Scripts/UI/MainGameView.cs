using PurrNet;
using TMPro;
using UnityEngine;

public class MainGameView : View
{
    [SerializeField] private TMP_Text healthText;


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
        healthText.text = health.ToString();
    }
}
