using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab; // Prefab do extintor
    [SerializeField] private Transform player;       // Referência ao Foguin

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1.5f;     // Tempo entre cada spawn
    [SerializeField] private float spawnDistanceOffset = 2f; // Distância extra além da borda da câmera

    [Header("Wave & Scaling Settings")]
    [SerializeField] private int enemiesPerWave = 25;        // Quantos inimigos derrotados para subir a wave
    private int currentWave = 1;
    public int CurrentWave => currentWave;
    private int enemiesDefeatedInCurrentWave = 0;
    private int enemyExtraHealth = 0;                        // Vida extra somada a cada wave

    private float spawnTimer;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (player == null)
        {
            GameObject p = GameObject.Find("Foguin");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null || enemyPrefab == null) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemyOffscreen();
        }
    }

    private void SpawnEnemyOffscreen()
    {
        // Calcula os limites da visão da câmera orthographic em world space
        float vertExtent = mainCamera.orthographicSize;
        float horizExtent = vertExtent * mainCamera.aspect;

        // Escolhe aleatoriamente um lado de fora da tela (0: Cima, 1: Baixo, 2: Esquerda, 3: Direita)
        int side = Random.Range(0, 4);
        Vector3 spawnPos = player.position;

        switch (side)
        {
            case 0: // Acima
                spawnPos.y += vertExtent + spawnDistanceOffset;
                spawnPos.x += Random.Range(-horizExtent, horizExtent);
                break;
            case 1: // Abaixo
                spawnPos.y -= vertExtent + spawnDistanceOffset;
                spawnPos.x += Random.Range(-horizExtent, horizExtent);
                break;
            case 2: // Esquerda
                spawnPos.x -= horizExtent + spawnDistanceOffset;
                spawnPos.y += Random.Range(-vertExtent, vertExtent);
                break;
            case 3: // Direita
                spawnPos.x += horizExtent + spawnDistanceOffset;
                spawnPos.y += Random.Range(-vertExtent, vertExtent);
                break;
        }

        // Instancia o inimigo na posição calculada
        GameObject newEnemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Aplica o aumento de vida baseado na wave atual
        Enemy enemyComponent = newEnemyObj.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.InitializeHealth(enemyExtraHealth);
        }
    }

    // Chamado pelo Enemy quando ele morre
    public void RegisterEnemyDefeated()
    {
        enemiesDefeatedInCurrentWave++;

        if (enemiesDefeatedInCurrentWave >= enemiesPerWave)
        {
            AdvanceWave();
        }
    }

    private void AdvanceWave()
    {
        currentWave++;
        enemiesDefeatedInCurrentWave = 0;
        enemyExtraHealth += 2; // Aumenta 2 de vida máxima dos próximos extintores a cada wave

        // Deixa o spawn um pouquinho mais ágil a cada wave (opcional, com limite mínimo)
        spawnInterval = Mathf.Max(0.4f, spawnInterval - 0.1f);

        Debug.Log($"Wave Avançada! Nova Wave: {currentWave} | Vida extra dos inimigos: {enemyExtraHealth}");
    }
}