using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.ShaderGraph.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gameplay")]
    public GameObject crabPrefab;
    public GameObject bushPrefab;
    public GameObject bossPrefab;
    public Transform spawnPoint;

    [Header("Level Timer")]
    public float levelTime = 30f;

    [Header("Chibi Timer")]
    public Transform chibiTimer;
    public Transform timerStartPosition;
    public Transform timerEndPosition;

    [Header("Scoring")]
    public int score = 0;
    public int enemyPoints = 10;
    public int multiplier = 1;
    public int maxMultiplier = 5;

    [Header("Tutorial")]
    private bool isTutorialScene = false;
    public bool tutorialActive = false;
    private bool tutorialShown = false;
    private int tutorialEnemyIndex = 0;
    public Canvas tutorialCanvas;

    [Header("Gesture Recognizers")]
    public SimpleGestureRecognizer simpleRecognizer;
    public SplitZoneGestureRecognizer splitZoneRecognizer;

    [HideInInspector]
    public Enemy currentEnemy { get; private set; }
    [HideInInspector]
    public int playerHealth;
    [HideInInspector]
    public LevelData currentLevelData;

    private float timer;
    private float timeScale = 1f;
    private bool bossSpawned = false;

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

            Debug.Log($"Nivel cargado: {currentLevelData.levelName}");
            Debug.Log($"Dificultad: {currentLevelData.difficulty}");
        }
    }

    private void Start()
    {
        timer = levelTime;
        

        chibiTimer.localPosition = timerStartPosition.localPosition;
        
        
        if (tutorialCanvas != null)
        {
            bool isTutorial = currentLevelData != null && currentLevelData.isTutorial;
            tutorialCanvas.gameObject.SetActive(isTutorial);
        }
        
        if (currentLevelData != null && currentLevelData.isTutorial)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.StartTutorial();
            }
            else
            {
                SpawnEnemy();
            }
        }
        else
        {
            SpawnEnemy();
        }
    }
    
    public void OnWelcomeComplete()
    {
        SpawnEnemy();
    }

    private void Update()
    {
        if (!tutorialActive)
        {
            HandleTimer();
        }
    }

    private void HandleTimer()
    {
        if (playerHealth <= 0) return;

        timer -= Time.deltaTime * timeScale;

        float progressRatio = 1f - (timer / levelTime);
        progressRatio = Mathf.Clamp01(progressRatio);

        Vector3 newPosition = Vector3.Lerp(timerStartPosition.localPosition, timerEndPosition.localPosition, progressRatio);
        chibiTimer.localPosition = newPosition;

        if (UIManager.Instance != null) UIManager.Instance.UpdateTimer(timer);

        if (timer <= 0f)
        {
            if (currentLevelData != null && currentLevelData.hasBoss && !bossSpawned)
            {
                SpawnBoss();
                bossSpawned = true;
            }
            else if (!currentLevelData.hasBoss)
            {
                WinGame();
            }
        }
    }

    public void SpawnEnemy()
    {
        string operation;
        int result;
        
        if (currentLevelData != null && currentLevelData.isTutorial && 
            currentLevelData.tutorialAnswers != null && 
            tutorialEnemyIndex < currentLevelData.tutorialAnswers.Length)
        {
            result = currentLevelData.tutorialAnswers[tutorialEnemyIndex];
            operation = GenerateSimpleTutorialOperation(result);
        }
        else
        {
            (operation, result) = MathGenerator.GenerateOperation(currentLevelData);
        }

        GameObject chosenPrefab = (Random.value > 0.5f) ? bushPrefab : crabPrefab;

        GameObject enemyObj = Instantiate(chosenPrefab, spawnPoint.position, Quaternion.identity);
        currentEnemy = enemyObj.GetComponent<Enemy>();

        if (currentLevelData != null)
        {
            currentEnemy.health = currentLevelData.enemyHealth;
        }

        currentEnemy.Setup(operation, result, true);

        SetupGestureRecognizer(operation, result);
        
        if (currentLevelData != null && currentLevelData.isTutorial && tutorialEnemyIndex == 0)
        {
            if (TutorialManager.Instance != null)
            {
                Invoke(nameof(ShowFirstEnemyTutorial), 0.5f);
            }
        }
    }
    
    private string GenerateSimpleTutorialOperation(int result)
    {
        result = Mathf.Clamp(result, 2, 9);
        int num1 = Random.Range(1, result);
        int num2 = result - num1;
        return $"{num1} + {num2}";
    }
    
    private void ShowFirstEnemyTutorial()
    {
        if (currentEnemy != null && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowFirstEnemyInstructions(currentEnemy.correctAnswer);
        }
    }

    public void SetupGestureRecognizer(string operation, int correctAnswer)
    {
        bool isSingleDigit = correctAnswer >= 0 && correctAnswer <= 9;

        if (isSingleDigit)
        {
            simpleRecognizer.gameObject.SetActive(true);
            splitZoneRecognizer.gameObject.SetActive(false);
        }
        else
        {
            splitZoneRecognizer.gameObject.SetActive(true);
            simpleRecognizer.gameObject.SetActive(false);
        }
    }

    public void EnemyDefeated(bool instaKill)
    {
        // For the tutorial
        if (currentLevelData != null && currentLevelData.isTutorial && tutorialEnemyIndex == 0)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowCongratulations();
            }
        }

        if (currentLevelData != null && currentLevelData.isTutorial)
        {
            tutorialEnemyIndex++;
        }

        // Level with boss
        if (currentLevelData != null && currentLevelData.hasBoss && currentEnemy.enemyType == Enemy.EnemyType.Boss)
        {
            WinGame();
        }

        if (!instaKill)
        {
            int pointsEarned = enemyPoints * multiplier;

            if (multiplier < maxMultiplier) multiplier++;

            if (Player.Instance.doublePointsActive)
            {
                score *= 2;
                Player.Instance.doublePointsActive = false;
            }

            if (UIManager.Instance != null && currentEnemy != null)
            {
                UIManager.Instance.ShowPointPopup(pointsEarned, currentEnemy.transform.position, () =>
                {
                    score += pointsEarned;
                    UIManager.Instance.UpdateScore(score);
                });
            }
            else
            {
                score += pointsEarned;
            }
        }

        // Desactivar ambos reconocedores temporalmente
        // simpleRecognizer?.gameObject.SetActive(false);
        // splitZoneRecognizer?.gameObject.SetActive(false);

        currentEnemy = null;

        Invoke(nameof(SpawnEnemy), 1f); // --> Esto se tendrá q cambiar según el lvl :p + dificulty
    }
    
    private void SpawnBoss()
    {
        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        if (bossPrefab != null)
        {
            GameObject bossObj = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
            currentEnemy = bossObj.GetComponent<Enemy>();

            if (UIManager.Instance != null)
            {
                //UIManager.Instance.BossUI();
            }
        }
        else
        {
            Debug.Log("El nivel tiene hasBoss = true, pero no se ha asignado nada al bossPrefab");
        }
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
        
        if (levelNum == 0)
        {
            SaveSystem.Instance.UnlockLevel(1);
            return;
        }

        SaveSystem.Instance.MarkLevelAsPlayed(levelNum);
        SaveSystem.Instance.SetLevelStars(levelNum, stars);

        // Desbloquear niveles configurados en LevelData
        if (currentLevelData.levelsToUnlock != null && currentLevelData.levelsToUnlock.Length > 0)
        {
            foreach (int levelToUnlock in currentLevelData.levelsToUnlock)
            {
                SaveSystem.Instance.UnlockLevel(levelToUnlock);
            }
        }
        else
        {
            // Si no hay configuración específica, desbloquear el siguiente
            int nextLevel = levelNum + 1;
            SaveSystem.Instance.UnlockLevel(nextLevel);
        }

        if (currentLevelData.powerupReward != PowerupType.None)
        {
            SaveSystem.Instance.UnlockPowerup(currentLevelData.powerupReward);
        }
    }

    public void LoseGame()
    {
        SceneManager.LoadScene("LoseScene");
    }

    public void SetTimeScale(float scale)
    {
        timeScale = scale;
    }
}
