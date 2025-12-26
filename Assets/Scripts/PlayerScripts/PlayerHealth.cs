using System;
using PurrNet;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{

  [SerializeField] private int selfLayer, otherLayer;
  [SerializeField] private SyncVar<int> playerHealth = new(100);

  public int Health => playerHealth;

  public Action<PlayerID> OnDeath_Server;

  protected override void OnSpawned()
  {
    base.OnSpawned();

    var actualLayer = isOwner ? selfLayer : otherLayer;
    SetLayerRecursive(gameObject, actualLayer);

    if (isOwner)
    {
      Debug.Log($"PlayerHealth: OnSpawned {playerHealth.value}");
      InstanceHandler.GetInstance<MainGameView>().UpdateHealth(playerHealth.value);
      playerHealth.onChanged += OnHealthChanged;
    }
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();

    playerHealth.onChanged -= OnHealthChanged;
  }

  private void OnHealthChanged(int newHealth)
  {
    InstanceHandler.GetInstance<MainGameView>().UpdateHealth(newHealth);
  }

  private void SetLayerRecursive(GameObject obj, int targetLayer)
  {
    obj.layer = targetLayer;

    foreach (Transform child in obj.transform)
    {
      SetLayerRecursive(child.gameObject, targetLayer);
    }
  }

  [ServerRpc(requireOwnership: false)]
  public void ChangeHealt(int amount, RPCInfo info = default)
  {
    playerHealth.value += amount;

    if (playerHealth.value <= 0)
    {
      if (InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
      {
        scoreManager.AddKill(info.sender);
        if (owner.HasValue) scoreManager.AddDeath(owner.Value);
      }
      OnDeath_Server?.Invoke(owner.Value);
      Destroy(gameObject);
    }
  }
}