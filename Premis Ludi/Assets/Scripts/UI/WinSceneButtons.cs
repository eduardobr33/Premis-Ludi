using UnityEngine;
using UnityEngine.UI;

public class WinSceneButtons : MonoBehaviour
{
    [SerializeField] private Button mapButton;

    private void Start()
    {
        if (ChangeSceneManager.Instance == null)
        {
            Debug.LogError("[WinSceneButtons] ChangeSceneManager.Instance no encontrado");
            return;
        }

        if (mapButton != null)
            mapButton.onClick.AddListener(OnMapClicked);
    }

    private void OnMapClicked()
    {
        ChangeSceneManager.Instance.GoToMapScene();
    }

    private void OnDestroy()
    {
        if (mapButton != null)
            mapButton.onClick.RemoveListener(OnMapClicked);
    }
}
