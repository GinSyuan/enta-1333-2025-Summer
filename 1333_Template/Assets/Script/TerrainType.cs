using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/TerrainType")]
public class TerrainType : ScriptableObject
{
    [Header("Terrain Identification")]
    [Tooltip("Name used in menus or debugging (e.g., 'Grass', 'Water').")]
    [SerializeField] private string terrainName = "Default";

    [Header("Grid Visualization")]
    [Tooltip("Color used for Gizmo drawing to represent this terrain.")]
    [SerializeField] private Color gizmoColor = Color.green;

    [Header("Pathfinding Properties")]
    [Tooltip("If false, nodes of this terrain become obstacles.")]
    [SerializeField] private bool walkable = true;
    [Tooltip("Movement cost when traversing this terrain type.")]
    [SerializeField] private int weight = 1;

    /// <summary>
    /// Name of this terrain type for identification.
    /// </summary>
    public string TerrainName => terrainName;

    /// <summary>
    /// Color to use when drawing Gizmos for nodes of this terrain.
    /// </summary>
    public Color GizmoColor => gizmoColor;

    /// <summary>
    /// Whether this terrain can be walked on by pathfinding.
    /// </summary>
    public bool Walkable => walkable;

    /// <summary>
    /// Movement cost multiplier for this terrain.
    /// </summary>
    public int Weight => weight;
}
