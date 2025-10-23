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
        
        Canvas canvas = uiTarget.root.GetComponent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiTarget.root as RectTransform,
            screenPos,
            canvas.worldCamera,
            out Vector2 localStart
        );
        rectTransform.localPosition = localStart;
        
        // Target posición en local coordinates
        targetPos = new Vector3(uiTarget.localPosition.x, uiTarget.localPosition.y + 20, 0);

        pointsText.text = $"+{points}";
        
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        while (Vector3.Distance(rectTransform.localPosition, targetPos) > 10f)
        {
            rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPos, Time.deltaTime * moveSpeed);
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