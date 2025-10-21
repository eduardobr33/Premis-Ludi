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
    
    public Image buttonImage;
    public Sprite unlockedSprite;
    public Sprite lockedSprite;
    public GameObject[] stars;
    public Sprite starSprite;
    public Sprite emptyStarSprite;

    private int currentStars = 0;
    private bool hasBeenPlayed = false;

    private void Start()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("SaveSystem.Instance es null en LevelSelection!");
            return;
        }

        unlocked = SaveSystem.Instance.IsLevelUnlocked(levelNumber);
        // Debug.Log($"Nivel {levelNumber} desbloqueado: {unlocked}");

        currentStars = SaveSystem.Instance.GetLevelStars(levelNumber);
        hasBeenPlayed = SaveSystem.Instance.HasLevelBeenPlayed(levelNumber);
        
        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = unlocked ? unlockedSprite : lockedSprite;
        }

        if (!unlocked)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                    stars[i].gameObject.SetActive(false);
            }
        }
        else if (hasBeenPlayed)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                {
                    stars[i].gameObject.SetActive(true);
                    Image starImage = stars[i].GetComponent<Image>();
                    
                    if (starImage != null)
                    {
                        starImage.sprite = (i < currentStars && starSprite != null) ? starSprite : emptyStarSprite;
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