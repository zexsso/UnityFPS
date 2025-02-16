using PurrNet;
using UnityEngine;

public class  PistolDeagle: NetworkBehaviour
{
  [SerializeField] private Transform shootPointCamera;
  [SerializeField] private LayerMask hitlayer;
  [SerializeField] private float range = 20f;
  [SerializeField] private int damage = 10;



  protected override void OnSpawned()
  {
    base.OnSpawned();
    enabled = isOwner;
  }

  private void Update()
  {
    if (!Input.GetKeyDown(KeyCode.Mouse0)) return;

    if(!Physics.Raycast(shootPointCamera.position, shootPointCamera.forward, out var hit, range, hitlayer)) return;

    if (!hit.transform.TryGetComponent(out PlayerHealth playerHealth)) return;

    playerHealth.ChangeHealt(-damage);
    Debug.Log($"Hit: {hit.transform.name}");
    

  }
}