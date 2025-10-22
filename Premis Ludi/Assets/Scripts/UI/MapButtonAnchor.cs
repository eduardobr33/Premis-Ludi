using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class MapButtonAnchor : UIBehaviour
{
    [SerializeField] private Image mapImage; // Imagen con Preserve Aspect
    [SerializeField, Range(0,1)] public float horizontalPosition = 0.5f; // Posición horizontal (0-1)
    [SerializeField, Range(0,1)] public float verticalPosition = 0.5f; // Posición vertical (0-1)
    [SerializeField] public float sizeScale = 1f; // Escala de tamaño (1 = 100%)

    private RectTransform buttonRect;
    private Vector2 originalSize;
    private Vector2 lastPosition;
    private float lastSizeScale;
    private float lastHeight;

    protected override void Awake()
    {
        base.Awake();
        buttonRect = GetComponent<RectTransform>();
        originalSize = buttonRect.sizeDelta;
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        UpdateButtonPosition();
        UpdateButtonSize();
    }

#if UNITY_EDITOR
    // Para que también actualice en modo editor
    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (!buttonRect)
                buttonRect = GetComponent<RectTransform>();
            
            Vector2 currentPos = new Vector2(horizontalPosition, verticalPosition);
            if (currentPos != lastPosition || lastSizeScale != sizeScale)
            {
                UpdateButtonPosition();
                UpdateButtonSize();
                lastPosition = currentPos;
                lastSizeScale = sizeScale;
            }
        }
    }
#endif

    private void UpdateButtonPosition()
    {
        if (!mapImage || !buttonRect) return;
        
        Rect visibleRect = GetVisibleRect(mapImage);
        buttonRect.anchoredPosition = new Vector2(
            Mathf.Lerp(visibleRect.xMin, visibleRect.xMax, horizontalPosition),
            Mathf.Lerp(visibleRect.yMin, visibleRect.yMax, verticalPosition)
        );
    }

    private void UpdateButtonSize()
    {
        if (!buttonRect || !mapImage) return;

        Rect visibleRect = GetVisibleRect(mapImage);
        float visibleHeight = visibleRect.height;
        
        if (Mathf.Approximately(visibleHeight, lastHeight)) return;
        
        float heightScale = visibleHeight > 0 ? visibleHeight / originalSize.y : 1f;
        buttonRect.sizeDelta = originalSize * sizeScale * heightScale;
        lastHeight = visibleHeight;
    }

    private Rect GetVisibleRect(Image image)
    {
        Rect rect = image.rectTransform.rect;
        Sprite sprite = image.sprite;
        if (!sprite) return rect;

        float imageRatio = sprite.rect.width / sprite.rect.height;
        float rectRatio = rect.width / rect.height;

        if (imageRatio > rectRatio)
        {
            float height = rect.width / imageRatio;
            float offsetY = (rect.height - height) * 0.5f;
            return new Rect(rect.xMin, rect.yMin + offsetY, rect.width, height);
        }
        
        float width = rect.height * imageRatio;
        float offsetX = (rect.width - width) * 0.5f;
        return new Rect(rect.xMin + offsetX, rect.yMin, width, rect.height);
    }

#if UNITY_EDITOR
    [ContextMenu("Guardar posición actual como normalizada")]
    private void SaveNormalized()
    {
        if (!mapImage || !buttonRect) return;
        
        Rect visibleRect = GetVisibleRect(mapImage);
        Vector2 pos = buttonRect.anchoredPosition;
        horizontalPosition = Mathf.InverseLerp(visibleRect.xMin, visibleRect.xMax, pos.x);
        verticalPosition = Mathf.InverseLerp(visibleRect.yMin, visibleRect.yMax, pos.y);
    }
#endif
}
