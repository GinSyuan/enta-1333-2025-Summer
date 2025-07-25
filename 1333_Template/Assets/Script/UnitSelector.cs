using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles unit selection and movement commands:
/// - Left click = select single unit
/// - Ctrl + left click = multi-select
/// - Drag box = select multiple units
/// - Right click = move all selected units
/// </summary>
public class UnitSelector : MonoBehaviour
{
    private UnitManager unitManager;                   // Reference to the UnitManager
    private GridManager gridManager;                   // Reference to the GridManager

    private List<int> selectedUnitIndices = new();     // Indices of currently selected units
    private List<Renderer> lastRenderers = new();      // Previously highlighted renderers
    private List<Color> lastOriginalColors = new();    // Their original colors for restoring

    private Vector2 dragStartPos;                      // Mouse start position for drag selection
    private Vector2 dragEndPos;                        // Mouse end position for drag selection
    private bool isDragging = false;                   // Whether currently dragging a selection box

    private void Start()
    {
        // Find references in the scene
        gridManager = FindObjectOfType<GridManager>();
        unitManager = FindObjectOfType<UnitManager>();
    }

    private void Update()
    {
        HandleLeftMouse();  // selection logic
        HandleRightClick(); // movement command

        // Clean up selected indices that no longer exist
        for (int i = selectedUnitIndices.Count - 1; i >= 0; i--)
        {
            int idx = selectedUnitIndices[i];
            if (idx >= unitManager.Units.Count || unitManager.Units[idx].unitTransform == null)
            {
                selectedUnitIndices.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Handles left mouse button down, up, and selection logic
    /// </summary>
    private void HandleLeftMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Start drag
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            // End drag
            isDragging = false;
            dragEndPos = Input.mousePosition;

            // Check if user actually dragged
            bool isDraggingBox = Vector2.Distance(dragStartPos, dragEndPos) > 10f;

            // Check for multi-select
            bool isCtrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // If not holding ctrl, clear previous selection
            if (!isCtrlHeld)
            {
                foreach (var rend in lastRenderers)
                {
                    if (rend != null)
                        rend.material.color = Color.white;
                }

                lastRenderers.Clear();
                lastOriginalColors.Clear();
                selectedUnitIndices.Clear();
            }

            // Box select or single click select
            if (isDraggingBox)
            {
                SelectUnitsInBox(dragStartPos, dragEndPos);
            }
            else
            {
                SelectSingleUnit(Input.mousePosition, isCtrlHeld);
            }
        }
    }

    /// <summary>
    /// Select a single unit by clicking on it.
    /// </summary>
    private void SelectSingleUnit(Vector2 screenPos, bool isCtrl)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            for (int i = 0; i < unitManager.Units.Count; i++)
            {
                var entry = unitManager.Units[i];

                if (hit.transform.root == entry.unitTransform)
                {
                    if (!selectedUnitIndices.Contains(i))
                    {
                        Renderer[] renderers = entry.unitTransform.GetComponentsInChildren<Renderer>();
                        foreach (var rend in renderers)
                        {
                            lastRenderers.Add(rend);
                            lastOriginalColors.Add(rend.material.color);
                            rend.material.color = Color.green;
                        }
                        selectedUnitIndices.Add(i);
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Select units within a drag rectangle on screen.
    /// </summary>
    private void SelectUnitsInBox(Vector2 screenStart, Vector2 screenEnd)
    {
        Rect selectionRect = Utils.GetScreenRect(screenStart, screenEnd);

        foreach (var entry in unitManager.Units)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(entry.unitTransform.position);

            if (selectionRect.Contains(screenPos, true))
            {
                int index = unitManager.Units.IndexOf(entry);
                if (!selectedUnitIndices.Contains(index))
                {
                    Renderer[] renderers = entry.unitTransform.GetComponentsInChildren<Renderer>();
                    foreach (var rend in renderers)
                    {
                        lastRenderers.Add(rend);
                        lastOriginalColors.Add(rend.material.color);
                        rend.material.color = Color.green;
                    }
                    selectedUnitIndices.Add(index);
                }
            }
        }
    }

    /// <summary>
    /// Issues a move command to all selected units on right click.
    /// </summary>
    private void HandleRightClick()
    {
        if (selectedUnitIndices.Count == 0) return;
        if (!Input.GetMouseButtonDown(1)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Mark all current unit cells as occupied
            foreach (var unit in unitManager.Units)
            {
                Vector2Int index = gridManager.GetXYIndex(unit.unitTransform.position);
                gridManager.SetCellOccupied(index.x, index.y, true);
            }

            // Get center of clicked grid cell
            Vector2Int centerIndex = gridManager.GetXYIndex(hit.point);
            List<Vector2Int> freeSpots = new List<Vector2Int>();

            // Look for available cells around the center
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int candidate = new Vector2Int(centerIndex.x + dx, centerIndex.y + dy);
                    if (gridManager.IsCellFree(candidate.x, candidate.y))
                    {
                        freeSpots.Add(candidate);
                    }
                }
            }

            // If there aren't enough free spots, keep using the center
            while (freeSpots.Count < selectedUnitIndices.Count)
            {
                freeSpots.Add(centerIndex);
            }

            // Assign each selected unit to a free spot
            for (int i = 0; i < selectedUnitIndices.Count; i++)
            {
                var unitIndex = selectedUnitIndices[i];
                var u = unitManager.Units[unitIndex];

                Vector2Int gridTarget = freeSpots[i];
                Vector3 worldTarget = gridManager.GetNode(gridTarget.x, gridTarget.y).WorldPosition;

                u.targetTransform.position = worldTarget;

                // Recalculate path
                u.pathfinder.FindPath();
                u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
                u.pathIndex = 0;
            }
        }
    }

    /// <summary>
    /// Draws the selection rectangle on the screen using GUI.
    /// </summary>
    private void OnGUI()
    {
        if (isDragging)
        {
            Rect rect = Utils.GetScreenRectForGUI(dragStartPos, Input.mousePosition);
            Utils.DrawScreenRectBorder(rect, 2, Color.red);
        }
    }
}
