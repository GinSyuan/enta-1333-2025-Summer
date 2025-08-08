/// <summary>
/// Controls timed spawning of enemy waves.
/// Key Usage: Configure wave interval and enemy composition; starts after certain conditions are met.
/// </summary>
﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy waves: starts after first building is placed, spawns enemies every X seconds,
/// updates UI, and plays alert sound.
/// </summary>
public class WaveManager : MonoBehaviour
{
    /* ────────────────────────────── Inspector ─ */
    [Header("Wave Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float initialDelay = 60f;
    public float waveInterval = 60f;
    public int baseEnemiesPerWave = 3;
    public float spacing = 2f;     // distance between enemies when spawning

    [Header("UI")]
    public Text countdownText;

    [Header("Audio")]
    public AudioClip waveAlarm;
    private AudioSource audioSource;

    /* ────────────────────────────── Runtime ─ */
    private bool hasStarted = false;
    private bool isSpawning = false;
    private float waveTimer = 0f;
    private int waveCount = 0;

    private UnitManager unitManager;
    private GridManager gridManager;

    /* ────────────────────────────── Unity ─ */
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        countdownText.text = "";
        unitManager = FindObjectOfType<UnitManager>();
        gridManager = FindObjectOfType<GridManager>();
    }

    void Update()
    {
        if (!hasStarted) return;

        waveTimer += Time.deltaTime;
        float remaining = waveInterval - waveTimer;
        countdownText.text = $"Next Wave: {Mathf.CeilToInt(Mathf.Max(remaining, 0))}s";

        if (!isSpawning && waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            StartCoroutine(SpawnWave());
        }
    }

    /* ────────────────────────────── Public API ─ */
/// <summary>
    /// StartWaveTimer - Run setup logic at the beginning
    /// </summary>
    public void StartWaveTimer()
    {
        if (hasStarted) return;
        hasStarted = true;
        StartCoroutine(DelayFirstWave());
    }

    /* ────────────────────────────── Coroutines ─ */
    IEnumerator DelayFirstWave()
    {
        float t = initialDelay;
        while (t > 0f)
        {
            countdownText.text = $"First Wave: {Mathf.CeilToInt(t)}s";
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
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            /* 1. Instantiate enemy prefab */
            Vector3 spawnPos = spawn.position + new Vector3(i * spacing, 0f, 0f);
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            /* 2. Set faction */
            Unit unit = enemy.GetComponent<Unit>();
            unit.factionID = 1;

            /* 3. Ensure it has a Pathfinder */
            Pathfinder pf = enemy.GetComponent<Pathfinder>();
            if (pf == null) pf = enemy.AddComponent<Pathfinder>();

            /* 4. Create a marker for this unit’s path target */
            Transform marker = new GameObject($"{enemy.name}_Target").transform;
            marker.position = spawnPos;                 // start at spawn position

            /* 5. Initialise pathfinder */
            pf.Init(gridManager, enemy.transform, marker);

            /* 6. Register into UnitManager */
            var entry = new UnitManager.UnitEntry
            {
                unitTransform = enemy.transform,
                targetTransform = marker,
                pathfinder = pf,
                path = new List<Vector3>(),
                pathIndex = 0
            };
            unitManager.Units.Add(entry);

            yield return new WaitForSeconds(0.3f);       // small stagger between spawns
        }

        /* 7. Play alarm */
        if (waveAlarm != null && audioSource != null)
            audioSource.PlayOneShot(waveAlarm);

        isSpawning = false;

        if (GameManager.Instance != null)
            GameManager.Instance.OnWaveCompleted();
    }
}
