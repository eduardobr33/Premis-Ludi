using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gameplay")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int playerHealth = 5;
    public int difficulty = 1;

    [Header("Gesture Recognizers")]
    public SimpleGestureRecognizer simpleRecognizer;
    public SplitZoneGestureRecognizer splitZoneRecognizer;

    public Enemy currentEnemy { get; private set; }

    private void Awake()
    {
        Instance = this;
        
        // Inicialmente desactivar ambos
        if (simpleRecognizer != null)
            simpleRecognizer.gameObject.SetActive(false);
        
        if (splitZoneRecognizer != null)
            splitZoneRecognizer.gameObject.SetActive(false);
    }

    void Start()
    {
        SpawnEnemy();
    }

    void Update()
    {
        //HandleInput();
    }

    public void SpawnEnemy()
    {
        var (operation, result) = MathGenerator.GenerateOperation(difficulty);

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        currentEnemy = enemyObj.GetComponent<Enemy>();
        currentEnemy.Setup(operation, result);

        SetupGestureRecognizer(operation, result);
    }

    private void SetupGestureRecognizer(string operation, int correctAnswer)
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

    public void PlayerTakeDamage()
    {
        playerHealth--;
        Debug.Log("Player health: " + playerHealth);
    }

    public void EnemyDefeated()
    {
        currentEnemy = null;
        
        // Desactivar ambos reconocedores temporalmente
        // simpleRecognizer?.gameObject.SetActive(false);
        // splitZoneRecognizer?.gameObject.SetActive(false);
        
        Invoke(nameof(SpawnEnemy), 1f); // --> Esto se tendrá q cambiar según el lvl :p + dificulty
    }
}
