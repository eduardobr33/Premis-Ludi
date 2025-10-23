using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Texts")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("Popups")]
    public GameObject pointPopupPrefab;
    public RectTransform scoreTarget;

    [Header("Panels")]
    public GameObject multiplicationTablesPanel;

    [Header("Boss Health")]
    public GameObject bossHealthBar;
    public Image bossHealthFill;

    private void Awake()
    {
        Instance = this;

        bossHealthBar.SetActive(false);
    }

    public void UpdateTimer(float time)
    {
        timerText.text = Mathf.CeilToInt(time).ToString();
    }

    public void UpdateScore(int newScore)
    {
        StartCoroutine(ScoreChangeAnimation(newScore));
    }

    public void ShowPointPopup(int points, Vector3 worldPos, System.Action onComplete = null)
    {
        if (!pointPopupPrefab || !scoreTarget) return;

        GameObject popupObj = Instantiate(pointPopupPrefab, scoreTarget.parent);
        var popup = popupObj.GetComponent<PointPopup>();
        popup.Initialize(points, worldPos, scoreTarget, onComplete);
    }

    public void ToggleMultiplicationPanel(bool state)
    {
        multiplicationTablesPanel?.SetActive(state);
    }

    private IEnumerator ScoreChangeAnimation(int newScore)
    {
        Vector3 originalScale = scoreText.rectTransform.localScale;

        // Stretch
        float t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.3f, t / 0.1f);
            scoreText.rectTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // Update score
        scoreText.text = newScore.ToString();

        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1.3f, 0.85f, t / 0.1f);
            scoreText.rectTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // Returns to its size
        t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(0.85f, 1f, t / 0.15f);
            scoreText.rectTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        scoreText.rectTransform.localScale = originalScale;
    }

    public void ShowBossHealthBar(bool show)
    {
        if (bossHealthBar != null) bossHealthBar.SetActive(show);
    }

    public void UpdateBossHealth(float current, float max)
    {
        if (bossHealthFill == null) return;

        float fillAmount = Mathf.Clamp01(current / max);
        bossHealthFill.fillAmount = fillAmount;
    }
}
