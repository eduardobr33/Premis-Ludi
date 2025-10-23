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
        saveData = new GameSaveData();
        
        // Cargar nivel 0 siempre desbloqueado
        saveData.levels[0].unlocked = true;
        
        // Cargar datos individuales de cada nivel
        for (int i = 1; i < 20; i++)
        {
            string unlockedKey = $"Level_{i}_Unlocked";
            string playedKey = $"Level_{i}_Played";
            string starsKey = $"Level_{i}_Stars";
            
            saveData.levels[i].unlocked = PlayerPrefs.GetInt(unlockedKey, 0) == 1;
            saveData.levels[i].played = PlayerPrefs.GetInt(playedKey, 0) == 1;
            saveData.levels[i].stars = PlayerPrefs.GetInt(starsKey, 0);
        }
        
        // Cargar power-ups desbloqueados
        saveData.unlockedPowerups.Clear();
        int powerupCount = PlayerPrefs.GetInt("PowerupCount", 0);
        for (int i = 0; i < powerupCount; i++)
        {
            int powerupId = PlayerPrefs.GetInt($"Powerup_{i}", -1);
            if (powerupId >= 0)
                saveData.unlockedPowerups.Add(powerupId);
        }
        
        Debug.Log("Partida cargada desde PlayerPrefs");
    }

    public void SaveGame()
    {
        try
        {
            // Guardar niveles individuales
            for (int i = 1; i < 20; i++)
            {
                PlayerPrefs.SetInt($"Level_{i}_Unlocked", saveData.levels[i].unlocked ? 1 : 0);
                PlayerPrefs.SetInt($"Level_{i}_Played", saveData.levels[i].played ? 1 : 0);
                PlayerPrefs.SetInt($"Level_{i}_Stars", saveData.levels[i].stars);
            }
            
            // Guardar power-ups
            PlayerPrefs.SetInt("PowerupCount", saveData.unlockedPowerups.Count);
            for (int i = 0; i < saveData.unlockedPowerups.Count; i++)
            {
                PlayerPrefs.SetInt($"Powerup_{i}", saveData.unlockedPowerups[i]);
            }
            
            PlayerPrefs.Save();
            Debug.Log("Partida guardada en PlayerPrefs");
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
