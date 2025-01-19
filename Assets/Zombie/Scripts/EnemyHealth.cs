using UnityEngine;

[RequireComponent(typeof(Zombie))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 5;
    private float currentHealth;
    private IEnemy enemy;

    private void Awake()
    {
        currentHealth = maxHealth;
        enemy = GetComponent<IEnemy>();
    }

    public void TakeDamage(float damage, GameObject damager)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) enemy.Die();
    }
}



