using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages enemy waves: starts after first building is placed, spawns enemies every X seconds,
/// updates UI, and plays alert sound.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float initialDelay = 60f;
    public float waveInterval = 60f;
    public int baseEnemiesPerWave = 3;
    public float spacing = 2f; // Distance between enemies when spawning

    [Header("UI")]
    public Text countdownText;

    [Header("Audio")]
    public AudioClip waveAlarm;
    private AudioSource audioSource;

    private bool hasStarted = false;
    private bool isSpawning = false;
    private float waveTimer = 0f;
    private int waveCount = 0;

    private UnitManager unitManager;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        countdownText.text = "";
        unitManager = FindObjectOfType<UnitManager>();
    }

    void Update()
    {
        if (!hasStarted) return;

        waveTimer += Time.deltaTime;
        float remaining = waveInterval - waveTimer;
        countdownText.text = "Next Wave: " + Mathf.CeilToInt(Mathf.Max(remaining, 0)) + "s";

        if (!isSpawning && waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            StartCoroutine(SpawnWave());
        }
    }

    /// <summary>
    /// Call this when the player places their first building.
    /// </summary>
    public void StartWaveTimer()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            StartCoroutine(DelayFirstWave());
        }
    }

    IEnumerator DelayFirstWave()
    {
        float t = initialDelay;
        while (t > 0)
        {
            countdownText.text = "First Wave: " + Mathf.CeilToInt(t) + "s";
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }

        yield return SpawnWave();
    }

    IEnumerator SpawnWave()
    {
        isSpawning = true;
        waveCount++;

        int enemiesToSpawn = baseEnemiesPerWave + waveCount;

        // Choose one random spawn point for this wave
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 spawnPos = spawn.position + new Vector3(i * spacing, 0f, 0f);
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            Unit unit = enemy.GetComponent<Unit>();
            unit.factionID = 1;

            // Add to UnitManager so enemies will be seen by others
            UnitManager.UnitEntry entry = new UnitManager.UnitEntry
            {
                unitTransform = enemy.transform,
                targetTransform = enemy.transform // optional: could be a dummy marker
            };
            unitManager.Units.Add(entry);

            yield return new WaitForSeconds(0.3f);
        }

        if (waveAlarm != null && audioSource != null)
        {
            audioSource.PlayOneShot(waveAlarm);
        }

        isSpawning = false;
    }
}
