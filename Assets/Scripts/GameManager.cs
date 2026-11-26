using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private enum UpgradeType
    {
        MaxHealth,
        Regen,
        MoveSpeed,
        Damage,
        Cooldown,
        ExplosionArea,
        MagnetRange
    }

    [Header("Level & XP Settings")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 10;

    [Header("HUD UI")]
    [SerializeField] private Image xpBarFill;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private int maxRounds = 10;

    [Header("Level Up UI Panel")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Button[] upgradeButtons = new Button[3];
    [SerializeField] private TMP_Text[] upgradeButtonTexts = new TMP_Text[3];
    [SerializeField] private TMP_Text playerStatsText;

    [Header("End Game UI (Vitória / Derrota)")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TMP_Text endGameText;

    private EnemySpawner spawner;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private bool gameEnded = false;

    private readonly UpgradeType[] currentChoices = new UpgradeType[3];

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1f;

        XpGem.bonusMagnetRadius = 0f;
        FireProjectile.bonusDamage = 0;
        FireProjectile.bonusExplosionRadius = 0f;
    }

    private void Start()
    {
        spawner = Object.FindFirstObjectByType<EnemySpawner>();
        playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        playerAttack = Object.FindFirstObjectByType<PlayerAttack>();

        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(false);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i;
            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(index));
            }
        }

        UpdateXPBar();
        UpdateRoundUI();
    }

    private void Update()
    {
        if (!gameEnded)
        {
            UpdateRoundUI();
        }
    }

    public void AddXP(int amount)
    {
        if (gameEnded) return;

        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        UpdateXPBar();
    }

    private void LevelUp()
    {
        if (gameEnded) return;

        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.3f);

        SetupLevelUpChoices();
        UpdatePlayerStatsUI();

        Time.timeScale = 0f;
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }
    }

    private void SetupLevelUpChoices()
    {
        List<UpgradeType> pool = new List<UpgradeType>
        {
            UpgradeType.MaxHealth,
            UpgradeType.Regen,
            UpgradeType.MoveSpeed,
            UpgradeType.Damage,
            UpgradeType.Cooldown,
            UpgradeType.ExplosionArea,
            UpgradeType.MagnetRange
        };

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            UpgradeType chosen = pool[randomIndex];
            pool.RemoveAt(randomIndex);

            currentChoices[i] = chosen;

            if (i < upgradeButtonTexts.Length && upgradeButtonTexts[i] != null)
            {
                upgradeButtonTexts[i].text = GetUpgradeDescription(chosen);
            }
        }
    }

    private string GetUpgradeDescription(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.MaxHealth:
                return "Aumenta a vida máxima (+20)";
            case UpgradeType.Regen:
                return "Aumenta a regeneração de vida (+3)";
            case UpgradeType.MoveSpeed:
                return "Aumenta a velocidade de movimento (+0.8)";
            case UpgradeType.Damage:
                return "Aumenta o dano do fogo (+1)";
            case UpgradeType.Cooldown:
                return "Atira bolas de fogo mais rápido (-0.1s)";
            case UpgradeType.ExplosionArea:
                return "Aumenta a área da explosão (+0.4)";
            case UpgradeType.MagnetRange:
                return "Aumenta o alcance de coleta (+1.2)";
            default:
                return "";
        }
    }

    public void SelectUpgrade(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= currentChoices.Length) return;

        UpgradeType chosen = currentChoices[buttonIndex];

        switch (chosen)
        {
            case UpgradeType.MaxHealth:
                if (playerHealth != null) playerHealth.IncreaseMaxHealth(20);
                break;

            case UpgradeType.Regen:
                if (playerHealth != null) playerHealth.IncreaseRegen(3);
                break;

            case UpgradeType.MoveSpeed:
                if (playerMovement != null) playerMovement.IncreaseSpeed(0.8f);
                break;

            case UpgradeType.Damage:
                FireProjectile.bonusDamage += 1;
                break;

            case UpgradeType.Cooldown:
                if (playerAttack != null) playerAttack.ReduceCooldown(0.1f);
                break;

            case UpgradeType.ExplosionArea:
                FireProjectile.bonusExplosionRadius += 0.4f;
                break;

            case UpgradeType.MagnetRange:
                XpGem.bonusMagnetRadius += 1.2f;
                break;
        }

        ResumeGame();
    }

    private void UpdatePlayerStatsUI()
    {
        if (playerStatsText == null) return;

        int maxHp = playerHealth != null ? playerHealth.MaxHealth : 100;
        int curHp = playerHealth != null ? playerHealth.CurrentHealth : 100;
        int regen = playerHealth != null ? playerHealth.RegenAmount : 5;
        float speed = playerMovement != null ? playerMovement.MoveSpeed : 0f;
        float interval = playerAttack != null ? playerAttack.AttackInterval : 1.5f;

        playerStatsText.text =
            $"Nível: {currentLevel}\n" +
            $"Vida: {curHp}/{maxHp}\n" +
            $"Regen: +{regen}\n" +
            $"Velocidade: {speed:F1}\n" +
            $"Intervalo Tiro: {interval:F2}s\n" +
            $"Dano Extra: +{FireProjectile.bonusDamage}\n" +
            $"Área Extra: +{FireProjectile.bonusExplosionRadius:F1}\n" +
            $"Coleta Extra: +{XpGem.bonusMagnetRadius:F1}";
    }

    private void UpdateXPBar()
    {
        if (xpBarFill != null && xpToNextLevel > 0)
        {
            xpBarFill.fillAmount = Mathf.Clamp01((float)currentXP / xpToNextLevel);
        }
    }

    private void UpdateRoundUI()
    {
        if (spawner == null)
        {
            spawner = Object.FindFirstObjectByType<EnemySpawner>();
        }

        int currentRound = spawner != null ? spawner.CurrentWave : 1;

        if (currentRound >= maxRounds)
        {
            if (roundText != null)
            {
                roundText.text = $"Round {maxRounds}/{maxRounds}";
            }
            TriggerVictory();
            return;
        }

        if (roundText != null)
        {
            roundText.text = $"Round {currentRound}/{maxRounds}";
        }
    }

    public void TriggerVictory()
    {
        ShowEndGameScreen("Vitória");
    }

    public void TriggerGameOver()
    {
        ShowEndGameScreen("Derrota");
    }

    private void ShowEndGameScreen(string resultText)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        if (endGameText != null)
        {
            endGameText.text = resultText;
        }

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void ResumeGame()
    {
        if (gameEnded) return;
        Time.timeScale = 1f;
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }
}