using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitType", menuName = "Game/UnitType")]
public class UnitType : ScriptableObject
{
    [Tooltip("A name for this unit type.")]
    public string typeName;

    [Tooltip("Prefab to instantiate for this unit type. Root GameObject must have a Collider for selection purposes.")]
    public GameObject unitPrefab;
}
