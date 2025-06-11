using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingTypeso", menuName = "ScriptableObjects/BuildingTypes")]
public class BuildingTypesSo: ScriptableObject
{
    public List<BuildingData> Building = new();
}


[System.Serializable]

public class BuildingData
{
    public string BuildingName;
    public Sprite BuildingIcon;
    public GameObject Prefab;      
    public Vector2Int Size;        
    public int Health;        
    public int Team;          
}
