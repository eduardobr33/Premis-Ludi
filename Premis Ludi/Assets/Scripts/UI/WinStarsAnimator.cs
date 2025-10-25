using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class WinStarsAnimator : MonoBehaviour
{
    [SerializeField] private List<Image> emptyStarImages = new List<Image>();
    [SerializeField] private Image starPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private AudioSource audioSource1;
    [SerializeField] private AudioSource audioSource2;
    [SerializeField] private AudioSource audioSource3;
    [SerializeField] private AudioClip starSound;
    
    [Header("Animation Settings")]
    [SerializeField] private float startScale = 2.5f;
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private float delayBetweenStars = 0.2f;
    [SerializeField] private float shakeStrength = 30f;
    [SerializeField] private int shakeVibrato = 8;
    [SerializeField] private float shakeDuration = 0.4f;
    
    [Header("Sound Settings")]
    [SerializeField] private float basePitch = 0.8f;
    [SerializeField] private float pitchIncrement = 0.3f;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private int debugStarsToTest = 3;

    private int earnedStars = 0;
    private AudioSource[] audioSources;

    private void Start()
    {
        // Obtener estrellas ganadas del nivel actual
        int levelNum = 0;
        if (LevelManager.Instance != null && LevelManager.Instance.currentLevelData != null)
            levelNum = LevelManager.Instance.currentLevelData.levelNumber;
        
        earnedStars = SaveSystem.Instance != null ? SaveSystem.Instance.GetLevelStars(levelNum) : 0;
        
        // En debug mode, usar las estrellas configuradas
        if (debugMode)
        {
            earnedStars = debugStarsToTest;
            Debug.Log($"[WinStarsAnimator DEBUG] Probando {earnedStars} estrellas");
        }
        
        Debug.Log($"[WinStarsAnimator] Estrellas ganadas: {earnedStars}");
        
        // Crear AudioSources si no existen
        CreateAudioSourcesIfNeeded();
        
        // Animar estrellas después de la transición de nubes (0.5s)
        Invoke(nameof(PlayStarsAnimation), 0.5f);
    }

    private void CreateAudioSourcesIfNeeded()
    {
        if (audioSource1 == null)
        {
            audioSource1 = gameObject.AddComponent<AudioSource>();
            audioSource1.playOnAwake = false;
        }
        if (audioSource2 == null)
        {
            audioSource2 = gameObject.AddComponent<AudioSource>();
            audioSource2.playOnAwake = false;
        }
        if (audioSource3 == null)
        {
            audioSource3 = gameObject.AddComponent<AudioSource>();
            audioSource3.playOnAwake = false;
        }
        
        audioSources = new AudioSource[] { audioSource1, audioSource2, audioSource3 };
    }

    private void PlayStarsAnimation()
    {
        if (earnedStars == 0)
        {
            Debug.LogWarning("[WinStarsAnimator] No hay estrellas para animar");
            return;
        }
        
        Debug.Log($"[WinStarsAnimator] Iniciando animación de {earnedStars} estrellas");
        
        for (int i = 0; i < earnedStars; i++)
        {
            float delay = i * delayBetweenStars;
            AnimateStar(i, delay);
        }
    }

    private void AnimateStar(int index, float delay)
    {
        if (index >= emptyStarImages.Count)
        {
            Debug.LogWarning($"[WinStarsAnimator] Index {index} fuera de rango. Empty star images: {emptyStarImages.Count}");
            return;
        }

        Image targetStar = emptyStarImages[index];
        RectTransform targetRect = targetStar.GetComponent<RectTransform>();
        
        if (targetRect == null)
        {
            Debug.LogError($"[WinStarsAnimator] No se encontró RectTransform para estrella {index}");
            return;
        }
        
        Vector2 targetPos = targetRect.anchoredPosition;
        Vector2 targetSize = targetRect.sizeDelta;
        
        // Crear estrella animada
        if (starPrefab == null)
        {
            Debug.LogError("[WinStarsAnimator] Star prefab no asignado");
            return;
        }
        
        Image animatedStar = Instantiate(starPrefab, canvas.transform);
        RectTransform animatedRect = animatedStar.GetComponent<RectTransform>();
        
        // Configurar estrella animada
        // Posición inicial: arriba de la pantalla
        Vector2 startPos = new Vector2(targetPos.x, targetPos.y + 1000f);
        animatedRect.anchoredPosition = startPos;
        
        // Tamaño inicial: muy grande
        Vector2 startSize = targetSize * startScale;
        animatedRect.sizeDelta = startSize;
        
        // Color visible
        animatedStar.color = new Color(1, 1, 1, 1);
        
        // Asegurar que la estrella de fondo esté visible (ya ganada)
        targetStar.color = new Color(targetStar.color.r, targetStar.color.g, targetStar.color.b, 1f);

        Debug.Log($"[WinStarsAnimator] Animando estrella {index}: pos({targetPos.x}, {targetPos.y}) size({targetSize.x}, {targetSize.y}) delay {delay}s");

        // Secuencia de animación
        Sequence seq = DOTween.Sequence();
        
        // Movimiento: de arriba hacia la posición objetivo
        seq.Append(animatedRect.DOAnchorPos(targetPos, animationDuration)
            .SetEase(Ease.OutQuad));
        
        // Tamaño: de grande al tamaño objetivo (simultáneo con movimiento)
        seq.Join(animatedRect.DOSizeDelta(targetSize, animationDuration)
            .SetEase(Ease.OutQuad));
        
        // Callback al llegar a posición final: shake exagerado y sonido
        seq.AppendCallback(() =>
        {
            // Sonido al llegar
            PlayStarSound(index);
            
            // Shake exagerado en la posición final
            animatedRect.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato, 90f)
                .SetEase(Ease.OutQuad);
        });
        
        seq.SetDelay(delay);
    }

    private void PlayStarSound(int starIndex)
    {
        if (audioSources == null || starIndex >= audioSources.Length || starSound == null)
        {
            Debug.LogWarning($"[WinStarsAnimator] AudioSource o Sound no configurados para estrella {starIndex}");
            return;
        }
        
        AudioSource audioSource = audioSources[starIndex];
        if (audioSource == null)
        {
            Debug.LogWarning($"[WinStarsAnimator] AudioSource {starIndex} es null");
            return;
        }
        
        // Calcular pitch progresivo: primera estrella pitch bajo, tercera pitch alto
        float pitch = basePitch + (starIndex * pitchIncrement);
        
        Debug.Log($"[WinStarsAnimator] Reproduciendo sonido estrella {starIndex} con pitch {pitch}");
        
        // Reproducir sonido con el AudioSource correspondiente
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(starSound);
    }
}
