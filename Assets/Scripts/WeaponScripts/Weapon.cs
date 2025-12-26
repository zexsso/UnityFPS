using System.Collections;
using PurrNet;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private int hsDamage = 100;

    [SerializeField] private float fireDelay = 0.6f;
    [SerializeField] private bool automatic;

    [Header("Weapon Recoil")]
    [SerializeField] private float recoilStrenght = 0.1f;
    [SerializeField] private float recoilDuration = 0.2f;
    [SerializeField] private AnimationCurve recoilCurve;
    [SerializeField] private float rotationAmout = 30f;
    [SerializeField] private AnimationCurve rotationCurve;


    [Header("References")]
    [SerializeField] private Transform shootPointCamera;
    [SerializeField] private LayerMask hitlayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Transform rightHandTarget, leftHandTarget;
    [SerializeField] private Transform rightIKTarget, leftIKTarget;
    [SerializeField] private ParticleSystem environementHitEffect, playerHitEffect;


    private float _lastFireTime;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Coroutine _recoilCoroutine;

    private void Start()
    {
        _originalPosition = transform.localPosition;
        _originalRotation = transform.localRotation;
    }



    private void Update()
    {

        SetIkTargets();

        if (!isOwner) return;

        if (automatic && !Input.GetKey(KeyCode.Mouse0) || !automatic && !Input.GetKeyDown(KeyCode.Mouse0)) return;

        if (Time.unscaledTime < _lastFireTime + fireDelay && _lastFireTime != 0) return;
        _lastFireTime = Time.unscaledTime;

        PlayShotEffect();


        if (!Physics.Raycast(shootPointCamera.position, shootPointCamera.forward, out var hit, range, hitlayer)) return;

        Debug.Log($"Hit: {hit.collider}");

        if (!hit.transform.TryGetComponent(out PlayerHealth playerHealth))
        {
            EnvironementHit(hit.point, hit.normal);
            return;
        }



        PlayerHit(playerHealth, playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);

        // int appliedDamage = 0; // TODO: change to damage
        if (hit.collider.CompareTag("Head"))
        {
            Debug.Log("Headshot détecté !");
            playerHealth.ChangeHealt(-hsDamage);
        }
        Debug.Log($"Hit: {hit.transform.name}, {hitlayer.value}");
    }

    [ObserversRpc(runLocally: true)]
    private void PlayerHit(PlayerHealth player, Vector3 localPosition, Vector3 normal)
    {
        if (playerHitEffect && player) Instantiate(playerHitEffect, player.transform.TransformPoint(localPosition), Quaternion.LookRotation(normal));
    }


    [ObserversRpc(runLocally: true)]
    private void EnvironementHit(Vector3 position, Vector3 normal)
    {
        if (environementHitEffect) Instantiate(environementHitEffect, position, Quaternion.LookRotation(normal));
    }

    private void SetIkTargets()
    {
        rightIKTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
        leftIKTarget.SetPositionAndRotation(leftHandTarget.position, leftHandTarget.rotation);
    }

    [ObserversRpc(runLocally: true)]
    private void PlayShotEffect()
    {
        if (muzzleFlash) muzzleFlash.Play();

        if (_recoilCoroutine != null) StopCoroutine(_recoilCoroutine);

        _recoilCoroutine = StartCoroutine(PlayRecoilEffect());
    }


    private IEnumerator PlayRecoilEffect()
    {
        float elapsedTime = 0f;

        while (elapsedTime < recoilDuration)
        {
            elapsedTime += Time.deltaTime;
            float curveTime = elapsedTime / recoilDuration;

            // Recoil Position
            float recoilValue = recoilCurve.Evaluate(curveTime);
            Vector3 recoilOffset = Vector3.back * (recoilValue * recoilStrenght);
            transform.localPosition = _originalPosition + recoilOffset;

            // Recoil Rotation
            float rotationValue = rotationCurve.Evaluate(curveTime);
            Vector3 rotationOffset = Vector3.forward * (rotationValue * -rotationAmout);
            // Vector3 rotationOffset = Vector3(0f, 0f, rotationValue * -rotationAmout);
            transform.localRotation = _originalRotation * Quaternion.Euler(rotationOffset);

            yield return null;
        }

        transform.SetLocalPositionAndRotation(_originalPosition, _originalRotation);
    }
}
