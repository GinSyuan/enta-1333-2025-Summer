using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles unit selection and move commands at runtime:
/// - Left click: select a unit (changes its material color to green).
/// - Right click: if a unit is selected, set that unit's target marker to the clicked point and recalculate path.
/// </summary>
public class UnitSelector : MonoBehaviour
{
    private UnitManager unitManager;
    private int selectedUnitIndex = -1;

    private List<Renderer> lastRenderers = new List<Renderer>();
    private List<Color> lastOriginalColors = new List<Color>();

    /// <summary>
    /// Find the UnitManager in the scene at Start.
    /// </summary>
    private void Start()
    {
        unitManager = FindObjectOfType<UnitManager>();
        if (unitManager == null)
        {
            Debug.LogError("UnitSelector: No UnitManager found in the scene.");
        }
    }

    /// <summary>
    /// - On left‐mouse click: raycast and check if any unit's root transform was hit.  
    ///   If so, restore previous selected unit's color, then highlight this new unit.  
    ///   Store renderers so we can revert colors later.
    /// - On right‐mouse click: if a unit is currently selected, raycast to find world point,  
    ///   set that as the unit's target marker position, call FindPath(), and reset pathIndex.
    /// </summary>
    private void Update()
    {
        // Left click for selection
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                for (int i = 0; i < unitManager.Units.Count; i++)
                {
                    var entry = unitManager.Units[i];

                    if (hit.transform.root == entry.unitTransform)
                    {
                        // Revert the previous selected unit's color
                        if (lastRenderers.Count > 0)
                        {
                            for (int idx = 0; idx < lastRenderers.Count; idx++)
                            {
                                lastRenderers[idx].material.color = lastOriginalColors[idx];
                            }
                        }

                        lastRenderers.Clear();
                        lastOriginalColors.Clear();

                        // Highlight all renderers on the newly selected unit
                        Renderer[] renderers = entry.unitTransform.GetComponentsInChildren<Renderer>();
                        foreach (var rend in renderers)
                        {
                            lastRenderers.Add(rend);
                            lastOriginalColors.Add(rend.material.color);
                            rend.material.color = Color.green;
                        }

                        selectedUnitIndex = i;
                        Debug.Log($"Selected unit: {entry.unitTransform.name}");
                        break;
                    }
                }
            }
        }

        // Right click to issue move command to selected unit
        if (selectedUnitIndex != -1 && Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var u = unitManager.Units[selectedUnitIndex];
                u.targetTransform.position = hit.point;
                u.pathfinder.FindPath();
                u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
                u.pathIndex = 0;
                Debug.Log($"Command: Move unit {u.unitTransform.name} to {hit.point}");
            }
        }
    }
}
