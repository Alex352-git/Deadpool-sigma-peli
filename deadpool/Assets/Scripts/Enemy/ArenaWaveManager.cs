using System.Collections;
using UnityEngine;
using TMPro;

public class ArenaWaveManager : MonoBehaviour
{
    public static ArenaWaveManager Instance;

    [Header("Prefabs & Locations")]
    public GameObject lizardPrefab;
    public Transform[] spawnPoints;
    public float spawnRadius = 3.0f; // Spreads enemies out so they don't overlap

    [Header("UI Text")]
    public TextMeshProUGUI waveBannerText;
    public TextMeshProUGUI enemyCounterText;

    [Header("Wave Configuration")]
    public int baseEnemiesPerWave = 5;
    public int extraEnemiesPerWave = 3;
    public float timeBetweenSpawns = 1.0f;
    public float timeBetweenWaves = 4.0f;

    [Header("Difficulty Scaling")]
    public float speedIncreaseFactor = 0.08f; // +8% speed per wave
    public float healthIncreaseFactor = 0.15f; // +15% health per wave
    public float damageIncreaseFactor = 0.10f; // +10% damage per wave

    [Header("Boss Settings")]
    public int bossEveryNWaves = 3;
    public float bossSizeScale = 2.2f;
    public float bossHealthMultiplier = 3.5f;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private int enemiesLeftToSpawn = 0;
    private bool isWaveInProgress = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(StartNextWaveRoutine());
    }

    IEnumerator StartNextWaveRoutine()
    {
        isWaveInProgress = false;
        currentWave++;

        enemiesLeftToSpawn = baseEnemiesPerWave + ((currentWave - 1) * extraEnemiesPerWave);
        enemiesAlive = enemiesLeftToSpawn;
        bool isBossWave = (currentWave % bossEveryNWaves == 0);

        if (waveBannerText != null)
        {
            waveBannerText.gameObject.SetActive(true);
            if (isBossWave) waveBannerText.text = $"WAVE {currentWave}\n<color=red>BOSS LIZARD APPROACHING!</color>";
            else waveBannerText.text = $"WAVE {currentWave}";
        }

        UpdateEnemyCounterUI();
        yield return new WaitForSeconds(timeBetweenWaves);

        if (waveBannerText != null) waveBannerText.gameObject.SetActive(false);
        isWaveInProgress = true;

        if (isBossWave)
        {
            SpawnEnemy(isBoss: true);
            enemiesLeftToSpawn--;
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        while (enemiesLeftToSpawn > 0)
        {
            SpawnEnemy(isBoss: false);
            enemiesLeftToSpawn--;
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    void SpawnEnemy(bool isBoss)
    {
        if (spawnPoints.Length == 0 || lizardPrefab == null) return;

        Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Calculate a random position around the spawn point
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = chosenPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        GameObject newLizard = Instantiate(lizardPrefab, spawnPos, chosenPoint.rotation);

        LizardMonsterAI ai = newLizard.GetComponent<LizardMonsterAI>();
        if (ai != null)
        {
            float speedMult = 1.0f + ((currentWave - 1) * speedIncreaseFactor);
            float healthMult = 1.0f + ((currentWave - 1) * healthIncreaseFactor);
            float damageMult = 1.0f + ((currentWave - 1) * damageIncreaseFactor);

            if (isBoss)
            {
                ai.InitializeStats(healthMult * bossHealthMultiplier, speedMult * 0.9f, damageMult * 2.0f, bossSizeScale);
            }
            else
            {
                ai.InitializeStats(healthMult, speedMult, damageMult, 1.0f);
            }
        }
    }

    public void OnEnemyKilled()
    {
        if (!isWaveInProgress) return;

        enemiesAlive--;
        UpdateEnemyCounterUI();

        if (enemiesAlive <= 0)
        {
            StartCoroutine(StartNextWaveRoutine());
        }
    }

    void UpdateEnemyCounterUI()
    {
        if (enemyCounterText != null) enemyCounterText.text = $"Enemies Left: {enemiesAlive}";
    }
}