using UnityEngine;
using UnityEngine.UI;

public class LoseSceneButtons : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mapButton;

    private void Start()
    {
        if (ChangeSceneManager.Instance == null)
        {
            Debug.LogError("[LoseSceneButtons] ChangeSceneManager.Instance no encontrado");
            return;
        }

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (mapButton != null)
            mapButton.onClick.AddListener(OnMapClicked);
    }

    private void OnRetryClicked()
    {
        if (LevelManager.Instance?.currentLevelData != null)
            ChangeSceneManager.Instance.GoToGameplayScene();
        else
            ChangeSceneManager.Instance.GoToMapScene();
    }

    private void OnMapClicked()
    {
        ChangeSceneManager.Instance.GoToMapScene();
    }

    private void OnDestroy()
    {
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryClicked);
        if (mapButton != null)
            mapButton.onClick.RemoveListener(OnMapClicked);
    }
}
