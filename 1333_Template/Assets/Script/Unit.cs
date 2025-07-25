using UnityEngine;

/// <summary>
/// Handles unit health, detection, melee attacks, and drawing gizmos
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("Combat Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Ranges")]
    public float detectRange = 5f;
    public float attackRange = 1.5f;

    public int attackDamage = 20;
    public float attackCooldown = 1.0f;

    [Header("Faction")]
    public int factionID = 0;  // 0=player, 1=enemy

    private float attackTimer = 0f;
    private UnitManager unitManager;
    private Unit currentTarget;

    /// <summary>
    /// Static toggle to show or hide detection/attack range
    /// controlled by the GameManager
    /// </summary>
    public static bool showRangeGizmos = true;

    /// <summary>
    /// Static toggle to enable/disable combat globally
    /// </summary>
    public static bool isCombatEnabled = true;

  
    private float idleTimer = 0f;
    private bool isIdleSoundPlayed = false;
    private Vector3 lastPosition;

    private void Start()
    {
        currentHealth = maxHealth;
        unitManager = FindObjectOfType<UnitManager>();

        lastPosition = transform.position;
        showRangeGizmos = false;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

    
        if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            idleTimer = 0f;
            lastPosition = transform.position;
            isIdleSoundPlayed = false;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= 10f && !isIdleSoundPlayed)
            {
                if (factionID == 0)
                {
                    AudioManager.Instance.PlayBored();
                }
                isIdleSoundPlayed = true;
            }
        }

        if (!isCombatEnabled) return;

        if (currentTarget == null || currentTarget.currentHealth <= 0)
        {
            currentTarget = FindNearestEnemyInDetectRange();
        }

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist > attackRange)
            {
                // Move closer
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    currentTarget.transform.position,
                    2f * Time.deltaTime
                );
            }
            else
            {
                if (attackTimer <= 0f)
                {
                    currentTarget.TakeDamage(attackDamage);
                    attackTimer = attackCooldown;

                    AudioManager.Instance.PlayCombat();
                }
            }
        }
    }

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

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");

        if (unitManager != null)
        {
            var entry = unitManager.Units.Find(e => e.unitTransform == this.transform);
            if (entry != null)
                unitManager.Units.Remove(entry);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Always draw health bar
        float healthPercent = (maxHealth > 0) ? (float)currentHealth / maxHealth : 0f;
        Vector3 barPos = transform.position + Vector3.up * 2f;

        // gray background
        Gizmos.color = Color.gray;
        Gizmos.DrawCube(barPos, new Vector3(1f, 0.1f, 0.1f));

        // colored health
        Gizmos.color = (factionID == 0) ? Color.green : Color.red;
        Gizmos.DrawCube(barPos, new Vector3(healthPercent, 0.1f, 0.1f));

        // Only draw detection/attack range if toggled on
        if (showRangeGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
