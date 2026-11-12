using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject fireProjectilePrefab; // Prefab do tiro de fogo
    [SerializeField] private float attackInterval = 1.5f;     // Tempo entre os disparos
    [SerializeField] private float attackRange = 10f;         // Raio de alcance para buscar inimigos
    [SerializeField] private float projectileSpeed = 8f;

    private float attackTimer;

    private void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            ShootNearestEnemy();
        }
    }

    private void ShootNearestEnemy()
    {
        // Encontra todos os objetos com o script Enemy na cena
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        if (enemies.Length == 0) return;

        Transform nearestEnemy = null;
        float shortestDistance = attackRange;

        foreach (var enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        // Se encontrou um inimigo no alcance, dispara o projétil
        if (nearestEnemy != null && fireProjectilePrefab != null)
        {
            GameObject proj = Instantiate(fireProjectilePrefab, transform.position, Quaternion.identity);
            Vector2 direction = (nearestEnemy.position - transform.position).normalized;

            // Se o projétil tiver um Rigidbody2D, empurra ele na direção
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }
        }
    }
}