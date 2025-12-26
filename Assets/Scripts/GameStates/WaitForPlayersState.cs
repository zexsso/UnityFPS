using System.Collections;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class WaitForPlayersState : StateNode
{
    [SerializeField] private int minPlayers = 2;

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer) return;

        StartCoroutine(WaitForPlayers());
    }

    private IEnumerator WaitForPlayers()
    {
        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.LogError("WaitForPlayersState failed to get Instance GameViewManager", this);
            yield break;
        }
        gameViewManager.ShowView<WaitingForPlayersView>(false);


        while (networkManager.players.Count < minPlayers)
        {
            yield return null;
        }

        gameViewManager.HideView<WaitingForPlayersView>();
        machine.Next();
    }
}
