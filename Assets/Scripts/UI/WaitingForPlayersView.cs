using UnityEngine;
using PurrNet;
using TMPro;

public class WaitingForPlayersView : View
{

    [SerializeField] private TMP_Text playerCountText;
    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<WaitingForPlayersView>();
    }

    public override void OnShow()
    {
    }

    public override void OnHide()
    {
    }
}

