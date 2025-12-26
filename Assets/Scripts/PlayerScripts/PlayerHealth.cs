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
  public void ChangeHealth(int amount, RPCInfo info = default)
  {
    ChangeHealthInternal(amount, info.sender, false);
  }

  [ServerRpc(requireOwnership: false)]
  public void ChangeHealthWithHeadshot(int amount, bool isHeadshot, RPCInfo info = default)
  {
    ChangeHealthInternal(amount, info.sender, isHeadshot);
  }

  private void ChangeHealthInternal(int amount, PlayerID attackerId, bool isHeadshot)
  {
    playerHealth.value += amount;

    if (playerHealth.value <= 0)
    {
      if (InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
      {
        if (owner.HasValue)
        {
          scoreManager.AddKill(attackerId, owner.Value, isHeadshot);
          scoreManager.AddDeath(owner.Value);
        }
      }

      // Play death sound
      if (AudioManager.Instance != null)
      {
        AudioManager.Instance.PlayDeath(transform.position);
      }

      OnDeath_Server?.Invoke(owner.Value);
      Destroy(gameObject);
    }
  }

  /// <summary>
  /// Resets health to max value. Called by server when respawning.
  /// </summary>
  public void ResetHealth()
  {
    playerHealth.value = 100;
  }
}