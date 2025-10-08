using UnityEngine;
using TMPro;
using System.Collections;

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

    private void Awake()
    {
        Instance = this;
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
}
