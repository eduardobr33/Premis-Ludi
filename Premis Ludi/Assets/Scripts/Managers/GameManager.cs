using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.ShaderGraph.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gameplay")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int difficulty = 1;

    [Header("Level Timer")]
    public float levelTime = 30f;

    [Header("Scoring")]
    public int score = 0;
    public int enemyPoints = 10;
    public int multiplier = 1;
    public int maxMultiplier = 5;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("Tutorial")]
    private bool isTutorialScene = false;
    public bool tutorialActive = false;
    private bool tutorialShown = false;

    [Header("Gesture Recognizers")]
    public SimpleGestureRecognizer simpleRecognizer;
    public SplitZoneGestureRecognizer splitZoneRecognizer;

    [HideInInspector]
    public Enemy currentEnemy { get; private set; }
    [HideInInspector]
    public int playerHealth;

    private float timer;
    private LevelData currentLevelData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        isTutorialScene = SceneManager.GetActiveScene().name == "TutorialScene";

        // Inicialmente desactivar ambos
        if (simpleRecognizer != null)
            simpleRecognizer.gameObject.SetActive(false);

        if (splitZoneRecognizer != null)
            splitZoneRecognizer.gameObject.SetActive(false);

        tutorialShown = false;
        
        LoadLevelConfiguration();
    }

    private void LoadLevelConfiguration()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.currentLevelData != null)
        {
            currentLevelData = LevelManager.Instance.currentLevelData;
            
            difficulty = currentLevelData.difficulty;
            
            Debug.Log($"Nivel cargado: {currentLevelData.levelName}");
            Debug.Log($"Dificultad: {currentLevelData.difficulty}");
        }
    }

    private void Start()
    {
        timer = levelTime;
        SpawnEnemy();
    }

    private void Update()
    {
        if (!tutorialActive)
        {
            HandleTimer();

            if (isTutorialScene && !tutorialShown && currentEnemy != null)
            {
                if (currentEnemy.transform.localScale.x > 0.5f && !TutorialManager.Instance.completed)
                {
                    ShowTutorial();
                }
            }
        }
    }

    private void HandleTimer()
    {
        if (playerHealth <= 0) return;

        timer -= Time.deltaTime;
        timerText.text = $"{Mathf.CeilToInt(timer)}";

        if (timer <= 0f) WinGame();
    }

    public void SpawnEnemy()
    {
        var (operation, result) = MathGenerator.GenerateOperation(difficulty);

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        currentEnemy = enemyObj.GetComponent<Enemy>();
        
        if (currentLevelData != null)
        {
            currentEnemy.health = currentLevelData.enemyHealth;
        }
        
        currentEnemy.Setup(operation, result, true);

        SetupGestureRecognizer(operation, result);
    }

    public void SetupGestureRecognizer(string operation, int correctAnswer)
    {
        bool isSingleDigit = correctAnswer >= 0 && correctAnswer <= 9;

        if (isSingleDigit)
        {
            simpleRecognizer.gameObject.SetActive(true);
            Debug.Log($"✓ SimpleGestureRecognizer ACTIVADO (respuesta: {correctAnswer} - 1 dígito)");

            splitZoneRecognizer.gameObject.SetActive(false);
            Debug.Log("  SplitZoneGestureRecognizer desactivado");
        }
        else
        {
            splitZoneRecognizer.gameObject.SetActive(true);
            Debug.Log($"✓ SplitZoneGestureRecognizer ACTIVADO (respuesta: {correctAnswer} - 2+ dígitos)");

            simpleRecognizer.gameObject.SetActive(false);
            Debug.Log("  SimpleGestureRecognizer desactivado");
        }

    }

    public void EnemyDefeated(bool instaKill)
    {
        // Score UI
        if (!instaKill)
        {
            score += enemyPoints * multiplier;
            if (multiplier < maxMultiplier) multiplier++;

            if (Player.Instance.doublePointsActive)
            {
                score *= 2;
                Player.Instance.doublePointsActive = false;
            }

            scoreText.text = $"{score}";   
        }

        // Desactivar ambos reconocedores temporalmente
        // simpleRecognizer?.gameObject.SetActive(false);
        // splitZoneRecognizer?.gameObject.SetActive(false);

        currentEnemy = null;

        Invoke(nameof(SpawnEnemy), 1f); // --> Esto se tendrá q cambiar según el lvl :p + dificulty
    }

    public void WinGame()
    {
        if (currentLevelData != null)
        {
            Debug.Log("¡Has ganado el nivel!");
            int stars = CalculateStars();
            SaveLevelProgress(stars);
        }
        
        SceneManager.LoadScene("WinScene");
    }

    private int CalculateStars()
    {
        float healthPercentage = (float)playerHealth / 5f;
        
        if (healthPercentage >= 0.8f) return 3;
        if (healthPercentage >= 0.4f) return 2;
        return 1;
    }

    private void SaveLevelProgress(int stars)
    {
        int levelNum = currentLevelData.levelNumber;
        
        PlayerPrefs.SetInt("Level_" + levelNum + "_Played", 1);
        
        int currentStars = PlayerPrefs.GetInt("Level_" + levelNum + "_Stars", 0);
        if (stars > currentStars)
        {
            PlayerPrefs.SetInt("Level_" + levelNum + "_Stars", stars);
        }
        
        // Desbloquear el siguiente nivel
        int nextLevel = levelNum + 1;
        PlayerPrefs.SetInt("Level_" + nextLevel + "_Unlocked", 1);
        
        PlayerPrefs.Save();
        
        Debug.Log($"Nivel {levelNum} completado con {stars} estrellas");
        Debug.Log($"Nivel {nextLevel} desbloqueado!");
    }

    public void LoseGame()
    {
        SceneManager.LoadScene("LoseScene");
    }

    //Tutorial Manager
    private void ShowTutorial()
    {
        tutorialShown = true;
        tutorialActive = true;
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ShowTutorial();

        if (currentEnemy != null)
            currentEnemy.PauseScaling();
    }

    public void ResumeAfterTutorial()
    {
        tutorialActive = false;
        tutorialShown = false;
        TutorialManager.Instance.completed = false;
        if (currentEnemy != null)
            currentEnemy.ResumeScaling();
    }
}
