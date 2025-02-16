using System;
using System.Collections.Generic;
using System.Linq;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class GameEndState : StateNode<Dictionary<PlayerID, int>>
{
    public override void Enter(Dictionary<PlayerID, int> roundWins,bool asServer)
    {
        base.Enter(asServer);

        var winner = roundWins.First();


        foreach (var player in roundWins)
        {
            if(player.Value > winner.Value) winner = player;
        }

        Debug.Log($"Game has ended with winner: {winner.Key} with {winner.Value} wins");

        roundWins.Clear();

        if (!asServer) return;


    }
}
