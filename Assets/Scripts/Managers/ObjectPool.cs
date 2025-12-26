using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for efficient object reuse.
/// Reduces garbage collection by recycling frequently spawned objects.
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _pool = new();
    private readonly int _initialSize;

    public ObjectPool(T prefab, int initialSize = 10, Transform parent = null)
    {
        _prefab = prefab;
        _initialSize = initialSize;
        _parent = parent;

        // Pre-populate the pool
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewInstance();
        }
    }

    private T CreateNewInstance()
    {
        T instance = Object.Instantiate(_prefab, _parent);
        instance.gameObject.SetActive(false);
        _pool.Enqueue(instance);
        return instance;
    }

    public T Get()
    {
        T instance;

        if (_pool.Count > 0)
        {
            instance = _pool.Dequeue();
        }
        else
        {
            instance = Object.Instantiate(_prefab, _parent);
        }

        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Return(T instance)
    {
        if (instance == null) return;

        instance.gameObject.SetActive(false);
        _pool.Enqueue(instance);
    }

    public void Clear()
    {
        while (_pool.Count > 0)
        {
            var instance = _pool.Dequeue();
            if (instance != null)
            {
                Object.Destroy(instance.gameObject);
            }
        }
    }
}

/// <summary>
/// Manages object pools for particle effects.
/// Provides centralized access to effect pools.
/// </summary>
public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance { get; private set; }

    [Header("Effect Prefabs")]
    [SerializeField] private ParticleSystem environmentHitEffectPrefab;
    [SerializeField] private ParticleSystem playerHitEffectPrefab;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 20;

    private ObjectPool<ParticleSystem> _environmentHitPool;
    private ObjectPool<ParticleSystem> _playerHitPool;
    private ObjectPool<ParticleSystem> _muzzleFlashPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializePools();
    }

    private void InitializePools()
    {
        if (environmentHitEffectPrefab != null)
            _environmentHitPool = new ObjectPool<ParticleSystem>(environmentHitEffectPrefab, poolSize, transform);

        if (playerHitEffectPrefab != null)
            _playerHitPool = new ObjectPool<ParticleSystem>(playerHitEffectPrefab, poolSize, transform);

        if (muzzleFlashPrefab != null)
            _muzzleFlashPool = new ObjectPool<ParticleSystem>(muzzleFlashPrefab, poolSize, transform);
    }

    public ParticleSystem GetEnvironmentHitEffect(Vector3 position, Quaternion rotation)
    {
        if (_environmentHitPool == null) return null;

        var effect = _environmentHitPool.Get();
        effect.transform.SetPositionAndRotation(position, rotation);
        effect.Play();

        // Auto-return after effect duration
        StartCoroutine(ReturnAfterDelay(effect, _environmentHitPool, effect.main.duration + effect.main.startLifetime.constantMax));

        return effect;
    }

    public ParticleSystem GetPlayerHitEffect(Vector3 position, Quaternion rotation)
    {
        if (_playerHitPool == null) return null;

        var effect = _playerHitPool.Get();
        effect.transform.SetPositionAndRotation(position, rotation);
        effect.Play();

        // Auto-return after effect duration
        StartCoroutine(ReturnAfterDelay(effect, _playerHitPool, effect.main.duration + effect.main.startLifetime.constantMax));

        return effect;
    }

    private System.Collections.IEnumerator ReturnAfterDelay(ParticleSystem effect, ObjectPool<ParticleSystem> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Return(effect);
    }

    private void OnDestroy()
    {
        _environmentHitPool?.Clear();
        _playerHitPool?.Clear();
        _muzzleFlashPool?.Clear();
    }
}
