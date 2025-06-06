using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A ScriptableObject that defines a type of unit in the game.
/// You can create new UnitType assets via the Unity Editor (Assets → Create → Game → UnitType).
/// </summary>
[CreateAssetMenu(fileName = "UnitType", menuName = "Game/UnitType")]
public class UnitType : ScriptableObject
{
    /// <summary>
    /// A human‐readable name for this unit type (e.g., "Infantry", "Archer", "Cavalry").
    /// </summary>
    [Tooltip("A human‐readable name for this unit type.")]
    public string typeName;

    /// <summary>
    /// The prefab to instantiate when spawning a unit of this type.
    /// The root GameObject of this prefab must have a Collider so that the unit can be selected by the player.
    /// </summary>
    [Tooltip("Prefab to instantiate for this unit type. Root GameObject must have a Collider for selection purposes.")]
    public GameObject unitPrefab;
}
