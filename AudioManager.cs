using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource vacuumSource;

    [Header("Audio Clips")]
    public AudioClip menuMusicClip;
    public AudioClip gameMusicClip;
    public AudioClip vacuumClip;
    public AudioClip hitClip;
    public AudioClip ghostDieClip;
    public AudioClip clickClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManager: Создан и сохранён между сценами");
        }
        else
        {
            Debug.LogWarning("AudioManager: Дубликат удалён!");
            Destroy(gameObject);
            return;
        }

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (vacuumSource == null) vacuumSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.5f;
        musicSource.spatialBlend = 0f;
        musicSource.priority = 0;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = 1f;
        sfxSource.spatialBlend = 0f;
        sfxSource.priority = 128;
        
        vacuumSource.playOnAwake = false;
        vacuumSource.loop = true;
        vacuumSource.volume = 0.8f;
        vacuumSource.spatialBlend = 0f;

        Debug.Log("AudioManager: menuMusicClip = " + (menuMusicClip != null ? "НАЗНАЧЕН" : "ПУСТОЙ"));
        Debug.Log("AudioManager: gameMusicClip = " + (gameMusicClip != null ? "НАЗНАЧЕН" : "ПУСТОЙ"));
    }

    public void PlayMenuMusic()
    {
        if (Instance == null)
        {
            Debug.LogError("AudioManager: Instance = NULL!");
            return;
        }

        musicSource.Stop();

        if (menuMusicClip != null)
        {
            musicSource.clip = menuMusicClip;
            musicSource.Play();
            Debug.Log("AudioManager: Играет музыка МЕНЮ");
        }
        else if (gameMusicClip != null)
        {
            musicSource.clip = gameMusicClip;
            musicSource.Play();
            Debug.Log("AudioManager: Играет музыка ИГРЫ (запасной вариант)");
        }
        else
        {
            Debug.LogError("AudioManager: Нет клипов для музыки!");
        }
    }

    public void PlayGameMusic()
    {
        if (Instance == null)
        {
            Debug.LogError("AudioManager: Instance = NULL!");
            return;
        }

        musicSource.Stop();

        if (gameMusicClip != null)
        {
            musicSource.clip = gameMusicClip;
            musicSource.Play();
            Debug.Log("AudioManager: Играет музыка ИГРЫ");
        }
        else
        {
            Debug.LogError("AudioManager: gameMusicClip не назначен!");
        }
    }

    public void StopAllMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            Debug.Log("AudioManager: Вся музыка остановлена");
        }
    }

    public void StartVacuumSound()
    {
        if (vacuumClip != null && vacuumSource != null)
        {
            vacuumSource.clip = vacuumClip;
            vacuumSource.Play();
        }
    }

    public void StopVacuumSound()
    {
        if (vacuumSource != null)
        {
            vacuumSource.Stop();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayHitSound() => PlaySound(hitClip);
    public void PlayGhostDieSound() => PlaySound(ghostDieClip);
    public void PlayClickSound() => PlaySound(clickClip);
}