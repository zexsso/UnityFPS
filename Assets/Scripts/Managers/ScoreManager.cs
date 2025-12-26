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

  public void AddKill(PlayerID playerId)
  {
    CheckForDictionnaryEntry(playerId);

    var myPlayer = scores[playerId];
    myPlayer.kills++;
    scores[playerId] = myPlayer;
  }

  public void AddDeath(PlayerID playerId)
  {
    CheckForDictionnaryEntry(playerId);

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

  public void CheckForDictionnaryEntry(PlayerID playerId)
  {
    if (!scores.ContainsKey(playerId)) scores.Add(playerId, new ScoreData());
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
