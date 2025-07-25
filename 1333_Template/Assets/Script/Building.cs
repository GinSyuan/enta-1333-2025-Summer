using UnityEngine;

/// <summary>
/// Represents a building placed on the grid with health, size, and team ownership.
/// Handles initialization, taking damage, and destruction.
/// </summary>
public class Building : MonoBehaviour
{
    public int Health { get; private set; }           // Current health
    public int Team { get; private set; }             // Which team owns this building
    public Vector2Int Size { get; private set; }      // Grid footprint size of the building

    private Vector2Int origin;                        // Grid position of the building's bottom-left corner
    private GridManager gridManager;                  // Reference to the grid manager

    [Header("Building Stats")]
    [SerializeField] private int maxHealth = 200;     // Maximum health

    /// <summary>
    /// Initializes this building with its grid origin, size, health, team, and grid reference.
    /// </summary>
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
        this.maxHealth = health;  // Store for later percentage calculations
        this.Team = team;
        this.gridManager = gm;

        // Mark the occupied cells in the grid
        for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
                gm.SetCellOccupied(origin.x + dx, origin.y + dy, true);
    }

    /// <summary>
    /// Inflicts damage on the building and destroys it if health reaches zero.
    /// </summary>
    public void TakeDamage(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    /// <summary>
    /// Handles the destruction of the building.
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} destroyed!");
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Draws a simple health bar above the building in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (maxHealth <= 0) return;

        float percent = (float)Health / maxHealth;

    
        Vector3 barPos = transform.position + Vector3.up * 6f;

        Gizmos.color = Color.gray;
        Gizmos.DrawCube(barPos, new Vector3(2f, 0.2f, 0.1f));

 
        Color barColor = (Team == 0) ? Color.green : Color.red;
        Gizmos.color = barColor;
        Gizmos.DrawCube(barPos, new Vector3(2f * percent, 0.2f, 0.1f));
    }
}
