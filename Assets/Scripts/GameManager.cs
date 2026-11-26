using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level & XP Settings")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 10;

    [Header("HUD UI")]
    [SerializeField] private Image xpBarFill;          // Arraste o 'XpBarPreenchimento' (Filled)
    [SerializeField] private TMP_Text roundText;       // Arraste o 'RoundTexto' (TextMeshPro)
    [SerializeField] private int maxRounds = 10;       // O jogo termina na 10ª rodada

    [Header("Level Up UI Panel")]
    [SerializeField] private GameObject levelUpPanel; // Painel de escolha que aparece na tela

    private EnemySpawner spawner;
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        spawner = Object.FindFirstObjectByType<EnemySpawner>();

        if (levelUpPanel != null) levelUpPanel.SetActive(false);

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
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.3f); // Aumenta a curva de XP

        Time.timeScale = 0f; // Pausa o jogo para o player escolher o power-up
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }

        Debug.Log($"Level Up! Novo nível: {currentLevel}");
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
            gameEnded = true;
            if (roundText != null)
            {
                roundText.text = $"Round {maxRounds}/{maxRounds} - Vitória!";
            }
            Time.timeScale = 0f;
            Debug.Log("Fim de jogo: 10ª rodada alcançada!");
            return;
        }

        if (roundText != null)
        {
            roundText.text = $"Round {currentRound}/{maxRounds}";
        }
    }

    // Métodos chamados pelos botões de escolha de Power-Up na UI
    public void ChoosePowerUpSpeed()
    {
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player != null) player.IncreaseSpeed(1.5f);
        ResumeGame();
    }

    public void ChoosePowerUpDamage()
    {
        // Exemplo: Aumenta o dano do tiro futuramente
        ResumeGame();
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }
}