using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections;

public class PowerupUnlockScreen : MonoBehaviour
{
    public static PowerupUnlockScreen Instance;

    [Header("UI Elements")]
    public Canvas unlockCanvas;
    public RectTransform characterPanel;
    public Image characterImage;
    public RectTransform speechBubble;
    public TextMeshProUGUI dialogueText;
    
    [Header("Animation Settings")]
    public float slideInDuration = 0.5f;
    public float bubblePopDuration = 0.3f;
    public float bubbleScale = 2.7f;
    public float typewriterSpeed = 0.05f;
    
    [Header("Powerup Icon References")]
    public Image skipIcon;
    public Image doublePointsIcon;
    public Image slowMotionIcon;
    public Image multiplicationTablesIcon;
    
    [Header("Icon Animation Settings")]
    public float iconShakeDuration = 0.8f;
    public float iconShakeStrength = 60f;
    public Color unlockColor = Color.yellow;
    public float colorFlashDuration = 0.3f;
    public int colorFlashCount = 3;
    
    [Header("Powerup Messages")]
    [TextArea] public string skipMessage = "¡Nuevo poder desbloqueado!\n\nSALTO: Elimina al enemigo actual sin ganar puntos. Úsalo sabiamente.";
    [TextArea] public string doublePointsMessage = "¡Nuevo poder desbloqueado!\n\nDOBLE PUNTOS: El siguiente enemigo que derrotes dará el doble de puntos.";
    [TextArea] public string slowMotionMessage = "¡Nuevo poder desbloqueado!\n\nCÁMARA LENTA: Ralentiza el tiempo durante 5 segundos para facilitar el dibujo.";
    [TextArea] public string multiplicationTablesMessage = "¡Nuevo poder desbloqueado!\n\nTABLAS: Consulta las tablas de multiplicar en cualquier momento.";

    private bool isShowingDialogue = false;
    private bool dialogueComplete = false;
    private string currentMessage = "";
    private Coroutine typewriterCoroutine;
    private Action onUnlockComplete;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        unlockCanvas.gameObject.SetActive(false);
        
        float offScreenPosition = Screen.width + characterPanel.rect.width;
        characterPanel.anchoredPosition = new Vector2(offScreenPosition, characterPanel.anchoredPosition.y);
        speechBubble.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (isShowingDialogue && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            if (!dialogueComplete)
            {
                CompleteDialogue();
            }
            else
            {
                HideUnlockScreen();
            }
        }
    }

    public void ShowPowerupUnlock(PowerupType powerupType, Action onComplete)
    {
        onUnlockComplete = onComplete;
        unlockCanvas.gameObject.SetActive(true);

        Image targetIcon = GetPowerupIcon(powerupType);
        string message = GetPowerupMessage(powerupType);

        if (targetIcon != null)
        {
            AnimatePowerupIcon(targetIcon, () => ShowCharacterWithMessage(message));
        }
        else
        {
            ShowCharacterWithMessage(message);
        }
    }

    private Image GetPowerupIcon(PowerupType powerupType)
    {
        return powerupType switch
        {
            PowerupType.Skip => skipIcon,
            PowerupType.DoublePoints => doublePointsIcon,
            PowerupType.SlowMotion => slowMotionIcon,
            PowerupType.MultiplicationTables => multiplicationTablesIcon,
            _ => null
        };
    }

    private string GetPowerupMessage(PowerupType powerupType)
    {
        return powerupType switch
        {
            PowerupType.Skip => skipMessage,
            PowerupType.DoublePoints => doublePointsMessage,
            PowerupType.SlowMotion => slowMotionMessage,
            PowerupType.MultiplicationTables => multiplicationTablesMessage,
            _ => "¡Nuevo poder desbloqueado!"
        };
    }

    private void AnimatePowerupIcon(Image icon, Action onComplete)
    {
        Vector3 originalScale = icon.transform.localScale;
        Vector3 originalPosition = icon.transform.position;

        Sequence iconSequence = DOTween.Sequence();

        iconSequence.Append(icon.transform.DOMove(new Vector3(Screen.width / 2f, Screen.height / 2f, 0), 0.5f).SetEase(Ease.OutBack));
        iconSequence.Append(icon.transform.DOShakePosition(0.4f, 15f, 8, 90, false, true));
        iconSequence.Join(icon.transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 4, 0.5f));
        iconSequence.Append(icon.transform.DOMove(originalPosition, 0.5f).SetEase(Ease.InBack));
        
        iconSequence.OnComplete(() =>
        {
            icon.transform.position = originalPosition;
            icon.transform.localScale = originalScale;
            onComplete?.Invoke();
        });
    }

    private void ShowCharacterWithMessage(string message)
    {
        isShowingDialogue = true;
        dialogueComplete = false;

        characterPanel.DOAnchorPosX(0, slideInDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            ShowSpeechBubble(message);
        });
    }

    private void ShowSpeechBubble(string message)
    {
        speechBubble.DOScale(Vector3.one * bubbleScale, bubblePopDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            StartTypewriter(message);
        });
    }

    private void StartTypewriter(string message)
    {
        currentMessage = message;
        
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(message));
    }

    private IEnumerator TypewriterEffect(string message)
    {
        dialogueText.text = "";
        dialogueComplete = false;

        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        dialogueComplete = true;
    }

    private void CompleteDialogue()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        dialogueText.text = currentMessage;
        dialogueComplete = true;
    }

    private void HideUnlockScreen()
    {
        isShowingDialogue = false;

        Sequence hideSequence = DOTween.Sequence();

        hideSequence.Append(speechBubble.DOScale(Vector3.zero, bubblePopDuration).SetEase(Ease.InBack));
        
        float offScreenPosition = Screen.width + characterPanel.rect.width;
        hideSequence.Append(characterPanel.DOAnchorPosX(offScreenPosition, slideInDuration).SetEase(Ease.InBack));

        hideSequence.OnComplete(() =>
        {
            unlockCanvas.gameObject.SetActive(false);
            onUnlockComplete?.Invoke();
        });
    }
}
