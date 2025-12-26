using System;
using PurrNet;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
  [SerializeField] private SyncDictionary<PlayerID, ScoreData> scores = new();

  public void Awake()
  {
    InstanceHandler.RegisterInstance(this);
    scores.onChanged += OnScoresChanged;
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();

    InstanceHandler.UnregisterInstance<ScoreManager>();
    scores.onChanged -= OnScoresChanged;
  }

  private void OnScoresChanged(SyncDictionaryChange<PlayerID, ScoreData> changedScores)
  {
    if (InstanceHandler.TryGetInstance(out ScoreboardView scoreboardView))
    {
      scoreboardView.SetData(scores.ToDictionary());
    }
  }

  public void AddKill(PlayerID killerId, PlayerID victimId, bool isHeadshot = false)
  {
    CheckForDictionaryEntry(killerId);

    var myPlayer = scores[killerId];
    myPlayer.kills++;
    scores[killerId] = myPlayer;

    // Broadcast kill to all clients for kill feed
    BroadcastKill(killerId.id.ToString(), victimId.id.ToString(), isHeadshot);
  }

  [ObserversRpc]
  private void BroadcastKill(string killerName, string victimName, bool isHeadshot)
  {
    if (InstanceHandler.TryGetInstance(out KillFeedView killFeed))
    {
      killFeed.AddKillEntry($"Player {killerName}", $"Player {victimName}", isHeadshot);
    }
  }

  public void AddKill(PlayerID playerId)
  {
    CheckForDictionaryEntry(playerId);

    var myPlayer = scores[playerId];
    myPlayer.kills++;
    scores[playerId] = myPlayer;
  }

  public void AddDeath(PlayerID playerId)
  {
    CheckForDictionaryEntry(playerId);

    var myPlayer = scores[playerId];
    myPlayer.deaths++;
    scores[playerId] = myPlayer;
  }

  public PlayerID GetWinner()
  {
    PlayerID winner = default;
    int highestKills = 0;

    foreach (var playerScore in scores)
    {
      if (playerScore.Value.kills > highestKills)
      {
        highestKills = playerScore.Value.kills;
        winner = playerScore.Key;
      }
    }

    return winner;
  }

  public void CheckForDictionaryEntry(PlayerID playerId)
  {
    if (!scores.ContainsKey(playerId)) scores.Add(playerId, new ScoreData());
  }

  /// <summary>
  /// Resets all player scores. Called when starting a new game.
  /// </summary>
  public void ResetScores()
  {
    scores.Clear();
  }

  public struct ScoreData
  {
    public int kills;
    public int deaths;

    public override readonly string ToString()
    {
      return $"{kills}/{deaths}";
    }
  }
}
