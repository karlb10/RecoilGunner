using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource chargeSoundSource; // Separate source for looping charge sound

    [Header("Music")]
    public AudioClip backgroundMusic;
    public float musicVolume = 0.5f;

    [Header("Sound Effects")]
    public AudioClip chargeSound;
    public AudioClip fireSound;
    public AudioClip playerDamageSound;
    public AudioClip playerDeathSound;
    public AudioClip enemyDeathSound;
    public AudioClip buttonClickSound;

    [Header("Volume Settings")]
    public float sfxVolume = 0.7f;

    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ AudioManager created and set to persist");
        }
        else if (Instance != this)
        {
            Debug.Log("⚠️ Destroying duplicate AudioManager");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Setup music source
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        // Setup SFX source
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }

        // Setup charge sound source for looping
        if (chargeSoundSource == null)
        {
            chargeSoundSource = gameObject.AddComponent<AudioSource>();
            chargeSoundSource.loop = true;
        }

        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("⚠️ Background music not assigned!");
            return;
        }

        if (musicSource.isPlaying && musicSource.clip == backgroundMusic)
            return; // Already playing

        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.Play();

        Debug.Log("🎵 Background music started");
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void StartChargeSound()
    {
        if (chargeSound == null)
        {
            Debug.LogWarning("⚠️ Charge sound not assigned!");
            return;
        }

        if (chargeSoundSource == null)
        {
            Debug.LogError("❌ Charge AudioSource is null!");
            return;
        }

        if (chargeSoundSource.isPlaying && chargeSoundSource.clip == chargeSound)
            return; // Already playing

        chargeSoundSource.clip = chargeSound;
        chargeSoundSource.volume = sfxVolume;
        chargeSoundSource.loop = true;
        chargeSoundSource.Play();

        Debug.Log("⚡ Charge sound started");
    }

    public void StopChargeSound()
    {
        if (chargeSoundSource != null && chargeSoundSource.isPlaying)
        {
            chargeSoundSource.Stop();
            Debug.Log("⚡ Charge sound stopped");
        }
    }

    public void StopAllAudio()
    {
        StopChargeSound();
        if (sfxSource != null && sfxSource.isPlaying)
        {
            sfxSource.Stop();
        }
    }

    public void PlayFireSound()
    {
        PlaySFX(fireSound, "Fire");
    }

    public void PlayPlayerDamageSound()
    {
        PlaySFX(playerDamageSound, "Player Damage");
    }

    public void PlayPlayerDeathSound()
    {
        PlaySFX(playerDeathSound, "Player Death");
    }

    public void PlayEnemyDeathSound()
    {
        PlaySFX(enemyDeathSound, "Enemy Death");
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(buttonClickSound, "Button Click");
    }

    private void PlaySFX(AudioClip clip, string soundName)
    {
        if (clip == null)
        {
            Debug.LogWarning($"⚠️ {soundName} sound not assigned!");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError("❌ SFX AudioSource is null!");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (chargeSoundSource != null)
            chargeSoundSource.volume = sfxVolume;
    }
}