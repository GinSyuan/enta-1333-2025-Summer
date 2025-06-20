using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines which direction an army should face when spawned.
/// </summary>
public enum FacingDirection
{
    /// <summary>Face toward the +X axis.</summary>
    PositiveX,

    /// <summary>Face toward the –X axis.</summary>
    NegativeX,

    /// <summary>Face toward the +Z axis.</summary>
    PositiveZ,

    /// <summary>Face toward the –Z axis.</summary>
    NegativeZ
}

/// <summary>
/// Spawns one or more armies at startup, based on a list of ArmyDefinition entries.
/// Each ArmyDefinition specifies a spawn center, spacing, facing direction, material override,
/// and a list of unit roles/counts.  For each unit spawned, a target marker is created and
/// a UnitEntry is added to the UnitManager for pathfinding/movement at runtime.
/// </summary>
public class ArmyManager : MonoBehaviour
{
    /// <summary>
    /// Holds a reference to a UnitType (ScriptableObject) and how many of that type to spawn.
    /// </summary>
    [System.Serializable]
    public class UnitCount
    {
        [Tooltip("The ScriptableObject that defines prefab for this role")]
        public UnitType unitType;

        [Tooltip("How many of this role to spawn in the army")]
        public int count = 1;
    }

    /// <summary>
    /// Describes a single army: its name, spawn center transform, spacing between units,
    /// optional material override, facing direction, and the list of UnitCount entries.
    /// </summary>
    [System.Serializable]
    public class ArmyDefinition
    {
        [Tooltip("A friendly name for this army (e.g. PlayerArmy, EnemyArmy)")]
        public string armyName;

        [Tooltip("The Transform around which units of this army will be spawned")]
        public Transform spawnCenter;

        [Tooltip("Spacing between units on the X-axis")]
        public float spacing = 1.5f;

        [Tooltip("Assign a unique material to visually distinguish this army")]
        public Material armyMaterial;

        [Tooltip("Which way this entire army should face when spawned")]
        public FacingDirection facing = FacingDirection.PositiveZ;

        [Tooltip("Which unit roles (UnitType) and how many of each to create")]
        public List<UnitCount> unitCounts = new List<UnitCount>();
    }

    [Tooltip("Define one entry per army (e.g. Player Army, Enemy Army)")]
    [SerializeField] private List<ArmyDefinition> armies = new List<ArmyDefinition>();

    private UnitManager unitManager;

    /// <summary>
    /// On Awake, finds the UnitManager in the scene and then spawns each army defined in 'armies'.
    /// </summary>
    private void Awake()
    {
        // Locate the UnitManager component in the scene
        unitManager = FindObjectOfType<UnitManager>();

        // For each ArmyDefinition in the list, spawn its units now
        foreach (var army in armies)
        {
            SpawnEntireArmy(army);
        }
    }

    /// <summary>
    /// Spawns all units for the given ArmyDefinition:
    /// 1. Counts total number of units across all UnitCount entries (skips any null prefabs).
    /// 2. If totalCount is zero or less, does nothing.
    /// 3. Otherwise, iterates through each UnitCount and spawns 'count' copies of that unit prefab.
    ///    - Computes an X offset so that the entire line of units is centered around spawnCenter.
    ///    - Instantiates the prefab at the computed position with the rotation determined by facing.
    ///    - Optionally applies the armyMaterial override to its Renderer.
    ///    - Creates a new GameObject as a “_TargetMarker” at the same spawn position.
    ///    - Creates a UnitEntry (unitTransform + targetTransform) and adds it to unitManager.Units.
    /// </summary>
    /// <param name="army">The ArmyDefinition that holds spawn parameters.</param>
    private void SpawnEntireArmy(ArmyDefinition army)
    {
        // STEP 1: Determine how many units in total this army will spawn
        int totalCount = 0;
        foreach (var uc in army.unitCounts)
        {
            // Only count if the UnitType and its prefab are non-null
            if (uc.unitType != null && uc.unitType.unitPrefab != null)
            {
                totalCount += uc.count;
            }
        }

        // STEP 2: If no units to spawn, exit early
        if (totalCount <= 0)
            return;

        int unitIndex = 0;
        // STEP 3: Loop through each UnitCount entry and spawn its prefabs
        foreach (var uc in army.unitCounts)
        {
            // Skip if this UnitCount has no valid prefab
            if (uc.unitType == null || uc.unitType.unitPrefab == null)
                continue;

            for (int i = 0; i < uc.count; i++)
            {
                // Compute an X offset so the line of units is centered around spawnCenter
                float offsetX = (unitIndex - (totalCount - 1) / 2.0f) * army.spacing;
                Vector3 spawnPos = new Vector3(
                    army.spawnCenter.position.x + offsetX,
                    army.spawnCenter.position.y,
                    army.spawnCenter.position.z
                );

                // Determine the correct rotation quaternion based on 'facing'
                Quaternion rotation = GetRotationForFacing(army.facing);

                // Instantiate the unit prefab at spawnPos with the chosen rotation
                GameObject unitGO = Instantiate(
                    uc.unitType.unitPrefab,
                    spawnPos,
                    rotation
                );

                // Rename the GameObject to include type name, army name, and index
                unitGO.name = $"{uc.unitType.typeName}_{army.armyName}_{unitIndex}";

                // If a material override is provided, apply it to the first Renderer found
                if (army.armyMaterial != null)
                {
                    Renderer[] renderers = unitGO.GetComponentsInChildren<Renderer>();
                    foreach (var rend in renderers)
                    {
                        rend.material = army.armyMaterial;
                    }
                }

                // Create a new empty GameObject to serve as the target marker for this unit
                GameObject marker = new GameObject($"{unitGO.name}_TargetMarker");
                marker.transform.position = spawnPos;

                // Construct a UnitEntry and add it to the UnitManager’s list for runtime pathfinding
                var entry = new UnitManager.UnitEntry
                {
                    unitTransform = unitGO.transform,
                    targetTransform = marker.transform
                };
                unitManager.Units.Add(entry);

                unitIndex++;
            }
        }
    }

    /// <summary>
    /// Given a FacingDirection enum value, returns the corresponding rotation Quaternion:
    /// - PositiveX   → rotate 90° around Y
    /// - NegativeX   → rotate –90° around Y
    /// - PositiveZ   → identity rotation (no change)
    /// - NegativeZ   → rotate 180° around Y
    /// If an unknown enum value is provided, returns Quaternion.identity as fallback.
    /// </summary>
    /// <param name="facing">Which cardinal axis the units should face.</param>
    /// <returns>Quaternion representing the desired orientation.</returns>
    private Quaternion GetRotationForFacing(FacingDirection facing)
    {
        switch (facing)
        {
            case FacingDirection.PositiveX:
                return Quaternion.Euler(0f, 90f, 0f);
            case FacingDirection.NegativeX:
                return Quaternion.Euler(0f, -90f, 0f);
            case FacingDirection.PositiveZ:
                return Quaternion.Euler(0f, 0f, 0f);
            case FacingDirection.NegativeZ:
                return Quaternion.Euler(0f, 180f, 0f);
            default:
                return Quaternion.identity;
        }
    }
}
