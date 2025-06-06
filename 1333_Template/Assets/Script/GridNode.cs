using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single cell/node in the grid.  
/// Contains world position, walkability, weight, and a reference to the TerrainType ScriptableObject.
/// </summary>
[System.Serializable]
public struct GridNode
{
    /// <summary>
    /// Human‐readable name (e.g., "Cell_3_4").
    /// </summary>
    public string Name;

    /// <summary>
    /// World‐space position of the node's center.
    /// </summary>
    public Vector3 WorldPosition;

    /// <summary>
    /// If false, this node is considered blocked/unwalkable.
    /// </summary>
    public bool Walkable;

    /// <summary>
    /// Cost to traverse this node. Higher weight means more costly.
    /// </summary>
    public int Weight;

    /// <summary>
    /// Reference to the ScriptableObject that contains terrain data (color, walkable, weight).
    /// </summary>
    public TerrainType TerrainType;
}
