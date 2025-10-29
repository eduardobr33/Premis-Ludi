using UnityEngine;
using UnityEngine.UI;

public class MapDebugButtons : MonoBehaviour
{
    [SerializeField] private Button unlockAllButton;
    [SerializeField] private Button resetProgressButton;

    private void Start()
    {
        if (unlockAllButton != null)
            unlockAllButton.onClick.AddListener(OnUnlockAllClicked);

        if (resetProgressButton != null)
            resetProgressButton.onClick.AddListener(OnResetProgressClicked);
    }

    private void OnUnlockAllClicked()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("[MapDebugButtons] SaveSystem no encontrado");
            return;
        }

        // Desbloquear todos los niveles
        for (int i = 0; i < 20; i++)
        {
            SaveSystem.Instance.UnlockLevel(i);
        }

        // Desbloquear todos los powerups
        SaveSystem.Instance.UnlockPowerup(PowerupType.Skip);
        SaveSystem.Instance.UnlockPowerup(PowerupType.DoublePoints);
        SaveSystem.Instance.UnlockPowerup(PowerupType.SlowMotion);
        SaveSystem.Instance.UnlockPowerup(PowerupType.MultiplicationTables);

        // Actualizar UI
        RefreshLevelUI();

        Debug.Log("[MapDebugButtons] ¡Todos los niveles y powerups desbloqueados!");
    }

    private void OnResetProgressClicked()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("[MapDebugButtons] SaveSystem no encontrado");
            return;
        }

        SaveSystem.Instance.ResetAllProgress();
        
        // Actualizar UI
        RefreshLevelUI();
        
        Debug.Log("[MapDebugButtons] Progreso reseteado");
    }

    private void RefreshLevelUI()
    {
        // Buscar y actualizar LevelProgressIndicator
        LevelProgressIndicator[] levelIndicators = FindObjectsOfType<LevelProgressIndicator>();
        foreach (LevelProgressIndicator indicator in levelIndicators)
        {
            indicator.RefreshUI();
        }

        // Buscar y actualizar TODOS los LevelSelection
        LevelSelection[] levelSelections = FindObjectsOfType<LevelSelection>();
        foreach (LevelSelection levelSelection in levelSelections)
        {
            levelSelection.RefreshUI();
        }

        Debug.Log("[MapDebugButtons] UI actualizada - " + levelSelections.Length + " botones actualizados");
    }

    private void OnDestroy()
    {
        if (unlockAllButton != null)
            unlockAllButton.onClick.RemoveListener(OnUnlockAllClicked);

        if (resetProgressButton != null)
            resetProgressButton.onClick.RemoveListener(OnResetProgressClicked);
    }
}
