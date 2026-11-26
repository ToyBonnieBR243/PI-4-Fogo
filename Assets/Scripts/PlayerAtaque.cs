using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private string fireProjectileAddress = "BolaFogo_Prefab";
    [SerializeField] private GameObject fireProjectilePrefab; // Fallback opcional
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackRange = 20f;
    [SerializeField] private float projectileSpeed = 8f;

    public float AttackInterval => attackInterval;

    private float attackTimer;

    public void ReduceCooldown(float amount)
    {
        attackInterval = Mathf.Max(0.15f, attackInterval - amount);
    }

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

        if (nearestEnemy == null) return;

        Vector2 direction = (nearestEnemy.position - transform.position).normalized;

        if (!string.IsNullOrEmpty(fireProjectileAddress))
        {
            Addressables.InstantiateAsync(fireProjectileAddress, transform.position, Quaternion.identity).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    ApplyVelocity(handle.Result, direction);
                }
            };
        }
        else if (fireProjectilePrefab != null)
        {
            GameObject proj = Instantiate(fireProjectilePrefab, transform.position, Quaternion.identity);
            ApplyVelocity(proj, direction);
        }
    }

    private void ApplyVelocity(GameObject proj, Vector2 direction)
    {
        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * projectileSpeed;
        }
    }
}