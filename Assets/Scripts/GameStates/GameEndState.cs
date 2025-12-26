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
            Debug.LogError("GameEndState failed to get Instance ScoreManager", this);
            return;
        }

        var winner = scoreManager.GetWinner();

        if (winner == default)
        {
            Debug.LogError("GameEndState failed to get winner", this);
            return;
        }

        Debug.Log($"Game ended! Winner: {winner}");

        if (!InstanceHandler.TryGetInstance(out EndGameView endGameView))
        {
            Debug.LogError("GameEndState failed to get Instance EndGameView", this);
            return;
        }

        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.LogError("GameEndState failed to get Instance GameViewManager", this);
            return;
        }

        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>(false);

        // Only server controls state transitions
        if (!asServer) return;

        StartCoroutine(DelayedStateChange(gameViewManager, scoreManager));
    }

    private IEnumerator DelayedStateChange(GameViewManager gameViewManager, ScoreManager scoreManager)
    {
        yield return new WaitForSeconds(gameRestartDelay);
        gameViewManager.HideView<EndGameView>();

        // Reset scores for new game
        scoreManager.ResetScores();

        machine.SetState(spawningState);
    }
}
