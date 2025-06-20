using UnityEngine;

public class Building : MonoBehaviour
{
    public int Health { get; private set; }
    public int Team { get; private set; }
    public Vector2Int Size { get; private set; }

    private Vector2Int origin;
    private GridManager gridManager;



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
        this.Team = team;
        this.gridManager = gm;

        for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
                gm.SetCellOccupied(origin.x + dx, origin.y + dy, true);
    }
}
