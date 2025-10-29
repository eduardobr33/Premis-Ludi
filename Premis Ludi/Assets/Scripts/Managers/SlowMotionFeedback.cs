using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SlowMotionFeedback : MonoBehaviour
{
    public static SlowMotionFeedback Instance { get; private set; }

    [Header("Post Processing")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private float saturationAmount = -25f;
    [SerializeField] private float chromaticAberrationIntensity = 0.25f;
    [SerializeField] private float vignetteIntensity = 0.2f;

    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private float originalSaturation = 0f;
    private float originalChromaticIntensity = 0f;
    private float originalVignetteIntensity = 0f;
    private Coroutine slowMotionCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (postProcessVolume == null)
            postProcessVolume = FindObjectOfType<Volume>();

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
            postProcessVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
            postProcessVolume.profile.TryGet<Vignette>(out vignette);

            if (colorAdjustments != null)
                originalSaturation = colorAdjustments.saturation.value;
            if (chromaticAberration != null)
                originalChromaticIntensity = chromaticAberration.intensity.value;
            if (vignette != null)
                originalVignetteIntensity = vignette.intensity.value;
        }
    }

    public void StartSlowMotionFeedback(float duration)
    {
        if (slowMotionCoroutine != null)
            StopCoroutine(slowMotionCoroutine);

        slowMotionCoroutine = StartCoroutine(SlowMotionFeedbackCoroutine(duration));
    }

    public void StopSlowMotionFeedback()
    {
        if (slowMotionCoroutine != null)
            StopCoroutine(slowMotionCoroutine);

        StartCoroutine(ResetPostProcessingCoroutine());
    }

    private IEnumerator SlowMotionFeedbackCoroutine(float duration)
    {
        yield return StartCoroutine(ApplySlowMotionEffect());
        yield return new WaitForSeconds(duration);
        yield return StartCoroutine(ResetPostProcessingCoroutine());
    }

    private IEnumerator ApplySlowMotionEffect()
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionDuration;

            if (colorAdjustments != null)
                colorAdjustments.saturation.value = Mathf.Lerp(originalSaturation, originalSaturation + saturationAmount, progress);

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(originalChromaticIntensity, chromaticAberrationIntensity, progress);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(originalVignetteIntensity, vignetteIntensity, progress);

            yield return null;
        }
    }

    private IEnumerator ResetPostProcessingCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionDuration;

            if (colorAdjustments != null)
                colorAdjustments.saturation.value = Mathf.Lerp(originalSaturation + saturationAmount, originalSaturation, progress);

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberrationIntensity, originalChromaticIntensity, progress);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(vignetteIntensity, originalVignetteIntensity, progress);

            yield return null;
        }
    }
}
