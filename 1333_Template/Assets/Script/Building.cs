/// <summary>
/// Represents a building with health, team ownership, and grid position.
/// Key Usage: Placed via BuildingPlacementManager; can be attacked and destroyed.
/// </summary>
﻿using UnityEngine;

/// <summary>
/// Represents a building placed on the grid with health, size, and team ownership.
/// Handles initialization, taking damage, destruction, and shows real UI health bar.
/// </summary>
public class Building : MonoBehaviour
{
    [Header("Building Stats")]
    [SerializeField] private int maxHealth = 200;
    [SerializeField] public int Team = 0;
    [SerializeField] private Vector2Int sceneSize = new Vector2Int(1, 1);

    public int Health { get; private set; }
    public Vector2Int Size { get; private set; }

    private Vector2Int origin;
    private GridManager gridManager;

    public GameObject healthBarPrefab;
    private HealthBarUI healthBarUI;

    private static readonly Vector3 buildingBarOffset = new Vector3(0, 10f, 0);

/// <summary>
    /// Awake - Perform this action
    /// </summary>
    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (Health == 0)
            {
                Health = maxHealth;
                Size = sceneSize;
                gridManager = FindObjectOfType<GridManager>();
                if (gridManager != null)
                {
                    Vector2Int originGuess = gridManager.GetXYIndex(transform.position);
                    origin = originGuess;
                    for (int dx = 0; dx < Size.x; dx++)
                        for (int dy = 0; dy < Size.y; dy++)
                            gridManager.SetCellOccupied(origin.x + dx, origin.y + dy, true);
                }
            }
            CreateHealthBar();
        }
    }

    public void Initialize(
        Vector2Int origin,
        Vector2Int size,
        int health,
        int team,
        GridManager gm)
    {
        this.origin = origin;
        this.Size = size;
        this.Health = health;
        this.maxHealth = health;
        this.Team = team;
        this.gridManager = gm;

        for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
                gm.SetCellOccupied(origin.x + dx, origin.y + dy, true);

        CreateHealthBar();
    }

/// <summary>
    /// CreateHealthBar - Perform this action
    /// </summary>
    private void CreateHealthBar()
    {
        if (healthBarPrefab != null)
        {
            GameObject bar = Instantiate(healthBarPrefab);
            healthBarUI = bar.GetComponent<HealthBarUI>();
            // Use only Y axis for offset
            healthBarUI.SetTarget(transform, buildingBarOffset);
            healthBarUI.SetHealth(1.0f);
        }
    }

/// <summary>
    /// TakeDamage - Perform this action
    /// </summary>
    public void TakeDamage(int amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;
        UpdateHealthBar();

        if (Health <= 0)
        {
            Die();
        }
    }

/// <summary>
    /// UpdateHealthBar - Update state or handle per-frame logic
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBarUI != null)
        {
            float percent = (float)Health / maxHealth;
            healthBarUI.SetHealth(percent);
        }
    }

/// <summary>
    /// Die - Handle object destruction or death
    /// </summary>
    private void Die()
    {
        if (GameManager.Instance != null && GameManager.Instance.mainTower == this)
            GameManager.Instance.OnMainTowerDestroyed();
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);
        Destroy(this.gameObject);
    }
}
