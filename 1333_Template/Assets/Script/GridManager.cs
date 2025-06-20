using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("ScriptableObject containing grid dimensions and node size.")]
    [SerializeField] private GridSettings gridSettings;
    public GridSettings GridSettings => gridSettings;

    [Header("Available Terrain Types")]
    [Tooltip("List of all possible terrain types; used to assign walkability and weight per node.")]
    [SerializeField] private List<TerrainType> terrainTypes = new List<TerrainType>();
    [Tooltip("Default terrain type if terrainTypes list is empty.")]
    [SerializeField] private TerrainType defaultTerrainType;

    [Header("Random Seed Settings")]
    [Tooltip("Seed for Perlin noise. Change to get a different terrain layout.")]
    [SerializeField] private int seed = 0;
    [Tooltip("Scale factor for Perlin noise sampling.")]
    [SerializeField] private float noiseScale = 0.3f;

    [Header("Gizmos Toggle")]
    [Tooltip("Enable or disable drawing grid wireframe Gizmos at runtime.")]
    public bool showGridGizmos = true;

    private float noiseOffsetX;
    private float noiseOffsetY;
    private GridNode[,] gridNodes;
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// In Update, pressing 'G' toggles whether grid wireframe Gizmos are drawn.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            showGridGizmos = !showGridGizmos;
            Debug.Log($"Grid Gizmos = {showGridGizmos}");
        }
    }

    /// <summary>
    /// Called in the Editor when a value changes to rebuild the grid in real time.
    /// </summary>
    private void OnValidate()
    {
        InitializeGrid();
    }

    /// <summary>
    /// Rebuilds the grid based on current gridSettings, terrainTypes, and seed.  
    /// After creating all nodes, notifies any Pathfinder instances to recalculate.
    /// </summary>
    public void InitializeGrid()
    {
        if (gridSettings == null || (terrainTypes.Count == 0 && defaultTerrainType == null))
        {
            Debug.LogWarning("GridManager: Missing GridSettings or TerrainType. Skipping grid initialization.");
            return;
        }

        Random.InitState(seed);
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);

        int width = gridSettings.GridSizeX;
        int height = gridSettings.GridSizeY;
        float size = gridSettings.NodeSize;
        gridNodes = new GridNode[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Determine world position: use XZ plane or XY plane
                Vector3 pos = gridSettings.UseXZPlane
                    ? new Vector3(x, 0, y) * size
                    : new Vector3(x, y, 0) * size;

                // Sample Perlin noise to pick a terrain index
                float n = Mathf.PerlinNoise(x * noiseScale + noiseOffsetX, y * noiseScale + noiseOffsetY);
                int count = terrainTypes.Count;
                TerrainType type = (count > 0)
                    ? terrainTypes[Mathf.Clamp(Mathf.FloorToInt(n * count), 0, count - 1)]
                    : defaultTerrainType;

                gridNodes[x, y] = new GridNode
                {
                    Name = $"Cell_{x}_{y}",
                    WorldPosition = pos,
                    Walkable = type.Walkable,
                    Weight = type.Weight,
                    TerrainType = type
                };
            }
        }

        IsInitialized = true;

        // Notify all Pathfinders to recalculate their paths now that the grid changed
        foreach (var pf in FindObjectsOfType<Pathfinder>())
        {
            pf.FindPath();
        }
    }

    /// <summary>
    /// Assigns a new random seed and rebuilds the grid immediately.
    /// </summary>
    public void RandomizeSeedAndRebuild()
    {
        seed = Random.Range(0, 10000);
        InitializeGrid();
    }

    /// <summary>
    /// Returns the GridNode at grid coordinates (x, y). If grid is not built yet, it will initialize first.
    /// </summary>
    /// <param name="x">X index in the grid (0 to GridSizeX-1).</param>
    /// <param name="y">Y index in the grid (0 to GridSizeY-1).</param>
    /// <returns>GridNode at the specified indices.</returns>
    /// <exception cref="System.IndexOutOfRangeException">Thrown if indices are out of range.</exception>
    public GridNode GetNode(int x, int y)
    {
        if (gridNodes == null)
        {
            InitializeGrid();
        }

        if (x < 0 || y < 0 || x >= gridSettings.GridSizeX || y >= gridSettings.GridSizeY)
        {
            throw new System.IndexOutOfRangeException($"GridManager: Requested node ({x},{y}) is out of bounds.");
        }

        return gridNodes[x, y];
    }

    /// <summary>
    /// Converts a world position to grid indices. Rounds to nearest node center.
    /// </summary>
    /// <param name="wp">World position to convert.</param>
    /// <returns>Vector2Int of grid indices (x, y).</returns>
    public Vector2Int GetXYIndex(Vector3 wp)
    {
        float s = gridSettings.NodeSize;
        int xi = Mathf.RoundToInt(wp.x / s);
        int yi = gridSettings.UseXZPlane ? Mathf.RoundToInt(wp.z / s) : Mathf.RoundToInt(wp.y / s);
        return new Vector2Int(xi, yi);
    }

    /// <summary>
    /// Returns a list of orthogonal neighbor indices (up, down, left, right) for a given cell index.
    /// </summary>
    /// <param name="idx">Grid coordinates of the cell.</param>
    /// <returns>List of valid neighbor indices.</returns>
    public List<Vector2Int> GetNeighborsXY(Vector2Int idx)
    {
        var neighbors = new List<Vector2Int>(4);
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0),
                                    new Vector2Int(0, 1), new Vector2Int(0, -1) };

        foreach (var dir in directions)
        {
            int nx = idx.x + dir.x;
            int ny = idx.y + dir.y;
            if (nx >= 0 && ny >= 0 && nx < gridSettings.GridSizeX && ny < gridSettings.GridSizeY)
            {
                neighbors.Add(new Vector2Int(nx, ny));
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Draws wireframe cubes for each node in the Unity Editor to visualize the grid.
    /// Node color is taken from each node’s TerrainType.GizmoColor.
    /// Only draws when showGridGizmos is true.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGridGizmos || !IsInitialized || gridNodes == null)
            return;

        float size = gridSettings.NodeSize;
        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                Gizmos.color = node.TerrainType.GizmoColor;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * size * 0.9f);
            }
        }
    }


    public bool IsCellFree(int x, int y)
    {
        var node = GetNode(x, y);
        return node.Walkable && !node.Occupied;
    }

    public void SetCellOccupied(int x, int y, bool occupied)
    {
        var node = GetNode(x, y);
        node.Occupied = occupied;
        gridNodes[x, y] = node;  
    }

}
