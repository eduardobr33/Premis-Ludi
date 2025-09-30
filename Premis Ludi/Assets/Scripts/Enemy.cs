using UnityEngine;
using TMPro;
using UnityEditor.Build.Content;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public TextMeshPro textOp;
    public static Vector3 maxScale = new Vector3(2f, 2f, 2f);
    public float enemySpeed = 5f;
    public float attackSpeed = 1f;

    public int correctAnswer;

    private bool isActive = true;
    private bool isAproaching = true;
    private static float minScale = 0.3f;
    private float growSpeed = (maxScale.x - minScale) / 5f;
    private int health = 1;
    private Coroutine damageCoroutine;

    void Update()
    {
        if (!isActive) return;

        if (isAproaching) transform.localScale = Vector3.MoveTowards(transform.localScale, maxScale, growSpeed * Time.deltaTime);
        
        if (transform.localScale.x >= maxScale.x)
        {
            isAproaching = false;
            damageCoroutine = StartCoroutine(DamageOverTime());
        }
    }

    public void Setup(string operation, int answer)
    {
        textOp.text = operation;
        correctAnswer = answer;
        transform.localScale = Vector3.one * minScale;
    }

    private IEnumerator DamageOverTime()
    {
        while (isActive)
        {
            GameManager.Instance.PlayerTakeDamage();
            yield return new WaitForSeconds(attackSpeed);
        }
    }

    public void TakeDamage()
    {
        health -= 1;

        if (health <= 0) Kill();
    }

    private void Kill()
    {
        isActive = false;

        //Here goes the death animation
        Destroy(gameObject);
    }
}
