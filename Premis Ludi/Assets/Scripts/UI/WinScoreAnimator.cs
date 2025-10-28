using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class WinScoreAnimator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 3f;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip scoreLoopSound;
    [SerializeField] public AudioSource scoreAudioSource;
    [SerializeField] private float basePitch = 0.8f;
    [SerializeField] private float maxPitch = 1.5f;

    private int finalScore = 0;
    private int earnedStars = 0;
    private WinStarsAnimator starsAnimator;

    private void Start()
    {
        starsAnimator = FindObjectOfType<WinStarsAnimator>();
        CreateAudioSourceIfNeeded();
        GetScoreFromGameManager();
        CalculateEarnedStars();
        InitializeUI();
        StartScoreAnimation();
    }

    private void CreateAudioSourceIfNeeded()
    {
        if (scoreAudioSource == null)
        {
            scoreAudioSource = GetComponent<AudioSource>();
            if (scoreAudioSource == null)
                return;
        }
        
        scoreAudioSource.playOnAwake = false;
        scoreAudioSource.loop = true;
    }

    private void GetScoreFromGameManager()
    {
        if (PlayerPrefs.HasKey("CurrentLevelScore"))
        {
            finalScore = PlayerPrefs.GetInt("CurrentLevelScore");
        }
        else if (GameManager.Instance != null)
        {
            finalScore = GameManager.Instance.score;
        }
        else
        {
            finalScore = 50;
        }
    }

    private void CalculateEarnedStars()
    {
        if (finalScore >= 100)
            earnedStars = 3;
        else if (finalScore >= 50)
            earnedStars = 2;
        else
            earnedStars = 1;
    }

    private void InitializeUI()
    {
        if (scoreSlider != null)
        {
            scoreSlider.maxValue = 100f;
            scoreSlider.value = 0f;
        }

        if (scoreText != null)
            scoreText.text = "0";
    }

    private void StartScoreAnimation()
    {
        StartCoroutine(DelayedScoreAnimation());
    }

    private IEnumerator DelayedScoreAnimation()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(AnimateScore());
    }

    private IEnumerator AnimateScore()
    {
        if (scoreAudioSource != null && scoreLoopSound != null)
        {
            scoreAudioSource.clip = scoreLoopSound;
            scoreAudioSource.Play();
        }
        
        float elapsed = 0f;
        int displayedScore = 0;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;

            displayedScore = Mathf.RoundToInt(finalScore * progress);

            if (scoreText != null)
            {
                scoreText.text = displayedScore.ToString();
                scoreText.transform.localScale = Vector3.one + Vector3.one * 0.1f;
                scoreText.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack);
            }

            if (scoreSlider != null)
            {
                float sliderValue = Mathf.Min(finalScore * progress, 100f);
                scoreSlider.value = sliderValue;
            }

            if (scoreAudioSource != null)
                scoreAudioSource.pitch = Mathf.Lerp(basePitch, maxPitch, progress);

            CheckAndTriggerStars(progress);

            yield return null;
        }

        displayedScore = finalScore;
        if (scoreText != null)
        {
            scoreText.text = finalScore.ToString();
            scoreText.transform.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => scoreText.transform.DOScale(Vector3.one, 0.2f));
        }

        if (scoreSlider != null)
            scoreSlider.value = Mathf.Min(finalScore, 100f);

        if (scoreAudioSource != null)
            scoreAudioSource.Stop();
    }

    private void CheckAndTriggerStars(float progress)
    {
        if (starsAnimator == null)
            return;

        float sliderProgress = Mathf.Min(finalScore * progress, 60f) / 60f;

        if (earnedStars >= 1 && sliderProgress >= 0.33f && !starsAnimator.star1Triggered)
            starsAnimator.TriggerStar(0);

        if (earnedStars >= 2 && sliderProgress >= 0.66f && !starsAnimator.star2Triggered)
            starsAnimator.TriggerStar(1);

        if (earnedStars >= 3 && sliderProgress >= 1f && !starsAnimator.star3Triggered)
            starsAnimator.TriggerStar(2);
    }
}
