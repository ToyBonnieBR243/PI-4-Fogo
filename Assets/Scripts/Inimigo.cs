using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float moveSpeed = 3f;

    private int currentHealth;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    private EnemySpawner spawner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;

        spawner = Object.FindFirstObjectByType<EnemySpawner>();

        GameObject player = GameObject.Find("Foguin");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    public void InitializeHealth(int extraHealth)
    {
        currentHealth += extraHealth;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        // Direção base até o player
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // Evita amontoamento empurrando levemente para longe de outros inimigos próximos
        Vector2 separation = Vector2.zero;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.6f);
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Vector2 pushDir = (transform.position - col.transform.position);
                separation += pushDir.normalized / pushDir.magnitude;
            }
        }

        // Combina o movimento em direção ao player com o empurrão dos colegas
        Vector2 finalMoveDir = (direction + separation * 0.5f).normalized;
        rb.MovePosition(rb.position + finalMoveDir * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Efeito visual de piscar em branco
        if (spriteRenderer != null && !isFlashing)
        {
            StartCoroutine(FlashRedRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRedRoutine()
    {
        isFlashing = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f); // Tempo que fica branco
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    private void Die()
    {
        // Avisa o Spawner que um inimigo foi derrotado para contar para a próxima wave
        if (spawner != null)
        {
            spawner.RegisterEnemyDefeated();
        }

        // Aqui você pode adicionar efeitos de morte, som ou drop de XP futuramente
        Destroy(gameObject);
    }
}