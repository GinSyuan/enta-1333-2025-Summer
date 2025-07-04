using UnityEngine;

/// <summary>
/// Handles unit movement, detection, melee attack, and debug gizmos
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("Combat Stats")]
    public int maxHealth = 100;                // maximum health
    public int currentHealth;                  // current health

    [Header("Ranges")]
    public float detectRange = 5f;             // how far the unit can see enemies
    public float attackRange = 1.5f;           // how close it needs to be to attack

    public int attackDamage = 20;              // damage per hit
    public float attackCooldown = 1.0f;        // time between attacks

    [Header("Faction")]
    public int factionID = 0;                  // 0 = player, 1 = enemy

    private float attackTimer = 0f;            // cooldown timer
    private UnitManager unitManager;
    private Unit currentTarget;                // current attack target

    // Shared toggle for gizmos
    private static bool showGizmos = true;

    private void Start()
    {
        currentHealth = maxHealth;
        unitManager = FindObjectOfType<UnitManager>();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        // allow user to press G to toggle gizmos
        if (Input.GetKeyDown(KeyCode.G))
        {
            showGizmos = !showGizmos;
        }

        // if no target or target is dead, find a new one
        if (currentTarget == null || currentTarget.currentHealth <= 0)
        {
            currentTarget = FindNearestEnemyInDetectRange();
        }

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist > attackRange)
            {
                // move closer to target
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    currentTarget.transform.position,
                    2f * Time.deltaTime  // change 2f to your movement speed if needed
                );
            }
            else
            {
                // attack if cooldown is ready
                if (attackTimer <= 0f)
                {
                    currentTarget.TakeDamage(attackDamage);
                    attackTimer = attackCooldown;
                }
            }
        }
    }

    /// <summary>
    /// Finds the nearest enemy within detection range
    /// </summary>
    private Unit FindNearestEnemyInDetectRange()
    {
        float minDist = float.MaxValue;
        Unit nearestEnemy = null;

        foreach (var u in unitManager.Units)
        {
            if (u.unitTransform == null) continue;

            Unit unitScript = u.unitTransform.GetComponent<Unit>();
            if (unitScript == null) continue;
            if (unitScript.factionID == this.factionID) continue;

            float distance = Vector3.Distance(transform.position, u.unitTransform.position);
            if (distance <= detectRange && distance < minDist)
            {
                minDist = distance;
                nearestEnemy = unitScript;
            }
        }
        return nearestEnemy;
    }

    /// <summary>
    /// Applies damage to this unit
    /// </summary>
    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles unit death and removal from UnitManager
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} died");

        // remove from UnitManager
        if (unitManager != null)
        {
            var entryToRemove = unitManager.Units.Find(e => e.unitTransform == this.transform);
            if (entryToRemove != null)
                unitManager.Units.Remove(entryToRemove);
        }

        Destroy(this.gameObject);
    }

    /// <summary>
    /// Draws debug gizmos:
    /// - detection range (yellow)
    /// - attack range (red)
    /// - health bar (gray background + red or green fill)
    /// </summary>
    private void OnDrawGizmos()
    {
        // show detection + attack range only if toggled on
        if (showGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        // always draw health bar
        float healthPercent = (maxHealth > 0) ? (float)currentHealth / maxHealth : 0f;
        Vector3 barPos = transform.position + Vector3.up * 2f;

        // background
        Gizmos.color = Color.gray;
        Gizmos.DrawCube(barPos, new Vector3(1f, 0.1f, 0.1f));

        // fill
        Gizmos.color = (factionID == 0) ? Color.green : Color.red;
        Gizmos.DrawCube(barPos, new Vector3(healthPercent, 0.1f, 0.1f));
    }
}
