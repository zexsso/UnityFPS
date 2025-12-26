using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class ScoreboardView : View
{
    [SerializeField] private Transform scoreboardEntriesParent;
    [SerializeField] private ScoreboardEntry scoreboardEntryPrefab;

    private GameViewManager _gameViewManager;
    private GameInput _gameInput;
    private bool _wasScoreboardHeld;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<ScoreboardView>();
    }

    private void Start()
    {
        _gameViewManager = InstanceHandler.GetInstance<GameViewManager>();
        _gameInput = GameInput.Instance;
    }

    public void SetData(Dictionary<PlayerID, ScoreManager.ScoreData> data)
    {
        foreach (Transform child in scoreboardEntriesParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var playerScore in data)
        {
            var entry = Instantiate(scoreboardEntryPrefab, scoreboardEntriesParent);
            entry.SetData(playerScore.Key.id.ToString(), playerScore.Value.kills, playerScore.Value.deaths);
        }
    }

    private void Update()
    {
        if (_gameInput == null)
        {
            _gameInput = GameInput.Instance;
            if (_gameInput == null) return;
        }

        bool isScoreboardHeld = _gameInput.ScoreboardHeld;

        // Show on press
        if (isScoreboardHeld && !_wasScoreboardHeld)
        {
            _gameViewManager.ShowView<ScoreboardView>(false);
        }
        // Hide on release
        else if (!isScoreboardHeld && _wasScoreboardHeld)
        {
            _gameViewManager.HideView<ScoreboardView>();
        }

        _wasScoreboardHeld = isScoreboardHeld;
    }

    public override void OnHide()
    {
    }

    public override void OnShow()
    {
    }
}
