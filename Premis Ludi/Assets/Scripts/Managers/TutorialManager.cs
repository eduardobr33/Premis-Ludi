using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Elements")]
    public CanvasGroup overlayCanvas;
    public TextMeshProUGUI messageText;
    public Image gestureImage;

    [Header("Scene Flow")]
    public string nextSceneName = "GameplayScene";
    public float fadeDuration = 0.5f;

    public bool completed = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Arranque del overlay (fondo negro con opacidad)
        if (overlayCanvas != null)
        {
            overlayCanvas.alpha = 1f;
        }
    }

    private void Start()
    {

    }

    public void ShowTutorial()
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.alpha = 1f;
            overlayCanvas.gameObject.SetActive(true);
        }

        messageText.text = "Dibuixa el número correcte per derrotar l'enemic!";

    }

    public void HideTutorial()
    {
        completed = true;
        if (overlayCanvas != null)
            overlayCanvas.gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeAfterTutorial();
    }

}
