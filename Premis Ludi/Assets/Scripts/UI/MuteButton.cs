using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    private Button muteButton;

    private void Start()
    {
        muteButton = GetComponent<Button>();

        if (muteButton != null)
        {
            muteButton.onClick.AddListener(OnMuteClicked);
        }
    }

    private void OnMuteClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
        }
    }
}
