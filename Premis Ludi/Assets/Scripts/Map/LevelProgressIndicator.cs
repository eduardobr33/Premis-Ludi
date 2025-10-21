using UnityEngine;
using DG.Tweening;

public class LevelProgressIndicator : MonoBehaviour
{
    [SerializeField] private Vector3 position0;
    [SerializeField] private Vector3 position1;
    [SerializeField] private Vector3 position2;
    [SerializeField] private Vector3 position3;
    
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Ease easeType = Ease.InOutQuad;
    
    private Transform objectTransform;

    private void Start()
    {
        objectTransform = GetComponent<Transform>();
        if (objectTransform == null)
        {
            Debug.LogError("LevelProgressIndicator requiere un componente Transform");
            return;
        }

        objectTransform.localPosition = position0;
        Invoke(nameof(PlayAnimation), 1f);
    }

    private void PlayAnimation()
    {
        int unlockedCount = GetUnlockedLevelCount();
        UpdatePosition(unlockedCount);
    }

    private void UpdatePosition(int unlockedCount)
    {
        Vector3 targetPosition = GetPositionForLevel(unlockedCount);
        objectTransform.DOLocalMove(targetPosition, moveDuration).SetEase(easeType);
    }

    private int GetUnlockedLevelCount()
    {
        if (SaveSystem.Instance == null)
            return 0;

        int unlockedCount = 0;
        for (int i = 0; i < 20; i++)
        {
            if (SaveSystem.Instance.IsLevelUnlocked(i))
                unlockedCount++;
        }

        // Debug.Log("Total niveles desbloqueados: " + unlockedCount);
        return unlockedCount;
    }

    private Vector3 GetPositionForLevel(int unlockedLevel)
    {
        if (unlockedLevel >= 10) return position3;
        if (unlockedLevel >= 6) return position2;
        if (unlockedLevel >= 3) return position1;
        return position0;
    }

    private void OnDestroy()
    {
        if (objectTransform != null)
            objectTransform.DOKill();
    }
}
