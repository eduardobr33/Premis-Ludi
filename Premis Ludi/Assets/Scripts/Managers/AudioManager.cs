using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource fxSource;

    [Header("Clips")]
    public AudioClip backgroundMusic;
    public AudioClip battleMusic;
    public AudioClip[] enemyDamageSounds;
    public AudioClip playerDamageSound;
    public AudioClip buttonClickSound;

    [Header("Pitch Modulation")]
    public float enemyDamagePitch = 1f;
    public float playerDamagePitch = 1f;
    public float buttonClickPitch = 1f;

    private bool isMuted = false;
    private const string MUTE_PREF_KEY = "Muted";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (fxSource == null)
            fxSource = gameObject.AddComponent<AudioSource>();

        isMuted = PlayerPrefs.GetInt(MUTE_PREF_KEY, 0) == 1;
        ApplyMuteState();

        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayFX(AudioClip clip)
    {
        if (clip == null || isMuted) return;
        fxSource.PlayOneShot(clip);
    }

    public void PlayEnemyDamageSound()
    {
        if (enemyDamageSounds != null && enemyDamageSounds.Length > 0 && !isMuted)
        {
            AudioClip randomClip = enemyDamageSounds[Random.Range(0, enemyDamageSounds.Length)];
            fxSource.PlayOneShot(randomClip, 1f);
        }
    }

    public void PlayPlayerDamageSound()
    {
        if (playerDamageSound != null && !isMuted)
            fxSource.PlayOneShot(playerDamageSound, 1f);
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null && !isMuted)
            fxSource.PlayOneShot(buttonClickSound, 1f);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt(MUTE_PREF_KEY, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMuteState();
    }

    private void ApplyMuteState()
    {
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    public void PlayBattleMusic()
    {
        if (battleMusic != null)
        {
            musicSource.Stop();
            musicSource.clip = battleMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            musicSource.Stop();
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
