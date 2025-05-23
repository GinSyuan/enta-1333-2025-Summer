using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] 
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GridSettings gridSettings;
    public GridSettings GridSettings => gridSettings;  // Expose settings for other scripts

    [Header("Available Terrain Types")]
    [SerializeField] private List<TerrainType> terrainTypes = new List<TerrainType>();
    [SerializeField] private TerrainType defaultTerrainType;

    [Header("Random Seed Settings")]
    [SerializeField] private int seed = 0;           // Random seed for map generation
    [SerializeField] private float noiseScale = 0.3f; // Scale of Perlin noise sampling

    private float noiseOffsetX; // X offset based on seed
    private float noiseOffsetY; // Y offset based on seed

    private GridNode[,] gridNodes;       // 2D array holding all grid node data
    public bool IsInitialized { get; private set; }

    private void OnValidate()
    {
        // Called in the editor when a value changes in the Inspector
        InitializeGrid();
    }


    /// <summary>
    /// Builds or rebuilds the gridNodes array using Perlin noise and the current seed.
    /// </summary>
    public void InitializeGrid()
    {
        if (gridSettings == null || terrainTypes.Count == 0)
            return; // Cannot build without settings or at least one terrain type

        // Seed the random number generator and pick noise offsets
        Random.InitState(seed);
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);

        int width = gridSettings.GridSizeX;
        int height = gridSettings.GridSizeY;
        float size = gridSettings.NodeSize;

        gridNodes = new GridNode[width, height];

        // Loop through each cell in the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate world position for this node
                Vector3 worldPos = gridSettings.UseXZPlane
                    ? new Vector3(x, 0f, y) * size
                    : new Vector3(x, y, 0f) * size;

                // Sample Perlin noise at this grid point
                float noiseValue = Mathf.PerlinNoise(
                    x * noiseScale + noiseOffsetX,
                    y * noiseScale + noiseOffsetY
                );

                // Choose a terrain type based on the noise value
                TerrainType chosenType;
                int count = terrainTypes.Count;
                if (count == 0)
                {
                    chosenType = defaultTerrainType; // Fallback
                }
                else
                {
                    int index = Mathf.FloorToInt(noiseValue * count);
                    index = Mathf.Clamp(index, 0, count - 1);
                    chosenType = terrainTypes[index];
                }

                // Create and store the GridNode
                gridNodes[x, y] = new GridNode
                {
                    Name = $"Cell_{x}_{y}",
                    WorldPosition = worldPos,
                    Walkable = chosenType.Walkable,
                    Weight = chosenType.Weight,
                    TerrainType = chosenType
                };
            }
        }

        IsInitialized = true;

        // If a Pathfinder exists in the scene, trigger it to recalculate the path
        Pathfinder pf = Object.FindFirstObjectByType<Pathfinder>();
        if (pf != null)
            pf.FindPath();
    }

    
    /// Change a single node's terrain type at runtime.
    public void SetTerrainType(int x, int y, TerrainType newType)
    {
        if (!IsInitialized)
            InitializeGrid();

        if (x < 0 || x >= gridSettings.GridSizeX ||
            y < 0 || y >= gridSettings.GridSizeY)
            throw new System.IndexOutOfRangeException();

        gridNodes[x, y].TerrainType = newType;
        gridNodes[x, y].Walkable = newType.Walkable;
        gridNodes[x, y].Weight = newType.Weight;
    }

    
    /// Draw a wireframe cube for each node in the Scene view.
    /// Node color is taken from its TerrainType.
    private void OnDrawGizmos()
    {
        if (!IsInitialized || gridNodes == null)
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

    
    /// Return the GridNode at the given (x,y) grid coordinates.
    public GridNode GetNode(int x, int y)
    {
        if (gridNodes == null)
            InitializeGrid();

        if (x < 0 || x >= gridSettings.GridSizeX ||
            y < 0 || y >= gridSettings.GridSizeY)
            throw new System.IndexOutOfRangeException();

        return gridNodes[x, y];
    }

    
    /// Convert a world-space position to the nearest grid indices (x,y).
    public Vector2Int GetXYIndex(Vector3 worldPos)
    {
        float size = gridSettings.NodeSize;
        int x = Mathf.RoundToInt(worldPos.x / size);
        int y = gridSettings.UseXZPlane
            ? Mathf.RoundToInt(worldPos.z / size)
            : Mathf.RoundToInt(worldPos.y / size);
        return new Vector2Int(x, y);
    }

    
    /// Get the list of 4-directional neighbors for a given grid index.
    public List<Vector2Int> GetNeighborsXY(Vector2Int idx)
    {
        var neighbors = new List<Vector2Int>(4);
        Vector2Int[] dirs = {
            new Vector2Int( 1,  0),
            new Vector2Int(-1,  0),
            new Vector2Int( 0,  1),
            new Vector2Int( 0, -1)
        };

        foreach (var d in dirs)
        {
            int nx = idx.x + d.x, ny = idx.y + d.y;
            if (nx >= 0 && nx < gridSettings.GridSizeX &&
                ny >= 0 && ny < gridSettings.GridSizeY)
                neighbors.Add(new Vector2Int(nx, ny));
        }
        return neighbors;
    }
}
