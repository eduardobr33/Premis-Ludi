using UnityEngine;

public class GameplayMusicManager : MonoBehaviour
{
    private void Awake()
    {
        AudioManager.Instance?.PlayBattleMusic();
    }

    private void OnDestroy()
    {
        AudioManager.Instance?.PlayBackgroundMusic();
    }
}
