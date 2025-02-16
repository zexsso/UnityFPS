using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundEndState : StateNode<PlayerID>
{
    [SerializeField] private int amountOfRounds = 3;
    [SerializeField] private StateNode spawningState;

    [SerializeField] private float roundRestartDelay = 3f;

    private int _roundCount = 0;

    private Dictionary<PlayerID, int> _roundWins = new();

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer) return;

       Debug.Log("Round Has Ended with no winners");

       CheckForGameEnd();
    }

    public override void Enter(PlayerID roundWinner, bool asServer)
    {
        base.Enter(asServer);

        if (!asServer) return;

        if (!_roundWins.ContainsKey(roundWinner))_roundWins.Add(roundWinner, 1);
        else _roundWins[roundWinner]++; 

        Debug.Log($"Round winner: {roundWinner} wins: {_roundWins[roundWinner]}");

        CheckForGameEnd();
    }

    private void CheckForGameEnd()
    {
        _roundCount ++;
        if (_roundCount >= amountOfRounds) {
            Debug.Log("Game has ended");
            machine.Next(_roundWins);
            return;
        }
        StartCoroutine(DelayedStateChange());
    }

    private IEnumerator DelayedStateChange()
    {
        yield return new WaitForSeconds(roundRestartDelay);
        machine.SetState(spawningState);
    }
}
