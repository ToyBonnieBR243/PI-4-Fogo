using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FireProjectile : MonoBehaviour
{
    [Header("Damage & Lifetime")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.8f;
    [SerializeField] private GameObject explosionObject;
    [SerializeField] private float explosionDuration = 0.25f;
    [SerializeField] private float visualScaleMultiplier = 1f;

    private bool hasExploded = false;
    public static int bonusDamage = 0;
    public static float bonusExplosionRadius = 0f;

    private void Start()
    {
        if (explosionObject != null)
        {
            explosionObject.SetActive(false);
        }

        Invoke(nameof(ReleaseSelf), lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy == null)
        {
            hitEnemy = collision.GetComponentInParent<Enemy>();
        }

        if (hitEnemy != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;
        CancelInvoke(nameof(ReleaseSelf));

        Vector3 explosionPos = transform.position;
        float finalRadius = Mathf.Max(0.1f, explosionRadius + bonusExplosionRadius);
        int finalDamage = Mathf.Max(1, damage + bonusDamage);

        if (explosionObject != null)
        {
            explosionObject.transform.SetParent(null);
            explosionObject.transform.position = explosionPos;

            float diameter = finalRadius * 2f * visualScaleMultiplier;
            explosionObject.transform.localScale = new Vector3(diameter, diameter, 1f);

            explosionObject.SetActive(true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartCoroutine(HideAndDestroyExplosion(explosionObject, explosionDuration));
            }
            else
            {
                Destroy(explosionObject, explosionDuration);
            }
        }

        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPos, finalRadius);

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = hit.GetComponentInParent<Enemy>();
            }

            if (enemy != null && !damagedEnemies.Contains(enemy))
            {
                damagedEnemies.Add(enemy);
                enemy.TakeDamage(finalDamage);
            }
        }

        ReleaseSelf();
    }

    private IEnumerator HideAndDestroyExplosion(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null)
        {
            target.SetActive(false);
            Destroy(target);
        }
    }

    private void ReleaseSelf()
    {
        if (!Addressables.ReleaseInstance(gameObject))
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        float previewRadius = Mathf.Max(0.1f, explosionRadius + bonusExplosionRadius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, previewRadius);
    }
}