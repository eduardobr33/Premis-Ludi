using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource fxSource;

    [Header("Clips")]
    public AudioClip backgroundMusic;

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
}
