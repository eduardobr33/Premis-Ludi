using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class CloudGenerator : MonoBehaviour
{
    [Header("Cloud Settings")]
    [SerializeField] private Image cloudPrefab;
    [SerializeField] private Canvas uiCanvas;
    
    [Header("Generation")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float moveDistance = 1300f;
    [SerializeField] private float minYRange = -100f;
    [SerializeField] private float maxYRange = 100f;
    [SerializeField] private float cloudLifetime = 15f;
    
    [Header("Cloud Behavior")]
    [SerializeField] private float minSize = 1f;
    [SerializeField] private float maxSize = 2f;
    [SerializeField] private float moveDuration = 8f;
    [SerializeField] private float floatAmount = 20f;
    [SerializeField] private float floatSpeed = 2f;
    
    private float spawnTimer = 0f;

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval)
        {
            SpawnCloud();
            spawnTimer = 0f;
        }
    }

    private void SpawnCloud()
    {
        if (cloudPrefab == null || uiCanvas == null) return;

        Image newCloud = Instantiate(cloudPrefab, uiCanvas.transform);
        RectTransform cloudRect = newCloud.GetComponent<RectTransform>();
        
        RectTransform generatorRect = GetComponent<RectTransform>();
        float randomY = Random.Range(minYRange, maxYRange);
        cloudRect.anchoredPosition = new Vector2(generatorRect.anchoredPosition.x, generatorRect.anchoredPosition.y + randomY);
        
        float randomSize = Random.Range(minSize, maxSize);
        cloudRect.localScale = new Vector3(randomSize, randomSize, 1f);
        
        AnimateCloud(cloudRect);
        
        // Destruir después de 15 segundos
        Destroy(cloudRect.gameObject, cloudLifetime);
    }

    private void AnimateCloud(RectTransform cloudRect)
    {
        Vector3 startPos = cloudRect.transform.localPosition;
        Vector3 endPos = startPos + new Vector3(moveDistance, 0, 0);
        
        Sequence sequence = DOTween.Sequence();
        
        sequence.Join(cloudRect.transform.DOLocalMove(endPos, moveDuration).SetEase(Ease.Linear));
        sequence.Join(
            cloudRect.transform.DOLocalMoveY(startPos.y + floatAmount, floatSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
        );
    }

    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }

    public void PauseGeneration()
    {
        enabled = false;
    }

    public void ResumeGeneration()
    {
        enabled = true;
    }

    public void ClearAllClouds()
    {
        foreach (Transform child in uiCanvas.transform)
        {
            if (child.GetComponent<Image>() == cloudPrefab)
            {
                child.DOKill();
                Destroy(child.gameObject);
            }
        }
    }
}
