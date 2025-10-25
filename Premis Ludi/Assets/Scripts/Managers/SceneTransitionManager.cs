using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Image cloudPrefab;
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private float minCloudSize = 50f;
    [SerializeField] private float maxCloudSize = 150f;
    [SerializeField] private float transitionDuration = 0.8f;

    private List<Image> activeClouds = new List<Image>();
    private bool isTransitioning = false;
    private int cloudsPerRow;
    private int cloudsPerCol;
    private float avgCloudSize;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Configurar capacidad de DOTween
        DOTween.SetTweensCapacity(2000, 50);
    }

    private void Start()
    {
        if (transitionCanvas == null)
        {
            GameObject canvasGO = new GameObject("TransitionCanvas");
            canvasGO.transform.SetParent(transform);
            transitionCanvas = canvasGO.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 2560); // 9:16

            GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();
        }

        if (cloudPrefab == null)
        {
            CreateCloudPrefab();
        }

        CalculateGridSize();
    }

    private void CalculateGridSize()
    {
        RectTransform canvasRect = transitionCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        avgCloudSize = (minCloudSize + maxCloudSize) / 2f;
        // Optimizado para mobile: sin huecos (0.3x para cobertura completa)
        cloudsPerRow = Mathf.CeilToInt(canvasWidth / (avgCloudSize * 0.3f)) + 2;
        cloudsPerCol = Mathf.CeilToInt(canvasHeight / (avgCloudSize * 0.3f)) + 2;
    }

    private void CreateCloudPrefab()
    {
        GameObject cloudGO = new GameObject("CloudPrefab");
        Image image = cloudGO.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 1);
        
        RectTransform rect = cloudGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(avgCloudSize, avgCloudSize);

        cloudPrefab = image;
        cloudGO.SetActive(false);
    }

    public void PlayTransitionOut(System.Action onComplete = null)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        SpawnCloudsAndAnimateIn(() =>
        {
            onComplete?.Invoke();
            isTransitioning = false;
        });
    }

    public void PlayTransitionIn()
    {
        PlayTransitionInWithDelay();
    }

    private void PlayTransitionInWithDelay()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        AnimateCloundsOut(() =>
        {
            ClearClouds();
            isTransitioning = false;
        });
    }

    private void SpawnCloudsAndAnimateIn(System.Action onComplete)
    {
        ClearClouds();

        RectTransform canvasRect = transitionCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        Vector2 center = Vector2.zero;
        float maxDistance = Vector2.Distance(new Vector2(canvasWidth / 2, canvasHeight / 2), Vector2.zero);

        int totalClouds = cloudsPerRow * cloudsPerCol;
        int completedCount = 0;

        for (int x = 0; x < cloudsPerRow; x++)
        {
            for (int y = 0; y < cloudsPerCol; y++)
            {
                Image cloudImage = Instantiate(cloudPrefab, transitionCanvas.transform);
                cloudImage.gameObject.SetActive(true);

                RectTransform cloudRect = cloudImage.GetComponent<RectTransform>();
                
                float randomSize = Random.Range(minCloudSize, maxCloudSize);
                cloudRect.sizeDelta = new Vector2(randomSize, randomSize);
                
                float gridSpacing = avgCloudSize * 0.3f;
                float gridX = (x - cloudsPerRow / 2f) * gridSpacing + gridSpacing / 2f;
                float gridY = (y - cloudsPerCol / 2f) * gridSpacing + gridSpacing / 2f;
                
                float randomOffsetX = Random.Range(-gridSpacing * 0.3f, gridSpacing * 0.3f);
                float randomOffsetY = Random.Range(-gridSpacing * 0.3f, gridSpacing * 0.3f);
                
                Vector2 finalPos = new Vector2(gridX + randomOffsetX, gridY + randomOffsetY);
                
                Vector2 direction = (finalPos - center).normalized;
                Vector2 startPos = direction * (maxDistance + maxCloudSize);

                cloudRect.anchoredPosition = startPos;
                cloudImage.color = new Color(1, 1, 1, 0);
                
                float distance = Vector2.Distance(finalPos, center);
                float delay = (distance / maxDistance) * 0.2f;

                activeClouds.Add(cloudImage);

                cloudImage.DOFade(1f, transitionDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.InOutQuad);
                    
                cloudRect.DOAnchorPos(finalPos, transitionDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() =>
                    {
                        completedCount++;
                        if (completedCount == totalClouds)
                            onComplete?.Invoke();
                    });
            }
        }
    }

    private void AnimateCloundsOut(System.Action onComplete)
    {
        if (activeClouds.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        RectTransform canvasRect = transitionCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        Vector2 center = Vector2.zero;
        float maxDistance = Vector2.Distance(new Vector2(canvasWidth / 2, canvasHeight / 2), Vector2.zero);

        int completedCount = 0;
        int totalClouds = activeClouds.Count;

        foreach (Image cloudImage in activeClouds)
        {
            RectTransform cloudRect = cloudImage.GetComponent<RectTransform>();
            Vector2 currentPos = cloudRect.anchoredPosition;
            
            Vector2 direction = (currentPos - center).normalized;
            Vector2 endPos = direction * (maxDistance + maxCloudSize);

            float distance = Vector2.Distance(currentPos, center);
            float delay = (1f - distance / maxDistance) * 0.2f;

            cloudImage.DOFade(0f, transitionDuration)
                .SetDelay(delay)
                .SetEase(Ease.InOutQuad);
                
            cloudRect.DOAnchorPos(endPos, transitionDuration)
                .SetDelay(delay)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    completedCount++;
                    if (completedCount == totalClouds)
                        onComplete?.Invoke();
                });
        }
    }

    private void ClearClouds()
    {
        foreach (Image cloud in activeClouds)
        {
            if (cloud != null)
                Destroy(cloud.gameObject);
        }
        activeClouds.Clear();
    }

    private void OnDestroy()
    {
        ClearClouds();
    }
}
