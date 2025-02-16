using System.Collections;
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
        while (networkManager.players.Count < minPlayers) {
            // TODO: Add a message to the UI to show that the game is waiting for players
            yield return null;
        }

        machine.Next();
    }
}
