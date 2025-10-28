using UnityEngine;
using DG.Tweening;

public class FloatingAnimation : MonoBehaviour
{
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatDuration = 2f;

    private void Start()
    {
        StartFloating();
    }

    private void StartFloating()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 floatPos = startPos + Vector3.up * floatHeight;

        transform.DOLocalMove(floatPos, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
