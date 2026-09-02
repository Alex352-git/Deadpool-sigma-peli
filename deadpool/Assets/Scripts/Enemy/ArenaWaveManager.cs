using System.Collections;
using UnityEngine;
using TMPro;

public class ArenaWaveManager : MonoBehaviour
{
    public static ArenaWaveManager Instance;

    [Header("Prefabs & Locations")]
    public GameObject lizardPrefab;
    public Transform[] spawnPoints;
    public float spawnRadius = 3.0f; // Spreads enemies out if multiple spawn at the same point

    [Header("UI Text")]
    public TextMeshProUGUI waveBannerText;
    public TextMeshProUGUI enemyCounterText;

    [Header("Wave Configuration")]
    public int baseEnemiesPerWave = 5;
    public int extraEnemiesPerWave = 3;
    public float timeBetweenSpawns = 2.0f; // Time between simultaneous wave bursts
    public float timeBetweenWaves = 4.0f;

    [Header("Difficulty Scaling")]
    public float speedIncreaseFactor = 0.08f;
    public float healthIncreaseFactor = 0.15f;
    public float damageIncreaseFactor = 0.10f;

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

        // Spawn Boss at a random spawn point first if it's a Boss wave
        if (isBossWave && spawnPoints.Length > 0)
        {
            Transform bossSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnEnemyAtPoint(bossSpawn, isBoss: true);
            enemiesLeftToSpawn--;
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        // --- SIMULTANEOUS SPAWNING LOOP ---
        while (enemiesLeftToSpawn > 0)
        {
            // Spawn 1 enemy at EVERY spawn point simultaneously in the same frame
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (enemiesLeftToSpawn <= 0) break;

                SpawnEnemyAtPoint(spawnPoints[i], isBoss: false);
                enemiesLeftToSpawn--;
            }

            // Wait before spawning the next simultaneous group
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    void SpawnEnemyAtPoint(Transform targetPoint, bool isBoss)
    {
        if (targetPoint == null || lizardPrefab == null) return;

        // Apply a small position variation so enemies at the same spawn point don't overlap completely
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = targetPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        GameObject newLizard = Instantiate(lizardPrefab, spawnPos, targetPoint.rotation);

        LizardMonsterAI ai = newLizard.GetComponent<LizardMonsterAI>();
        if (ai != null)
        {
            float speedMult = 1.0f + ((currentWave - 1) * speedIncreaseFactor);
            float healthMult = 1.0f + ((currentWave - 1) * healthIncreaseFactor);
            float damageMult = 1.0f + ((currentWave - 1) * damageIncreaseFactor);

            // Grab the base scale from the prefab so it isn't overwritten with 1
            float baseScale = lizardPrefab.transform.localScale.x;

            if (isBoss)
            {
                // Pass the base scale multiplied by the boss scale (e.g., 7 * 2.2)
                ai.InitializeStats(healthMult * bossHealthMultiplier, speedMult * 0.9f, damageMult * 2.0f, baseScale * bossSizeScale);
            }
            else
            {
                // Pass the prefab's base scale (e.g., 7)
                ai.InitializeStats(healthMult, speedMult, damageMult, baseScale);
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