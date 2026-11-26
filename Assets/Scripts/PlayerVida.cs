using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Auto Regeneration")]
    [SerializeField] private float regenDelay = 3f;      // Segundos sem levar hit para começar a curar
    [SerializeField] private float regenInterval = 0.5f; // Intervalo entre cada tick de cura
    [SerializeField] private int regenAmount = 5;        // Quanto de vida recupera por tick

    [Header("UI Reference")]
    [SerializeField] private Image healthBarFill; // Arraste o objeto 'frente' aqui no Inspector

    private float lastDamageTime = -999f;
    private float nextRegenTime = 0f;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int RegenAmount => regenAmount;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        // Só regenera se estiver vivo, com vida abaixo do máximo e após o tempo sem tomar dano
        if (currentHealth > 0 && currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            if (Time.time >= nextRegenTime)
            {
                Heal(regenAmount);
                nextRegenTime = Time.time + regenInterval;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        lastDamageTime = Time.time; // Reseta o contador de regeneração ao levar hit
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        Heal(amount); // Também cura o valor ganho
    }

    public void IncreaseRegen(int amount)
    {
        regenAmount += amount;
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}