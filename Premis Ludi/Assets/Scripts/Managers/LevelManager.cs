using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    
    public LevelData currentLevelData { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadLevel(LevelData levelData)
    {
        currentLevelData = levelData;
    }
    
    public void ClearLevelData()
    {
        currentLevelData = null;
    }
}
