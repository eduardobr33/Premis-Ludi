using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [Header("Player Stats")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Powerup Usage (Current Game)")]
    public bool skipUsed = false;
    public bool doublePointsUsed = false;
    public bool doublePointsActive = false;
    public bool slowMotionUsed = false;
    
    [Header("Powerup Settings")]
    public float slowMotionTime = 5f;

    [Header("UI")]
    public GameObject multiplicationTablesPanel;
    
    [Header("Powerup Buttons")]
    public Button skipButton;
    public Button doublePointsButton;
    public Button slowMotionButton;
    public Button multiplicationTablesButton;

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
        UpdatePowerupButtons();
    }
    
    private void Update()
    {
        UpdatePowerupButtons();
    }
    
    private void UpdatePowerupButtons()
    {
        skipButton.interactable = CanUseSkip();
        doublePointsButton.interactable = CanUseDoublePoints();
        slowMotionButton.interactable = CanUseSlowMotion();
        multiplicationTablesButton.interactable = CanUseMultiplicationTables();
    }

    public void TakeDamage()
    {
        currentHealth--;
        GameManager.Instance.playerHealth = currentHealth;
        GameManager.Instance.multiplier = 1;

        Debug.Log("Player health: " + currentHealth);

        if (currentHealth <= 0)
        {
            GameManager.Instance.LoseGame();
        }
    }

    public void SkipEnemy()
    {
        if (skipUsed) return;

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

    public void SlowMotion()
    {
        if (slowMotionUsed) return;

        slowMotionUsed = true;
        Debug.Log("Slow Motion activado: tiempo reducido a la mitad durante 5s.");

        StartCoroutine(SlowMotionCoroutine());
    }

    public void ToggleMultiplicationTables()
    {
        bool newState = !multiplicationTablesPanel.activeSelf;
        multiplicationTablesPanel.SetActive(newState);
        Debug.Log($"Panel de tablas {(newState ? "mostrado" : "oculto")}.");
    }
    
    // Métodos para verificar disponibilidad de powerups (para UI)
    public bool CanUseSkip()
    {
        return SaveSystem.Instance.IsPowerupUnlocked(PowerupType.Skip) && !skipUsed;
    }
    
    public bool CanUseDoublePoints()
    {
        return SaveSystem.Instance.IsPowerupUnlocked(PowerupType.DoublePoints) && !doublePointsUsed;
    }
    
    public bool CanUseSlowMotion()
    {
        return SaveSystem.Instance.IsPowerupUnlocked(PowerupType.SlowMotion) && !slowMotionUsed;
    }
    
    public bool CanUseMultiplicationTables()
    {
        return SaveSystem.Instance.IsPowerupUnlocked(PowerupType.MultiplicationTables);
    }

    private IEnumerator SlowMotionCoroutine()
    {
        GameManager.Instance.SetTimeScale(0.5f);

        if (GameManager.Instance.currentEnemy != null)
        {
            GameManager.Instance.currentEnemy.enemySpeed *= 0.2f;
            GameManager.Instance.currentEnemy.attackSpeed *= 0.2f;
        }

        yield return new WaitForSeconds(slowMotionTime);

        GameManager.Instance.SetTimeScale(1f);

        if (GameManager.Instance.currentEnemy != null)
        {
            GameManager.Instance.currentEnemy.enemySpeed *= 5f;
            GameManager.Instance.currentEnemy.attackSpeed *= 5f;
        }

        Debug.Log("Slow Motion finalizado. Velocidades restauradas.");
    }
}
