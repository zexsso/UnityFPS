using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundEndState : StateNode
{
    [SerializeField] private int amountOfRounds = 60;
    [SerializeField] private StateNode spawningState;

    [SerializeField] private float roundRestartDelay = 1.5f;

    private int _roundCount = 0;

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer) return;

        if (_roundCount >= amountOfRounds) _roundCount = 0;
        CheckForGameEnd();
    }

    private void CheckForGameEnd()
    {
        _roundCount++;
        if (_roundCount >= amountOfRounds)
        {
            Debug.Log("Game has ended");
            machine.Next();
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
