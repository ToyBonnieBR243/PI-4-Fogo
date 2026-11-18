using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        // Destroi o projétil após alguns segundos caso não acerte nada
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se colidiu com um objeto que tem o script Enemy
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Destroi a bala ao acertar
        }
    }
}