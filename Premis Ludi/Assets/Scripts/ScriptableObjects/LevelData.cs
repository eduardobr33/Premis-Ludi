using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "Premis Ludi/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public string levelName; // Hará falta?
    
    public int difficulty = 1; // 1 : sumas | 2 : restas | 3 : multiplicaciones
    
    public int enemyHealth = 1;
}
