using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

public class PointPopup : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Action onComplete;

    private Vector3 targetPos;
    public float moveSpeed = 5f;
    public float fadeSpeed = 2f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(int points, Vector3 startWorldPos, RectTransform uiTarget, Action onCompleteCallback = null)
    {
        rectTransform = GetComponent<RectTransform>();
        onComplete = onCompleteCallback;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(startWorldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiTarget.root as RectTransform,
            screenPos,
            null,
            out Vector2 localStart
        );
        rectTransform.localPosition = localStart;
        targetPos = new Vector3(uiTarget.position.x + 75, uiTarget.position.y, uiTarget.position.z);

        pointsText.text = $"+{points}";
        
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        while (Vector3.Distance(rectTransform.position, targetPos) > 10f)
        {
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetPos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        onComplete?.Invoke();
        Destroy(gameObject);
    }
}