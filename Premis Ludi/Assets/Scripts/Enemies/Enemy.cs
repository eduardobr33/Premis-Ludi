using UnityEngine;
using TMPro;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Build.Content;
#endif

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public Vector3 maxScale = new Vector3(2f, 2f, 2f);
    public float enemySpeed = 1f;
    public float attackSpeed = 1f;
    public int health = 1;
    public float nextOperationDelay = 0.5f;

    [Header("Referecnes")]
    public TextMeshPro textOp;
    public SpriteRenderer spriteRenderer;

    public int correctAnswer; // For the abilities
    private bool isActive = true;
    private bool isAproaching = true;
    private static float minScale = 0.3f;
    private Coroutine damageCoroutine;

    private bool isPaused = false;

    public enum EnemyType { Ogre, Crab }
    public EnemyType enemyType = EnemyType.Ogre;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (!isActive) return;

        if (isAproaching && !isPaused)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, maxScale, enemySpeed * Time.deltaTime);

            if(enemyType == EnemyType.Crab)
            {
                float horizontalOffset = Mathf.Sin(Time.time * 5f) * 1f;
                transform.position = new Vector3(transform.position.x + horizontalOffset, transform.position.y, transform.position.z);
            }   
        }

        if (transform.localScale.x >= maxScale.x && damageCoroutine == null)
        {
            isAproaching = false;
            damageCoroutine = StartCoroutine(DamageOverTime());
        }
    }

    public void Setup(string operation, int answer, bool newEnemy)
    {
        textOp.text = operation;
        Debug.Log("Operation: " + textOp.text);
        correctAnswer = answer;
        Debug.Log("Answer: " + correctAnswer);
        if (newEnemy) transform.localScale = Vector3.one * minScale;
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
            Player.Instance.TakeDamage();
            yield return new WaitForSeconds(attackSpeed);
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
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;

        transform.localScale = new Vector3(transform.localScale.x - 0.3f, transform.localScale.y - 0.3f, transform.localScale.z - 0.3f);
        isAproaching = true;

        yield return new WaitForSeconds(0.2f);

        spriteRenderer.color = originalColor;
    }

    private void Kill(bool instaKill)
    {
        isActive = false;
        GameManager.Instance.EnemyDefeated(instaKill); // --> Esto se tendrá q cambiar según el lvl :p

        //Here goes the death animation
        Destroy(gameObject);
    }

    public void PauseScaling()
    {
        isPaused = true;
    }
    public void ResumeScaling()
    {
        isPaused = false;
    }
}
