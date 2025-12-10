using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _timeBetweenWaves = 3f;

    [Header("Wave Settings")]
    // [SerializeField] private int _enemiesPerWave = 5;
    // [SerializeField] private float _difficultyMultiplier = 1.2f; // Increase enemies each wave

    private int _enemiesRemaining;
    private int _currentWave = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // 1. Find the 3 pre-placed zombies already in the scene
        // Make sure your zombies have the "Enemy" tag!
        GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        _enemiesRemaining = existingEnemies.Length;

        if (_enemiesRemaining == 0)
        {
            Debug.LogWarning("[WaveManager] No pre-placed enemies found. Starting Wave 1 immediately.");
            StartCoroutine(StartNextWave());
        }
        else
        {
            Debug.Log($"[WaveManager] Found {_enemiesRemaining} pre-placed enemies. Waiting for them to die...");
        }
    }

    public void RegisterEnemyDeath()
    {
        _enemiesRemaining--;

        if (_enemiesRemaining <= 0)
        {
            // All enemies dead, start next wave
            StartCoroutine(StartNextWave());
        }
    }

    private IEnumerator StartNextWave()
    {
        _currentWave++;
        Debug.Log($"[WaveManager] Starting Wave {_currentWave} in {_timeBetweenWaves} seconds...");

        yield return new WaitForSeconds(_timeBetweenWaves);

        SpawnWave();
    }

    private void SpawnWave()
    {
        // 1. Check if we have spawn points
        if (_spawnPoints.Length == 0) return;

        // 2. Loop through EVERY spawn point in the list
        foreach (Transform spawnPoint in _spawnPoints)
        {
            // Spawn one enemy at this specific point
            Instantiate(_enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        // 3. Update the counter
        // The wave size is now exactly equal to the number of spawn points
        _enemiesRemaining = _spawnPoints.Length;

        Debug.Log($"[WaveManager] Wave {_currentWave} Started! Spawning {_enemiesRemaining} enemies (1 per point).");
    }

    private void SpawnEnemy()
    {
        if (_spawnPoints.Length == 0) return;

        // Pick random spawn point
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        Instantiate(_enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}