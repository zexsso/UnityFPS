using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundRunningState : StateNode<List<PlayerHealth>>
{
    [Header("Round Settings")]
    [SerializeField] private float roundDuration = 180f; // 3 minutes per round
    [SerializeField] private SyncVar<float> remainingTime = new(0f);

    private List<PlayerID> _players = new();
    private List<PlayerHealth> _subscribedPlayers = new();
    private Coroutine _timerCoroutine;

    public float RemainingTime => remainingTime.value;

    public override void Enter(List<PlayerHealth> data, bool asServer)
    {
        base.Enter(data, asServer);

        // Subscribe to timer changes for client UI updates
        remainingTime.onChanged += OnTimerChanged;

        // Update UI for all clients
        if (InstanceHandler.TryGetInstance(out MainGameView mainGameView))
        {
            mainGameView.ShowRoundTimer(true);
            mainGameView.UpdateRoundTimer(remainingTime.value);
        }

        // Play round start sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRoundStart();
        }

        if (!asServer) return;

        _players.Clear();
        _subscribedPlayers.Clear();

        foreach (var player in data)
        {
            if (player.owner.HasValue) _players.Add(player.owner.Value);
            player.OnDeath_Server += OnPlayerDeath;
            _subscribedPlayers.Add(player);
        }

        // Start round timer
        remainingTime.value = roundDuration;
        _timerCoroutine = StartCoroutine(RoundTimerCoroutine());
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);

        // Unsubscribe from timer changes
        remainingTime.onChanged -= OnTimerChanged;

        // Hide timer UI
        if (InstanceHandler.TryGetInstance(out MainGameView mainGameView))
        {
            mainGameView.ShowRoundTimer(false);
        }

        if (!asServer) return;

        // Stop timer
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        // Unsubscribe from all death events to prevent memory leaks
        foreach (var player in _subscribedPlayers)
        {
            if (player != null)
            {
                player.OnDeath_Server -= OnPlayerDeath;
            }
        }
        _subscribedPlayers.Clear();
    }

    private IEnumerator RoundTimerCoroutine()
    {
        while (remainingTime.value > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime.value -= 1f;

            // Update UI
            UpdateTimerUI();
        }

        // Time ran out - end round
        machine.Next();
    }

    private void UpdateTimerUI()
    {
        if (InstanceHandler.TryGetInstance(out MainGameView mainGameView))
        {
            mainGameView.UpdateRoundTimer(remainingTime.value);
        }
    }

    private void OnTimerChanged(float newTime)
    {
        // Update UI when timer syncs to clients
        UpdateTimerUI();
    }

    private void OnPlayerDeath(PlayerID deadPlayer)
    {
        _players.Remove(deadPlayer);

        if (_players.Count <= 1) machine.Next();
    }
}
