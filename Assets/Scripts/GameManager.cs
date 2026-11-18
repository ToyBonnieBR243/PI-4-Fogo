using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level & XP Settings")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 10;

    [Header("Level Up UI Panel")]
    [SerializeField] private GameObject levelUpPanel; // Painel de escolha que aparece na tela

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.3f); // Aumenta a curva de XP

        Time.timeScale = 0f; // Pausa o jogo para o player escolher o power-up
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }

        Debug.Log($"Level Up! Novo nível: {currentLevel}");
    }

    // Métodos chamados pelos botões de escolha de Power-Up na UI
    public void ChoosePowerUpSpeed()
    {
        // Exemplo: Aumenta a velocidade do player
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