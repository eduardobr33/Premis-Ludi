using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
// using System.Numerics;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Character")]
    public RectTransform characterPanel;
    public Image characterImage;
    public RectTransform speechBubble;
    public TextMeshProUGUI dialogueText;
    public float slideInDuration = 0.5f;
    public float bubblePopDuration = 0.3f;
    
    [Header("Number Helper")]
    public GameObject numberHelperParent;
    public GameObject[] numberPrefabs; // 0-9
    
    [Header("Tutorial Messages")]
    [TextArea] public string welcomeMessage = "¡Bienvenido! Los enemigos han invadido nuestra tierra...";
    [TextArea] public string firstEnemyMessage = "Dibuja el número que ves en el enemigo para eliminarlo.";
    [TextArea] public string congratsMessage = "¡Muy bien! Ayúdame a liberar los enemigos de la región.";
    
    private bool isShowingDialogue = false;
    private bool dialogueComplete = false;
    private Coroutine typewriterCoroutine;
    private int tutorialStep = 0;
    private GameObject currentNumberHelper;
    private string currentMessage = "";
    private System.Action currentOnComplete;

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
        
        if (speechBubble != null)
        {
            speechBubble.localScale = Vector3.zero;
        }
        
        if (numberHelperParent != null)
            numberHelperParent.SetActive(false);
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
                HideCharacter();
            }
        }
    }

    public void StartTutorial()
    {
        tutorialStep = 0;
        // Delay 1s para que se dispersen las nubes de transición
        Invoke(nameof(ShowWelcome), 1f);
    }

    private void ShowWelcome()
    {
        ShowCharacterWithMessage(welcomeMessage);
    }

    public void ShowFirstEnemyInstructions(int enemyNumber)
    {
        tutorialStep = 1;
        ShowCharacterWithMessage(firstEnemyMessage, () => 
        {
            ShowNumberHelper(enemyNumber);
        });
    }

    public void ShowCongratulations()
    {
        tutorialStep = 2;
        HideNumberHelper();
        ShowCharacterWithMessage(congratsMessage, () =>
        {
            Invoke(nameof(EndTutorial), 2f);
        });
    }

    private void ShowCharacterWithMessage(string message, System.Action onComplete = null)
    {
        if (characterPanel == null) return;

        isShowingDialogue = true;
        dialogueComplete = false;
        currentMessage = message;
        currentOnComplete = onComplete;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.tutorialActive = true;
            if (GameManager.Instance.currentEnemy != null)
            {
                GameManager.Instance.currentEnemy.PauseScaling();
            }
        }

        if (speechBubble != null)
        {
            speechBubble.localScale = Vector3.zero;
        }

        characterPanel.DOAnchorPosX(0, slideInDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            if (speechBubble != null)
            {
                speechBubble.DOScale(2.7f, bubblePopDuration).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    if (typewriterCoroutine != null)
                        StopCoroutine(typewriterCoroutine);
                    
                    typewriterCoroutine = StartCoroutine(TypewriterEffect(message, onComplete));
                });
            }
            else
            {
                if (typewriterCoroutine != null)
                    StopCoroutine(typewriterCoroutine);
                
                typewriterCoroutine = StartCoroutine(TypewriterEffect(message, onComplete));
            }
        });
    }

    private void HideCharacter()
    {
        if (characterPanel == null) return;

        isShowingDialogue = false;
        dialogueComplete = false;
        currentOnComplete = null;
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.tutorialActive = false;
            if (GameManager.Instance.currentEnemy != null)
            {
                GameManager.Instance.currentEnemy.ResumeScaling();
            }
        }

        if (speechBubble != null)
        {
            speechBubble.DOScale(0f, bubblePopDuration * 0.5f).SetEase(Ease.InBack);
        }

        float offScreenPosition = Screen.width + characterPanel.rect.width;
        characterPanel.DOAnchorPosX(offScreenPosition, slideInDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            if (tutorialStep == 0 && GameManager.Instance != null)
            {
                GameManager.Instance.OnWelcomeComplete();
            }
        });
    }

    private IEnumerator TypewriterEffect(string message, System.Action onComplete = null)
    {
        dialogueText.text = "";
        
        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }
        
        dialogueComplete = true;
        onComplete?.Invoke();
    }

    private void CompleteDialogue()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        
        if (dialogueText != null && !string.IsNullOrEmpty(currentMessage))
        {
            dialogueText.text = currentMessage;
        }
        
        dialogueComplete = true;
        
        if (currentOnComplete != null)
        {
            currentOnComplete.Invoke();
            currentOnComplete = null;
        }
    }

    private void ShowNumberHelper(int number)
    {
        if (numberHelperParent == null || numberPrefabs == null || number < 0 || number >= numberPrefabs.Length)
            return;

        HideNumberHelper();
        
        numberHelperParent.SetActive(true);
        currentNumberHelper = Instantiate(numberPrefabs[number], numberHelperParent.transform);
        
        currentNumberHelper.transform.localScale = Vector3.zero;
        currentNumberHelper.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
    }

    private void HideNumberHelper()
    {
        if (currentNumberHelper != null)
        {
            Destroy(currentNumberHelper);
            currentNumberHelper = null;
        }
        
        if (numberHelperParent != null)
            numberHelperParent.SetActive(false);
    }

    private void EndTutorial()
    {
        HideCharacter();
        HideNumberHelper();
    }
}
