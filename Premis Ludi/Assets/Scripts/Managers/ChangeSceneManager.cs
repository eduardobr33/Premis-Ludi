using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneManager : MonoBehaviour
{
    public static ChangeSceneManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Escenas principales
    public void GoToMainMenu() => ChangeScene("MainScene");
    public void GoToMapScene() => ChangeScene("MapScene");
    public void GoToGameplayScene() => ChangeScene("GameplayScene");
    public void GoToWinScene() => ChangeScene("WinScene");
    public void GoToLoseScene() => ChangeScene("LoseScene");

    // Métodos con delay opcional
    public void GoToMainMenuWithDelay(float delay) => ChangeSceneWithDelay("MainScene", delay);
    public void GoToMapSceneWithDelay(float delay) => ChangeSceneWithDelay("MapScene", delay);
    public void GoToGameplaySceneWithDelay(float delay) => ChangeSceneWithDelay("GameplayScene", delay);
    public void GoToWinSceneWithDelay(float delay) => ChangeSceneWithDelay("WinScene", delay);
    public void GoToLoseSceneWithDelay(float delay) => ChangeSceneWithDelay("LoseScene", delay);

    // Método genérico con transición
    private void ChangeScene(string sceneName)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PlayTransitionOut(() =>
            {
                LoadScene(sceneName);
                StartCoroutine(PlayTransitionInAfterSceneLoad());
            });
        }
        else
        {
            LoadScene(sceneName);
        }
    }

    private System.Collections.IEnumerator PlayTransitionInAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.5f);
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PlayTransitionIn();
    }

    private void ChangeSceneWithDelay(string sceneName, float delay)
    {
        StartCoroutine(ChangeSceneAfterDelay(sceneName, delay));
    }

    private System.Collections.IEnumerator ChangeSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeScene(sceneName);
    }

    // Método genérico para cargar escena
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Método para recargar la escena actual
    public void ReloadCurrentScene()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PlayTransitionOut(() =>
            {
                LoadScene(SceneManager.GetActiveScene().name);
                StartCoroutine(PlayTransitionInAfterSceneLoad());
            });
        }
        else
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
