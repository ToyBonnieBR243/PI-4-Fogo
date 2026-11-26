using System.Collections.Generic;
using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [Header("Damage & Lifetime")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.8f;
    [SerializeField] private GameObject explosionObject; // Arraste o filho 'Explosão' aqui
    [SerializeField] private float explosionDuration = 0.25f;

    // Se o sprite original da explosão já tiver um tamanho base em unidades do mundo,
    // ajuste esse multiplicador caso precise ficar maior ou menor que o raio exato.
    [SerializeField] private float visualScaleMultiplier = 1f;

    private bool hasExploded = false;

    private void Start()
    {
        if (explosionObject != null)
        {
            explosionObject.SetActive(false);
        }

        Destroy(gameObject, lifetime);
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
        Vector3 explosionPos = transform.position;

        if (explosionObject != null)
        {
            explosionObject.transform.SetParent(null);
            explosionObject.transform.position = explosionPos;

            // Diâmetro = raio * 2 (ex: raio 1.8 -> diâmetro 3.6)
            float diameter = explosionRadius * 2f * visualScaleMultiplier;
            explosionObject.transform.localScale = new Vector3(diameter, diameter, 1f);

            explosionObject.SetActive(true);
            Destroy(explosionObject, explosionDuration);
        }

        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPos, explosionRadius);

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
                enemy.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}