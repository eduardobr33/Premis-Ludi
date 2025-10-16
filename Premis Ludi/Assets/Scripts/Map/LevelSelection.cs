using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    [SerializeField] private bool unlocked = false;
    [SerializeField] private int levelNumber;
    [SerializeField] private LevelData levelData;
    
    public Image unlockImage;
    public GameObject[] stars;
    public Sprite starSprite;
    public Sprite emptyStarSprite;

    private int currentStars = 0;
    private bool hasBeenPlayed = false;

    private void Start()
    {
        // SaveSystem.Instance.ResetAllProgress(); // Para resetear progreso
        
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("SaveSystem.Instance es null en LevelSelection!");
            return;
        }
        
        if (levelNumber == 1)
        {
            unlocked = true;
        }
        else
        {
            unlocked = SaveSystem.Instance.IsLevelUnlocked(levelNumber);
        }

        currentStars = SaveSystem.Instance.GetLevelStars(levelNumber);
        hasBeenPlayed = SaveSystem.Instance.HasLevelBeenPlayed(levelNumber);
        
        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        if (!unlocked)
        {
            if (unlockImage != null)
                unlockImage.gameObject.SetActive(true);
            
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                    stars[i].gameObject.SetActive(false);
            }
        }
        else
        {
            if (unlockImage != null)
                unlockImage.gameObject.SetActive(false);
            
            if (hasBeenPlayed)
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (stars[i] != null)
                    {
                        stars[i].gameObject.SetActive(true);
                        Image starImage = stars[i].GetComponent<Image>();
                        
                        if (starImage != null)
                        {
                            if (i < currentStars && starSprite != null)
                            {
                                starImage.sprite = starSprite;
                            }
                            else if (emptyStarSprite != null)
                            {
                                starImage.sprite = emptyStarSprite;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (stars[i] != null)
                        stars[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void OnLevelPressed()
    {
        if (!unlocked) return;
        
        if (levelData == null)
        {
            Debug.LogWarning($"No hay LevelData asignado para el nivel {levelNumber}");
            return;
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(levelData);
        }
        else
        {
            Debug.LogWarning("LevelManager.Instance es null!");
        }
        
        SceneManager.LoadScene("GameplayScene");
    }

    public void PressSelection(string _LevelName)
    {
        OnLevelPressed();
    }
}