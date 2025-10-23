using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public LevelProgress[] levels = new LevelProgress[20];
    public List<int> unlockedPowerups = new List<int>();
    
    public GameSaveData()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i] = new LevelProgress();
        }
    }
}

[System.Serializable]
public class LevelProgress
{
    public bool unlocked = false;
    public bool played = false;
    public int stars = 0;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    
    private GameSaveData saveData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("GameSaveData"))
        {
            try
            {
                string json = PlayerPrefs.GetString("GameSaveData");
                saveData = JsonUtility.FromJson<GameSaveData>(json);
                
                if (saveData == null)
                {
                    Debug.LogWarning("saveData es null después de deserializar, creando nueva instancia");
                    saveData = new GameSaveData();
                    saveData.levels[0].unlocked = true;
                }
                
                Debug.Log("Partida cargada desde PlayerPrefs");
            }
            catch (Exception e)
            {
                Debug.LogError("Error al cargar partida: " + e.Message);
                saveData = new GameSaveData();
                saveData.levels[0].unlocked = true;
            }
        }
        else
        {
            Debug.Log("No existe guardado, creando nuevo");
            saveData = new GameSaveData();
            saveData.levels[0].unlocked = true;
            SaveGame();
        }
    }

    public void SaveGame()
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            PlayerPrefs.SetString("GameSaveData", json);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError("Error al guardar partida: " + e.Message);
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber == 0) return true;
        if (saveData == null) return false;
        
        if (levelNumber >= 0 && levelNumber < saveData.levels.Length)
        {
            return saveData.levels[levelNumber].unlocked;
        }
        return false;
    }

    public void UnlockLevel(int levelNumber)
    {
        if (saveData == null) return;
        
        if (levelNumber >= 0 && levelNumber < saveData.levels.Length)
        {
            saveData.levels[levelNumber].unlocked = true;
            SaveGame();
            Debug.Log($"Nivel {levelNumber} desbloqueado");
        }
    }

    public bool HasLevelBeenPlayed(int levelNumber)
    {
        if (saveData == null) return false;
        
        if (levelNumber >= 0 && levelNumber < saveData.levels.Length)
        {
            return saveData.levels[levelNumber].played;
        }
        return false;
    }

    public void MarkLevelAsPlayed(int levelNumber)
    {
        if (saveData == null) return;
        
        if (levelNumber >= 0 && levelNumber < saveData.levels.Length)
        {
            saveData.levels[levelNumber].played = true;
            SaveGame();
        }
    }

    public int GetLevelStars(int levelNumber)
    {
        if (saveData == null) return 0;
        
        if (levelNumber >= 0 && levelNumber < saveData.levels.Length)
        {
            return saveData.levels[levelNumber].stars;
        }
        return 0;
    }

    public void SetLevelStars(int levelNumber, int stars)
    {
        if (saveData == null) return;
        
        if (levelNumber >= 0 && levelNumber < saveData.levels.Length)
        {
            if (stars > saveData.levels[levelNumber].stars)
            {
                saveData.levels[levelNumber].stars = stars;
                SaveGame();
            }
        }
    }

    public void ResetAllProgress()
    {
        saveData = new GameSaveData();
        saveData.levels[0].unlocked = true;
        SaveGame();
        Debug.Log("Progreso reseteado");
    }
    
    public void UnlockPowerup(PowerupType powerupType)
    {
        int powerupId = (int)powerupType;
        if (powerupId != 0 && !saveData.unlockedPowerups.Contains(powerupId))
        {
            saveData.unlockedPowerups.Add(powerupId);
            SaveGame();
            Debug.Log($"Powerup desbloqueado: {powerupType}");
        }
    }
    
    public bool IsPowerupUnlocked(PowerupType powerupType)
    {
        int powerupId = (int)powerupType;
        return saveData.unlockedPowerups.Contains(powerupId);
    }
    
    public List<PowerupType> GetUnlockedPowerups()
    {
        List<PowerupType> powerups = new List<PowerupType>();
        foreach (int id in saveData.unlockedPowerups)
        {
            powerups.Add((PowerupType)id);
        }
        return powerups;
    }
}
