using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GridSettings gridSettings;
    public GridSettings GridSettings => gridSettings;

    [Header("Available Terrain Types")]
    [SerializeField] private List<TerrainType> terrainTypes = new List<TerrainType>();
    [SerializeField] private TerrainType defaultTerrainType;

    [Header("Random Seed Settings")]
    [SerializeField] private int seed = 0;
    [SerializeField] private float noiseScale = 0.3f;

    private float noiseOffsetX;
    private float noiseOffsetY;
    private GridNode[,] gridNodes;
    public bool IsInitialized { get; private set; }

    private void OnValidate()
    {
        InitializeGrid();
    }

    /// <summary>
    /// Rebuilds the grid using current seed and notifies Pathfinders.
    /// </summary>
    public void InitializeGrid()
    {
        if (gridSettings == null || terrainTypes.Count == 0)
            return;

        Random.InitState(seed);
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);

        int width = gridSettings.GridSizeX;
        int height = gridSettings.GridSizeY;
        float size = gridSettings.NodeSize;
        gridNodes = new GridNode[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = gridSettings.UseXZPlane ? new Vector3(x, 0, y) * size : new Vector3(x, y, 0) * size;
                float n = Mathf.PerlinNoise(x * noiseScale + noiseOffsetX, y * noiseScale + noiseOffsetY);
                int count = terrainTypes.Count;
                TerrainType type = count > 0
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

        IsInitialized = true;
        foreach (var pf in FindObjectsOfType<Pathfinder>())
            pf.FindPath();
    }

    /// <summary>
    /// Randomizes seed and rebuilds.
    /// </summary>
    public void RandomizeSeedAndRebuild()
    {
        seed = Random.Range(0, 10000);
        InitializeGrid();
    }

    public GridNode GetNode(int x, int y)
    {
        if (gridNodes == null) InitializeGrid();
        if (x < 0 || y < 0 || x >= gridSettings.GridSizeX || y >= gridSettings.GridSizeY)
            throw new System.IndexOutOfRangeException();
        return gridNodes[x, y];
    }

    public Vector2Int GetXYIndex(Vector3 wp)
    {
        float s = gridSettings.NodeSize;
        int xi = Mathf.RoundToInt(wp.x / s);
        int yi = gridSettings.UseXZPlane ? Mathf.RoundToInt(wp.z / s) : Mathf.RoundToInt(wp.y / s);
        return new Vector2Int(xi, yi);
    }

    public List<Vector2Int> GetNeighborsXY(Vector2Int idx)
    {
        var n = new List<Vector2Int>(4);
        Vector2Int[] d = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
        foreach (var dir in d)
        {
            int nx = idx.x + dir.x, ny = idx.y + dir.y;
            if (nx >= 0 && ny >= 0 && nx < gridSettings.GridSizeX && ny < gridSettings.GridSizeY)
                n.Add(new Vector2Int(nx, ny));
        }
        return n;
    }

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
}
