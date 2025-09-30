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

    public Enemy currentEnemy { get; private set; }

    private void Awake()
    {
        Instance = this;
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
    }

    public void PlayerTakeDamage()
    {
        playerHealth--;
        Debug.Log("Player health: " + playerHealth);
    }
}
