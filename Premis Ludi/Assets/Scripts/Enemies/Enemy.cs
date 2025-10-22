using UnityEngine;
using TMPro;
using System.Collections;
using UnityEditor.Tilemaps;

#if UNITY_EDITOR
using UnityEditor.Build.Content;
#endif

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public Vector3 maxScale = new Vector3(1f, 1f, 1f);
    public float enemySpeed = 1f;
    public float attackSpeed = 1f;
    public int health = 1;
    public float nextOperationDelay = 0.5f;

    [Header("Referecnes")]
    public TextMeshPro textOp;
    public SpriteRenderer spriteRenderer;
    public Vector3 startPos;

    public int correctAnswer; // For the abilities
    private bool isActive = true;
    private bool isAproaching = true;
    private Vector3 minScale = new Vector3(0.3f, 0.3f, 0.3f);
    private Coroutine damageCoroutine;

    private bool isPaused = false;

    public enum EnemyType { Crab, Bush, Boss }
    public EnemyType enemyType = EnemyType.Bush;

    private float animationSpeed = 1f;
    private Animator animator;
    private float startTime;
    private bool facingRight = true;
    private float growDuration = 12f;

    [Header("Animation")]
    public float attackAnimaDuration = 2f;
    public float idleAnimDuration = 1f;
    public float damageAnimDuration = 1f;
    public float flashDamageAnim = 0.1f;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        startTime = Time.time;

        AdjustScale();

        transform.localScale = minScale;
    }

    private void Update()
    {
        if (!isActive) return;

        if (isAproaching && !isPaused)
        {
            float t = Mathf.Clamp01((Time.time - startTime) / growDuration);
            float smoothT = 1f - Mathf.Exp(-1f * t);
            transform.localScale = Vector3.Lerp(minScale, maxScale, Mathf.Clamp01(smoothT));

            if (enemyType == EnemyType.Crab)
            {
                //float elapsed = Time.time - startTime;
                //float horizontalOffset = Mathf.Sin(elapsed * 2f * Mathf.PI / 1f) * 0.5f;

                //transform.position = new Vector3(startPos.x + horizontalOffset, transform.position.y, transform.position.z);

                // if (horizontalOffset > 0 && !facingRight) Flip(true);
                // else if (horizontalOffset < 0 && facingRight) Flip(false);
            }

            //animator.SetTrigger("Walking");

            if (t >= 1f && damageCoroutine == null)
            {
                isAproaching = false;
                damageCoroutine = StartCoroutine(DamageOverTime());
            }
        }
    }
    
    private void Flip(bool toRight)
    {
        facingRight = toRight;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (toRight ? 1 : -1);
        transform.localScale = localScale;
    }

    public void Setup(string operation, int answer, bool newEnemy)
    {
        textOp.text = operation;
        Debug.Log("Operation: " + textOp.text);
        correctAnswer = answer;
        Debug.Log("Answer: " + correctAnswer);
    }

    private void GenerateNewOperation()
    {
        var (operation, result) = MathGenerator.GenerateOperation(GameManager.Instance.currentLevelData);
        Setup(operation, result, false);

        GameManager.Instance.SetupGestureRecognizer(operation, result);
    }

    private IEnumerator DamageOverTime()
    {
        while (isActive)
        {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(attackAnimaDuration);

            animator.ResetTrigger("Attack");
            animator.SetTrigger("Idle");
            yield return new WaitForSeconds(idleAnimDuration);
        }
    }

    public void TakeDamage(bool instaKill)
    {
        StartCoroutine(FlashDamage());

        if (instaKill) health -= 99;
        else health -= 1;

        if (health > 0) Invoke(nameof(GenerateNewOperation), nextOperationDelay);
        else Kill(instaKill);
    }

    private IEnumerator FlashDamage()
    {
        animator.SetTrigger("Damage");

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) yield break;

        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) originalColors[i] = renderers[i].color;

        foreach (var sr in renderers) sr.color = Color.red;

        yield return new WaitForSeconds(flashDamageAnim);

        for (int i = 0; i < renderers.Length; i++) renderers[i].color = originalColors[i];

        yield return new WaitForSeconds(damageAnimDuration - flashDamageAnim);
    }

    private void Kill(bool instaKill)
    {
        isActive = false;
        GameManager.Instance.EnemyDefeated(instaKill); // --> Esto se tendrá q cambiar según el lvl :p

        //Here goes the death animation
        Destroy(gameObject);
    }

    public void DealDamage()
    {
        if (isActive) Player.Instance.TakeDamage();
    }

    public void PauseScaling()
    {
        isPaused = true;
    }

    public void ResumeScaling()
    {
        isPaused = false;
    }

    private void AdjustScale()
    {
        float worldHeight = Camera.main.orthographicSize * 2f;
        float worldWidth = worldHeight * Camera.main.aspect;

        // Percentage of screen
        float maxRelativeHeight = 0.2f;     // 10% high
        float maxRelativeWidth = 0.1f;     // 8% width

        float targetMaxHeight = worldHeight * maxRelativeHeight;

        // Sprites size
        float spriteHeight = 1f;
        float spriteWidth = 1f;

        // Maximum scale by height and width
        float maxScaleByHeight = targetMaxHeight / spriteHeight;
        float maxScaleByWidth = worldWidth * maxRelativeWidth / spriteWidth;
        float maxScaleFactor = Mathf.Min(maxScaleByHeight, maxScaleByWidth);

        // Nevel would be bigger than 1 (its high enough by default)
        maxScaleFactor = Mathf.Min(maxScaleFactor, 1f);

        // Minimum scale is always 1% of maximum
        float minScaleFactor = maxScaleFactor * 0.01f;

        maxScale = Vector3.one * maxScaleFactor;
        minScale = Vector3.one * minScaleFactor;

        Debug.Log($"Enemy Scale: max={maxScale}, min={minScale}");
    }
}
