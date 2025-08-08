/// <summary>
/// Specialized building type with unique attack or spawn logic.
/// Key Usage: Attach to mage tower prefab to enable special behavior.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageTower : MonoBehaviour
{
    [Header("Spawning Settings")]
    public UnitType unitType;
    public int spawnCount = 6;
    public float spawnInterval = 2f;
    public Transform spawnLocator;
    public Transform rallyPoint;
    public float formationSpacing = 1.2f;

    private UnitManager unitManager;
    private GridManager gridManager;
    private bool spawningStarted = false;

/// <summary>
    /// Awake - Perform this action
    /// </summary>
    private void Awake()
    {
        unitManager = FindObjectOfType<UnitManager>();
        gridManager = FindObjectOfType<GridManager>();
    }


/// <summary>
    /// StartSpawning - Run setup logic at the beginning
    /// </summary>
    public void StartSpawning()
    {
        if (spawningStarted) return;
        spawningStarted = true;
        StartCoroutine(SpawnRoutine());
    }

/// <summary>
    /// SpawnRoutine - Spawn or create objects
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        int columns = Mathf.CeilToInt(Mathf.Sqrt(spawnCount));
        int rows = Mathf.CeilToInt(spawnCount / (float)columns);

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnUnit(i, columns, rows);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

/// <summary>
    /// SpawnUnit - Spawn or create objects
    /// </summary>
    private void SpawnUnit(int index, int columns, int rows)
    {
        var unitGO = Instantiate(unitType.unitPrefab, spawnLocator.position, spawnLocator.rotation);

        int col = index % columns;
        int row = index / columns;
        float offsetX = (col - (columns - 1) / 2f) * formationSpacing;
        float offsetZ = (row - (rows - 1) / 2f) * formationSpacing;
        Vector3 targetPos = rallyPoint.position + new Vector3(offsetX, 0, offsetZ);

        Debug.Log($"[MageTower] Unit {index} rally at {targetPos}");
        var marker = new GameObject(unitGO.name + "_RallyMarker");
        marker.transform.position = targetPos;

        var entry = new UnitManager.UnitEntry
        {
            unitTransform = unitGO.transform,
            targetTransform = marker.transform,
            initialPosition = unitGO.transform.position
        };

        var pf = unitGO.AddComponent<Pathfinder>();
        pf.Init(gridManager, unitGO.transform, marker.transform);
        pf.FindPath();
        entry.pathfinder = pf;
        entry.path = new List<Vector3>(pf.PathPositions ?? new List<Vector3>());
        entry.pathIndex = 0;
        unitManager.Units.Add(entry);
    }
}
