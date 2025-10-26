using UnityEngine;
using UnityEngine.UI;

public class AutoButtonSoundApplier : MonoBehaviour
{
    private void Start()
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        
        foreach (Button btn in allButtons)
        {
            if (btn.GetComponent<ButtonSoundEffect>() == null)
            {
                btn.gameObject.AddComponent<ButtonSoundEffect>();
            }
        }
    }
}
