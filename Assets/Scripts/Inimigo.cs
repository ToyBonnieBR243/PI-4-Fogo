using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Contact Damage")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime = -999f;

    [Header("Loot")]
    [SerializeField] private string xpGemAddress = "PontoXP_Prefab";
    [SerializeField] private GameObject xpGemPrefab;

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
                if (pushDir.sqrMagnitude > 0.0001f)
                {
                    separation += pushDir.normalized / pushDir.magnitude;
                }
            }
        }

        // Combina o movimento em direção ao player com o empurrão dos colegas
        Vector2 finalMoveDir = (direction + separation * 0.5f).normalized;
        rb.MovePosition(rb.position + finalMoveDir * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

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
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    private void Die()
    {
        if (!string.IsNullOrEmpty(xpGemAddress))
        {
            Addressables.InstantiateAsync(xpGemAddress, transform.position, Quaternion.identity);
        }

        if (spawner != null)
        {
            spawner.RegisterEnemyDefeated();
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealContactDamage(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealContactDamage(other.gameObject);
    }

    private void TryDealContactDamage(GameObject target)
    {
        if (Time.time < lastDamageTime + damageCooldown) return;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = target.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            lastDamageTime = Time.time;
            playerHealth.TakeDamage(contactDamage);
        }
    }
}