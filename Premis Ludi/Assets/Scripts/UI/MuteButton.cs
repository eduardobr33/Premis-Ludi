using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite audioOnSprite;
    public Sprite audioOffSprite;

    private Button muteButton;
    private Image buttonImage;

    private void Start()
    {
        muteButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        if (muteButton != null)
        {
            muteButton.onClick.AddListener(OnMuteClicked);
        }

        UpdateIcon();
    }

    private void OnMuteClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        if (AudioManager.Instance == null || buttonImage == null) return;

        bool isMuted = AudioManager.Instance.isMuted;
        buttonImage.sprite = isMuted ? audioOffSprite : audioOnSprite;
    }
}
