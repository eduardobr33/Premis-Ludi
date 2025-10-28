using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class PositiveReinforcementManager : MonoBehaviour
{
    public static PositiveReinforcementManager Instance;

    [Header("UI Elements")]
    public RectTransform characterPanel;
    public Image characterImage;
    public RectTransform bubbleSpeech;
    public TextMeshProUGUI bubbleText;

    [Header("Animation Settings")]
    public float slideInDuration = 0.5f;
    public float bubblePopDuration = 0.3f;
    public float displayDuration = 2f;
    public float slideOutDuration = 0.5f;
    public float bubbleScale = 1.2f;

    [Header("Positive Reinforcement Messages")]
    [TextArea] public string[] messages = new string[]
    {
        "¡Molt bé!",
        "¡Genial!",
        "¡Increïble!",
        "¡Segueix així!",
        "¡Perfect!",
        "¡Ho estàs fent bé!",
        "¡Fantàstic!",
        "¡Impressionant!",
        "¡Ets un campió!",
        "¡Excepcional!"
    };

    private bool isShowingMessage = false;
    private Coroutine messageCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (characterPanel != null)
        {
            float offScreenPosition = Screen.width + characterPanel.rect.width;
            characterPanel.anchoredPosition = new Vector2(offScreenPosition, characterPanel.anchoredPosition.y);
        }
    }

    public void ShowRandomReinforcement()
    {
        if (isShowingMessage)
            return;

        string randomMessage = messages[Random.Range(0, messages.Length)];
        ShowReinforcement(randomMessage);
    }

    public void ShowReinforcement(string message)
    {
        if (isShowingMessage)
            return;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowReinforcementCoroutine(message));
    }

    private IEnumerator ShowReinforcementCoroutine(string message)
    {
        isShowingMessage = true;

        if (bubbleText != null)
            bubbleText.text = message;

        // Slide in
        if (characterPanel != null)
        {
            float offScreenPosition = Screen.width + characterPanel.rect.width;
            characterPanel.DOAnchorPosX(50f, slideInDuration).SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(slideInDuration);

        // Display for 2 seconds
        yield return new WaitForSeconds(displayDuration);

        // Slide out
        if (characterPanel != null)
        {
            float offScreenPosition = Screen.width + characterPanel.rect.width;
            characterPanel.DOAnchorPosX(offScreenPosition, slideOutDuration).SetEase(Ease.InBack);
        }

        yield return new WaitForSeconds(slideOutDuration);

        isShowingMessage = false;
    }
}
