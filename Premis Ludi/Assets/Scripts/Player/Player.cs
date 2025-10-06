using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [Header("Player Stats")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Abilities")]
    public bool skipUsed = false;
    public bool doublePointsUsed = false;
    public bool doublePointsActive = false;

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
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage()
    {
        currentHealth--;
        GameManager.Instance.playerHealth = currentHealth;
        GameManager.Instance.multiplier = 1;

        Debug.Log("Player health: {currentHealth}");

        if (currentHealth <= 0)
        {
            GameManager.Instance.LoseGame();
        }
    }

    public void SkipEnemy()
    {
        if (skipUsed || GameManager.Instance.currentEnemy == null) return;

        skipUsed = true;
        Debug.Log("Skip usado: enemigo eliminado sin puntos.");
        GameManager.Instance.currentEnemy.TakeDamage(true);
    }

    public void DoublePoints()
    {
        if (doublePointsUsed || doublePointsActive) return;

        doublePointsUsed = true;
        doublePointsActive = true;
        Debug.Log("Doble puntos activado para el siguiente enemigo.");
    }

    // public void ToggleMultiplicationTables()
    // {
    //     if (multiplicationTablesPanel == null) return;

    //     bool newState = !multiplicationTablesPanel.activeSelf;
    //     multiplicationTablesPanel.SetActive(newState);

    //     Debug.Log($"Panel de tablas {(newState ? "mostrado" : "oculto")}.");
    // }
}
