using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/GridSettings")]
public class GridSettings : ScriptableObject
{
    [Header("Grid Dimensions")]
    [Tooltip("Number of columns in the grid.")]
    [SerializeField] private int gridSizeX = 10;
    [Tooltip("Number of rows in the grid.")]
    [SerializeField] private int gridSizeY = 10;

    [Header("Node Appearance")]
    [Tooltip("Size of each grid node in world units.")]
    [SerializeField] private float nodeSize = 1f;
    [Tooltip("If true, the grid lies on the XZ plane (y = 0). If false, it lies on the XY plane (z = 0).")]
    [SerializeField] private bool useXZPlane = true;

    /// <summary>
    /// Number of columns in the grid.
    /// </summary>
    public int GridSizeX => gridSizeX;

    /// <summary>
    /// Number of rows in the grid.
    /// </summary>
    public int GridSizeY => gridSizeY;

    /// <summary>
    /// Size of each node in world units.
    /// </summary>
    public float NodeSize => nodeSize;

    /// <summary>
    /// Determines whether the grid lies on XZ (true) or XY (false).
    /// </summary>
    public bool UseXZPlane => useXZPlane;
}
