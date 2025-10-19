using UnityEngine;

public class AnimationEventsProxy : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    public void DealDamage()
    {
        if (enemy != null) enemy.DealDamage();
    }
}
