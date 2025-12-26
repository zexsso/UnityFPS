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
    private GameInput _gameInput;

    private void Start()
    {
        _originalPosition = transform.localPosition;
        _originalRotation = transform.localRotation;
        _gameInput = GameInput.Instance;
    }

    private void Update()
    {
        SetIkTargets();

        if (!isOwner) return;

        if (_gameInput == null)
        {
            _gameInput = GameInput.Instance;
            if (_gameInput == null) return;
        }

        // Check fire input based on weapon type
        bool shouldFire = automatic ? _gameInput.AttackHeld : _gameInput.AttackPressed;
        if (!shouldFire) return;

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

        // Apply damage - headshots deal more damage
        bool isHeadshot = hit.collider.CompareTag("Head");
        int appliedDamage = isHeadshot ? -hsDamage : -damage;
        playerHealth.ChangeHealthWithHeadshot(appliedDamage, isHeadshot);

        // Play hit marker sound (only for local player who fired)
        if (AudioManager.Instance != null)
        {
            if (isHeadshot)
            {
                AudioManager.Instance.PlayHeadshot();
            }
            else
            {
                AudioManager.Instance.PlayHitMarker();
            }
        }
    }

    [ObserversRpc(runLocally: true)]
    private void PlayerHit(PlayerHealth player, Vector3 localPosition, Vector3 normal)
    {
        if (player == null) return;

        Vector3 worldPosition = player.transform.TransformPoint(localPosition);
        Quaternion rotation = Quaternion.LookRotation(normal);

        // Try to use object pool, fallback to instantiate
        if (EffectPoolManager.Instance != null)
        {
            EffectPoolManager.Instance.GetPlayerHitEffect(worldPosition, rotation);
        }
        else if (playerHitEffect != null)
        {
            Instantiate(playerHitEffect, worldPosition, rotation);
        }
    }

    [ObserversRpc(runLocally: true)]
    private void EnvironementHit(Vector3 position, Vector3 normal)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);

        // Try to use object pool, fallback to instantiate
        if (EffectPoolManager.Instance != null)
        {
            EffectPoolManager.Instance.GetEnvironmentHitEffect(position, rotation);
        }
        else if (environementHitEffect != null)
        {
            Instantiate(environementHitEffect, position, rotation);
        }
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

        // Play weapon fire sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponFire(transform.position);
        }

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
