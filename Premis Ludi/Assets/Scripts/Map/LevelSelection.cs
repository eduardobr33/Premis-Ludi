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
    public Sprite bossUnlockedSprite;
    public Sprite bossLockedSprite;
    public GameObject[] stars;
    public Sprite starSprite;
    public Sprite emptyStarSprite;

    private int currentStars = 0;
    private bool hasBeenPlayed = false;

    private void Start()
    {
        if (SaveSystem.Instance == null)
        {
            return;
        }

        unlocked = SaveSystem.Instance.IsLevelUnlocked(levelNumber);

        currentStars = SaveSystem.Instance.GetLevelStars(levelNumber);
        hasBeenPlayed = SaveSystem.Instance.HasLevelBeenPlayed(levelNumber);
        
        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        if (buttonImage != null)
        {
            bool hasBoss = levelData != null && levelData.hasBoss;
            
            if (hasBoss)
            {
                buttonImage.sprite = unlocked ? bossUnlockedSprite : bossLockedSprite;
            }
            else
            {
                buttonImage.sprite = unlocked ? unlockedSprite : lockedSprite;
            }
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
            return;
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(levelData);
        }
        
        ChangeSceneManager.Instance.GoToGameplayScene();
    }

    public void PressSelection(string _LevelName)
    {
        OnLevelPressed();
    }

    public void RefreshUI()
    {
        unlocked = SaveSystem.Instance.IsLevelUnlocked(levelNumber);
        currentStars = SaveSystem.Instance.GetLevelStars(levelNumber);
        hasBeenPlayed = SaveSystem.Instance.HasLevelBeenPlayed(levelNumber);
        UpdateLevelUI();
    }
}