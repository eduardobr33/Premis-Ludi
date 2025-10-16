using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public LevelProgress[] levels = new LevelProgress[10];
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
    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
        LoadGame();
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                saveData = JsonUtility.FromJson<GameSaveData>(json);
                
                // Validar que saveData no sea null después de deserializar
                if (saveData == null)
                {
                    Debug.LogWarning("saveData es null después de deserializar, creando nueva instancia");
                    saveData = new GameSaveData();
                    saveData.levels[0].unlocked = true;
                }
                
                Debug.Log("Partida cargada desde: " + saveFilePath);
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
            Debug.Log("No existe archivo de guardado, creando nuevo");
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
            File.WriteAllText(saveFilePath, json);
            // Debug.Log("Partida guardada en: " + saveFilePath);
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
        
        int index = levelNumber - 1;
        if (index >= 0 && index < saveData.levels.Length)
        {
            return saveData.levels[index].unlocked;
        }
        return false;
    }

    public void UnlockLevel(int levelNumber)
    {
        if (saveData == null) return;
        
        int index = levelNumber - 1;
        if (index >= 0 && index < saveData.levels.Length)
        {
            saveData.levels[index].unlocked = true;
            SaveGame();
        }
    }

    public bool HasLevelBeenPlayed(int levelNumber)
    {
        if (levelNumber == 0) return false;
        if (saveData == null) return false;
        
        int index = levelNumber - 1;
        if (index >= 0 && index < saveData.levels.Length)
        {
            return saveData.levels[index].played;
        }
        return false;
    }

    public void MarkLevelAsPlayed(int levelNumber)
    {
        if (saveData == null) return;
        
        int index = levelNumber - 1;
        if (index >= 0 && index < saveData.levels.Length)
        {
            saveData.levels[index].played = true;
            SaveGame();
        }
    }

    public int GetLevelStars(int levelNumber)
    {
        if (levelNumber == 0) return 0;
        if (saveData == null) return 0;
        
        int index = levelNumber - 1;
        if (index >= 0 && index < saveData.levels.Length)
        {
            return saveData.levels[index].stars;
        }
        return 0;
    }

    public void SetLevelStars(int levelNumber, int stars)
    {
        if (saveData == null) return;
        
        int index = levelNumber - 1;
        if (index >= 0 && index < saveData.levels.Length)
        {
            if (stars > saveData.levels[index].stars)
            {
                saveData.levels[index].stars = stars;
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
