using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;
using System.Collections;

public class GameEndState : StateNode
{
    [SerializeField] private StateNode spawningState;
    [SerializeField] private float gameRestartDelay = 5f;


    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
        {
            Debug.LogError("GameEndState failled to get Instance ScoreManager", this);
            return;
        }


        var winner = scoreManager.GetWinner();

        if (winner == default)
        {
            Debug.LogError("GameEndState failled to get winner", this);
            return;
        }

        Debug.Log($"Game now ended with {winner} as our champion");

        if (!InstanceHandler.TryGetInstance(out EndGameView endGameView))
        {
            Debug.LogError("GameEndState failled to get Instance EndGameView", this);
            return;
        }

        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.LogError("GameEndState failled to get Instance GameViewManager", this);
            return;
        }

        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>(false);


        StartCoroutine(DelayedStateChange(gameViewManager));

        if (!asServer) return;
    }

    private IEnumerator DelayedStateChange(GameViewManager gameViewManager)
    {
        yield return new WaitForSeconds(gameRestartDelay);
        gameViewManager.HideView<EndGameView>();

        //TODO: reset score
        machine.SetState(spawningState);
    }
}
