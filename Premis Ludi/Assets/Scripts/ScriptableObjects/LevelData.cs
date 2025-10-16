using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level_", menuName = "Premis Ludi/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName; // Hará falta?
    public int levelNumber;   
    public int difficulty;

    [Header("Math Settings")]
    public List<string> allowedOperations;                      // ["add", "sub", "mul", "div"]
    public Vector2Int numberRange = new Vector2Int(1, 10);
    public int maxSteps = 1;                                    // 1 = only 2 numbers (3 + 4), 2 = (3 + 4 + 2)

    [Header("Gameplay")]
    public int enemyHealth = 1;
    public float levelTime = 30f;
    public float pointsMultiplier = 1f;
    
    [Header("Tutorial Settings")]
    public bool isTutorial = false;
    public int[] tutorialAnswers;
    
    [Header("Level Unlocking")]
    public int[] levelsToUnlock; // Niveles que se desbloquean al completar este
    
    [Header("Rewards")]
    public PowerupType powerupReward = PowerupType.None;
}
