using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// Centralized audio manager for playing sound effects and music.
/// Supports 3D spatial audio for world sounds.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource uiSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip weaponFireClip;
    [SerializeField] private AudioClip weaponReloadClip;
    [SerializeField] private AudioClip hitMarkerClip;
    [SerializeField] private AudioClip headshotClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip roundStartClip;
    [SerializeField] private AudioClip roundEndClip;
    [SerializeField] private AudioClip gameEndClip;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip menuOpenClip;
    [SerializeField] private AudioClip menuCloseClip;

    [Header("Settings")]
    [SerializeField] private int maxConcurrentSounds = 16;
    [SerializeField] private float defaultSpatialBlend = 1f;

    // Pool of audio sources for 3D sounds
    private Queue<AudioSource> _audioSourcePool = new();
    private List<AudioSource> _activeAudioSources = new();

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    private float _masterVolume = 1f;
    private float _sfxVolume = 1f;
    private float _musicVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioPool();
        LoadVolumeSettings();
    }

    private void InitializeAudioPool()
    {
        for (int i = 0; i < maxConcurrentSounds; i++)
        {
            CreatePooledAudioSource();
        }
    }

    private AudioSource CreatePooledAudioSource()
    {
        GameObject audioObj = new GameObject($"PooledAudioSource_{_audioSourcePool.Count}");
        audioObj.transform.SetParent(transform);

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = defaultSpatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = 50f;

        audioObj.SetActive(false);
        _audioSourcePool.Enqueue(source);

        return source;
    }

    private void LoadVolumeSettings()
    {
        _masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        _sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        _musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);

        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        if (musicSource != null)
            musicSource.volume = _masterVolume * _musicVolume;

        if (uiSource != null)
            uiSource.volume = _masterVolume * _sfxVolume;
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, _masterVolume);
        ApplyVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _sfxVolume);
        ApplyVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, _musicVolume);
        ApplyVolumeSettings();
    }

    /// <summary>
    /// Plays a 3D sound at a specific world position
    /// </summary>
    public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableAudioSource();
        if (source == null) return;

        source.transform.position = position;
        source.clip = clip;
        source.volume = _masterVolume * _sfxVolume * volumeMultiplier;
        source.spatialBlend = defaultSpatialBlend;
        source.Play();

        _activeAudioSources.Add(source);
        StartCoroutine(ReturnToPoolAfterPlay(source, clip.length));
    }

    /// <summary>
    /// Plays a 2D UI sound
    /// </summary>
    public void PlayUISound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || uiSource == null) return;

        uiSource.PlayOneShot(clip, _masterVolume * _sfxVolume * volumeMultiplier);
    }

    private AudioSource GetAvailableAudioSource()
    {
        // Clean up finished sources
        _activeAudioSources.RemoveAll(s => s == null || !s.isPlaying);

        if (_audioSourcePool.Count > 0)
        {
            AudioSource source = _audioSourcePool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }

        // If pool is empty and we haven't exceeded max, create new
        if (_activeAudioSources.Count < maxConcurrentSounds)
        {
            AudioSource source = CreatePooledAudioSource();
            _audioSourcePool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }

        return null;
    }

    private System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);

        if (source != null)
        {
            source.Stop();
            source.gameObject.SetActive(false);
            _activeAudioSources.Remove(source);
            _audioSourcePool.Enqueue(source);
        }
    }

    // Convenience methods for common sounds
    public void PlayWeaponFire(Vector3 position) => PlaySoundAtPosition(weaponFireClip, position);
    public void PlayWeaponReload(Vector3 position) => PlaySoundAtPosition(weaponReloadClip, position);
    public void PlayHitMarker() => PlayUISound(hitMarkerClip);
    public void PlayHeadshot() => PlayUISound(headshotClip);
    public void PlayDeath(Vector3 position) => PlaySoundAtPosition(deathClip, position);
    public void PlayJump(Vector3 position) => PlaySoundAtPosition(jumpClip, position, 0.5f);
    public void PlayFootstep(Vector3 position) => PlaySoundAtPosition(footstepClip, position, 0.3f);
    public void PlayRoundStart() => PlayUISound(roundStartClip);
    public void PlayRoundEnd() => PlayUISound(roundEndClip);
    public void PlayGameEnd() => PlayUISound(gameEndClip);
    public void PlayButtonClick() => PlayUISound(buttonClickClip, 0.5f);
    public void PlayMenuOpen() => PlayUISound(menuOpenClip);
    public void PlayMenuClose() => PlayUISound(menuCloseClip);
}
