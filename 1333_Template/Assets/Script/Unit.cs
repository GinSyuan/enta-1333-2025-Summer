/// <summary>
/// Represents a unit's health, detection, movement, and combat.
/// Key Usage: Controlled by UnitManager; can attack units/buildings and respond to commands.
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles health, detection, pathfinding, and combat for units.
/// Supports attacking both units and buildings, including AI logic.
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("UI")]
    public GameObject healthBarPrefab;   // Prefab for health bar (world-space UI)
    private HealthBarUI healthBarUI;     // Instance of the health bar

    // Default Y-offset so health bar appears above the unit
    private static readonly Vector3 unitBarOffset = new Vector3(0, 2.0f, 0);

    [Header("Combat")]
    public int maxHealth = 100;          // Maximum health points
    public float attackRange = 1.5f;     // Attack range in world units
    public int attackDamage = 20;        // Damage per attack
    public float attackCooldown = 1f;    // Time between attacks

    [Header("AI (faction 0 = player, 1 = enemy)")]
    public float detectRange = 5f;       // Detection radius for enemies
    public int factionID = 0;            // 0 = player, 1 = enemy

    public static bool showRangeGizmos = false;   // Toggle to show detection/attack ranges
    public static bool isCombatEnabled = true;    // Toggle to enable/disable combat globally

    private int currentHealth;           // Current health points
    private float atkTimer;              // Attack cooldown timer
    private float repathTimer;           // Timer for path recalculation
    private MonoBehaviour currentTarget; // Current enemy target (unit or building)

    private UnitManager unitMgr;         // Reference to the UnitManager
    private GridManager gridMgr;         // Reference to the GridManager

    void Start()
    {
        // Create and initialize health bar UI
        if (healthBarPrefab != null)
        {
            GameObject bar = Instantiate(healthBarPrefab);
            healthBarUI = bar.GetComponent<HealthBarUI>();
            healthBarUI.SetTarget(this.transform, unitBarOffset); // Follow this unit
            healthBarUI.SetHealth(1.0f); // Full health at start
        }

        currentHealth = maxHealth;
        atkTimer = attackCooldown;
        repathTimer = 0f;

        // Find game managers
        unitMgr = FindObjectOfType<UnitManager>();
        gridMgr = FindObjectOfType<GridManager>();

        // Mark starting grid cell as occupied
        var idx = gridMgr.GetXYIndex(transform.position);
        gridMgr.SetCellOccupied(idx.x, idx.y, true);

        // Start following assigned path
        StartCoroutine(FollowGridPath());

        // Start AI brain if this is an enemy
        if (factionID == 1)
            StartCoroutine(EnemyBrain());
    }

    void Update()
    {
        atkTimer -= Time.deltaTime;
        repathTimer -= Time.deltaTime;

        if (!isCombatEnabled) return; // Skip combat logic if disabled

        // Player unit: search for nearest enemy when no current target
        if (factionID == 0 && (currentTarget == null || !currentTarget))
        {
            var cand = FindNearestEnemyInDetectRange();
            if (cand != null)
            {
                currentTarget = cand;
                RepathTo(cand.transform.position);
            }
        }

        // Enemy unit: update target when a better one is found
        if (factionID == 1)
        {
            var cand = FindNearestEnemyInDetectRange();
            if (cand != null && cand != currentTarget)
            {
                currentTarget = cand;
                RepathTo(cand.transform.position);
            }
        }

        // Periodically re-check and update path to target
        if (repathTimer <= 0f && currentTarget != null)
        {
            var b = currentTarget as Building;
            var u = currentTarget as Unit;
            bool alive = (b != null && b.Health > 0) || (u != null && u.currentHealth > 0);
            if (alive)
            {
                repathTimer = 2f; // Repath every 2 seconds
                RepathTo(currentTarget.transform.position);
            }
        }

        // Attack logic
        if (currentTarget != null && atkTimer <= 0f)
        {
            var b = currentTarget as Building;
            var u = currentTarget as Unit;
            bool alive = (b != null && b.Health > 0) || (u != null && u.currentHealth > 0);
            if (!alive) return;

            // Calculate attack range (include building size offset)
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
            float reach = attackRange;
            if (b != null && gridMgr != null)
                reach += gridMgr.GridSettings.NodeSize * 0.5f;

            // If within range, apply damage
            if (dist <= reach)
            {
                if (u != null)
                    u.TakeDamage(attackDamage);
                else if (b != null)
                    b.TakeDamage(attackDamage);

                atkTimer = attackCooldown;
                AudioManager.Instance?.PlayCombat();
            }
        }
    }

    /// <summary>
    /// AI logic for enemy units to pick and chase targets.
    /// </summary>
    IEnumerator EnemyBrain()
    {
        yield return new WaitForSeconds(2f); // Delay before first action

        while (true)
        {
            bool targetInvalid = (currentTarget == null);
            var u = currentTarget as Unit;
            var b = currentTarget as Building;

            // Validate target
            if (u != null && (u.factionID != 0 || u.currentHealth <= 0))
                targetInvalid = true;
            if (b != null && (b.Team != 0 || b.Health <= 0))
                targetInvalid = true;

            // Pick a new target if current is invalid
            if (targetInvalid)
            {
                currentTarget = PickNearestBuildingOrSoldier();
                if (currentTarget != null)
                    RepathTo(currentTarget.transform.position);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Moves unit along its assigned grid path.
    /// </summary>
    IEnumerator FollowGridPath()
    {
        var entry = unitMgr.Units.Find(e => e.unitTransform == transform);
        if (entry == null) yield break;

        while (true)
        {
            if (entry.path != null && entry.pathIndex < entry.path.Count)
            {
                Vector3 next = entry.path[entry.pathIndex];
                transform.position = Vector3.MoveTowards(transform.position, next, 2f * Time.deltaTime);

                // If reached next node, update grid occupancy
                if (Vector3.Distance(transform.position, next) < 0.05f)
                {
                    Vector2Int prevIdx = gridMgr.GetXYIndex(entry.path[Mathf.Max(entry.pathIndex - 1, 0)]);
                    Vector2Int newIdx = gridMgr.GetXYIndex(next);
                    gridMgr.SetCellOccupied(prevIdx.x, prevIdx.y, false);
                    gridMgr.SetCellOccupied(newIdx.x, newIdx.y, true);

                    entry.pathIndex++;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Calculates and assigns a new path to destination.
    /// </summary>
    void RepathTo(Vector3 destWorld)
    {
        var entry = unitMgr.Units.Find(e => e.unitTransform == transform);
        if (entry == null) return;

        // Convert world positions to grid indices
        Vector2Int startIdx = gridMgr.GetXYIndex(transform.position);
        Vector2Int goalIdx = gridMgr.GetXYIndex(destWorld);

        // Ensure start and goal cells are free
        Vector2Int fixedStart = FindNearestFree(startIdx, 2);
        Vector2Int fixedGoal = FindNearestFree(goalIdx, 2);
        destWorld = gridMgr.GetNode(fixedGoal.x, fixedGoal.y).WorldPosition;

        // Create/update target transform
        entry.targetTransform ??= new GameObject($"{name}_Target").transform;
        entry.targetTransform.position = destWorld;

        // Create/update pathfinder component
        entry.pathfinder ??= GetComponent<Pathfinder>() ?? gameObject.AddComponent<Pathfinder>();
        entry.pathfinder.Init(gridMgr, transform, entry.targetTransform);
        entry.pathfinder.FindPath();

        // Assign new path
        var p = entry.pathfinder.PathPositions;
        if (p == null || p.Count == 0)
        {
            repathTimer = 0.2f;
            return;
        }
        entry.path = new List<Vector3>(p);
        entry.pathIndex = 0;
    }

    /// <summary>
    /// Finds nearest free grid cell within a radius.
    /// </summary>
    Vector2Int FindNearestFree(Vector2Int center, int radius)
    {
        if (gridMgr.IsCellFree(center.x, center.y)) return center;

        for (int r = 1; r <= radius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    Vector2Int idx = new(center.x + dx, center.y + dy);
                    if (idx.x < 0 || idx.y < 0 ||
                        idx.x >= gridMgr.GridSettings.GridSizeX ||
                        idx.y >= gridMgr.GridSettings.GridSizeY) continue;

                    if (gridMgr.IsCellFree(idx.x, idx.y))
                        return idx;
                }
        }
        return center;
    }

    /// <summary>
    /// Picks the nearest player building or soldier to target.
    /// </summary>
    MonoBehaviour PickNearestBuildingOrSoldier()
    {
        // Prioritize MainTower
        foreach (var b in FindObjectsOfType<Building>())
        {
            if (b.Team != 0 || b.Health <= 0) continue;
            if (b.gameObject.name.Contains("MainTower"))
                return b;
        }

        // Find nearest building
        Building bestB = null; float best = float.MaxValue;
        foreach (var b in FindObjectsOfType<Building>())
        {
            if (b.Team != 0 || b.Health <= 0) continue;
            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < best) { best = d; bestB = b; }
        }
        if (bestB) return bestB;

        // Find nearest soldier
        Unit bestU = null; best = float.MaxValue;
        foreach (var u in FindObjectsOfType<Unit>())
        {
            if (u.factionID != 0 || u.currentHealth <= 0) continue;
            float d = Vector3.Distance(transform.position, u.transform.position);
            if (d < best) { best = d; bestU = u; }
        }
        if (bestU) return bestU;

        return null;
    }

    /// <summary>
    /// Finds nearest enemy within detection range.
    /// </summary>
    MonoBehaviour FindNearestEnemyInDetectRange(float rangeOverride = -1f)
    {
        float range = rangeOverride > 0 ? rangeOverride : detectRange;
        MonoBehaviour bestTarget = null; float best = range;

        foreach (var b in FindObjectsOfType<Building>())
        {
            if (b.Team == factionID || b.Health <= 0) continue;
            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < best) { best = d; bestTarget = b; }
        }
        foreach (var u in FindObjectsOfType<Unit>())
        {
            if (u.factionID == factionID || u.currentHealth <= 0) continue;
            float d = Vector3.Distance(transform.position, u.transform.position);
            if (d < best) { best = d; bestTarget = u; }
        }
        return bestTarget;
    }

    /// <summary>
    /// Receives damage and updates health bar.
    /// </summary>
    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        float percent = (float)currentHealth / maxHealth;
        if (healthBarUI != null)
            healthBarUI.SetHealth(percent);

        if (currentHealth <= 0) Die();
    }

    /// <summary>
    /// Handles unit death: removes from manager, clears grid, destroys health bar and self.
    /// </summary>
    void Die()
    {
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);

        var ent = unitMgr.Units.Find(e => e.unitTransform == transform);
        if (ent != null) unitMgr.Units.Remove(ent);

        var idx = gridMgr.GetXYIndex(transform.position);
        gridMgr.SetCellOccupied(idx.x, idx.y, false);

        Destroy(gameObject);
    }

    /// <summary>
    /// Draws detection and attack range gizmos for debugging.
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showRangeGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
