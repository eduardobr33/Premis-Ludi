using UnityEngine;
using TMPro;
using System.Collections;
// using UnityEditor.Tilemaps;

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
    private bool isAttacking = false;
    private bool isTakingDamage = false;
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

    private enum State { Approaching, Idle, Attacking, Damaged }
    private State currentState = State.Approaching;
    private float attackTimer;
    private float attackInterval = 4f;
    public int maxHealth;

    private Coroutine stateCoroutine;

    private void Start()
    {
        health = maxHealth;
        animator = GetComponentInChildren<Animator>();
        startTime = Time.time;
        startPos = transform.localPosition;

        AdjustScale();
        transform.localScale = minScale;

        currentState = State.Approaching;
        animator.SetTrigger("Walking");
    }

    private void Update()
    {
        if (!isActive || isPaused) return;

        switch (currentState)
        {
            case State.Approaching:
                HandleApproaching();
                break;
            case State.Idle:
                HandleIdle();
                break;
        }
    }

    private void HandleApproaching()
    {
        float t = Mathf.Clamp01((Time.time - startTime) / growDuration);
        float smoothT = 1f - Mathf.Exp(-t);
        transform.localScale = Vector3.Lerp(minScale, maxScale, smoothT);
        Vector3 targetPos = new Vector3(startPos.x, startPos.y - 2f, startPos.z);
        transform.localPosition = Vector3.Lerp(startPos, targetPos, smoothT);

        if(t >= 1f)
        {
            currentState = State.Idle;
            animator.ResetTrigger("Walking");
            animator.SetTrigger("Idle");
            attackTimer = attackInterval;
        }
    }

    private void HandleIdle()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (currentState == State.Damaged) return;

        currentState = State.Attacking;
        animator.ResetTrigger("Idle");
        animator.SetTrigger("Attack");

        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        stateCoroutine = StartCoroutine(AttackRoutine());
    }
    
    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(attackAnimaDuration);
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Idle");
        attackTimer = attackInterval;
        currentState = State.Idle;
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
            if (!isAttacking && !isTakingDamage)
            {
                isAttacking = true;
                animator.ResetTrigger("Idle");
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");

                yield return new WaitForSeconds(attackAnimaDuration);

                animator.ResetTrigger("Attack");
                animator.SetTrigger("Idle");

                yield return new WaitForSeconds(idleAnimDuration * 4);
                isAttacking = false;
            }
            else
            {
                yield return null; // Security check
            }
        }
    }

    public void TakeDamage(bool instaKill)
    {
        if (!isActive) return;

        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        StartCoroutine(FlashDamage());

        health -= instaKill ? 99 : 1;
        if (enemyType == EnemyType.Boss) UIManager.Instance.UpdateBossHealth(health, maxHealth);

        if (health > 0) Invoke(nameof(GenerateNewOperation), nextOperationDelay);
        else Kill(instaKill);

        attackTimer = attackInterval;
        currentState = State.Idle;
    }

    private IEnumerator FlashDamage()
    {
        currentState = State.Damaged;
        animator.SetTrigger("Damage");

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) originalColors[i] = renderers[i].color;
        foreach (var sr in renderers) sr.color = Color.red;

        yield return new WaitForSeconds(flashDamageAnim);
        for (int i = 0; i < renderers.Length; i++) renderers[i].color = originalColors[i];

        yield return new WaitForSeconds(damageAnimDuration - flashDamageAnim);

        animator.ResetTrigger("Damage");
        animator.SetTrigger("Idle");
        currentState = State.Idle;
    }

    private void Kill(bool instaKill)
    {
        isActive = false;
        GameManager.Instance.EnemyDefeated(instaKill);

        //Here goes the death animation
        StopAllCoroutines();
        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        float fadeDuration = 0.16f;
        float elapsed = 0f;

        // Fade
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            foreach (var sr in renderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }

            yield return null;
        }

        // Destroy enemy
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
        float maxRelativeHeight = 0.2f;                         // 40% high
        float maxRelativeWidth = IsEnemyCrab() ? 0.1f : 0.2f;   // 20% width || 10% if crab enemy

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

    private bool IsEnemyCrab()
    {
        if (enemyType == EnemyType.Crab) return true;
        else return false;
    }
}
