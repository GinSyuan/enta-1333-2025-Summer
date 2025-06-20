using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles unit selection (click or drag box) and move commands:
/// - Click = select
/// - Ctrl+Click = multi-select
/// - Drag = box select
/// - Right click = move all selected units
/// </summary>
public class UnitSelector : MonoBehaviour
{
    private UnitManager unitManager;
    private GridManager gridManager;

    private List<int> selectedUnitIndices = new List<int>();
    private List<Renderer> lastRenderers = new List<Renderer>();
    private List<Color> lastOriginalColors = new List<Color>();

    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        unitManager = FindObjectOfType<UnitManager>();
    }

    private void Update()
    {
        HandleLeftMouse();
        HandleRightClick();
    }

    private void HandleLeftMouse()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }

       
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            dragEndPos = Input.mousePosition;

            bool isDraggingBox = Vector2.Distance(dragStartPos, dragEndPos) > 10f;
            bool isCtrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (!isCtrlHeld)
            {
                foreach (var rend in lastRenderers)
                    rend.material.color = Color.white;

                lastRenderers.Clear();
                lastOriginalColors.Clear();
                selectedUnitIndices.Clear();
            }

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


    private void HandleRightClick()
    {
        if (selectedUnitIndices.Count == 0) return;
        if (!Input.GetMouseButtonDown(1)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Mark all current unit positions as occupied
            foreach (var unit in unitManager.Units)
            {
                Vector2Int index = gridManager.GetXYIndex(unit.unitTransform.position);
                gridManager.SetCellOccupied(index.x, index.y, true);
            }

            // 1. Get center grid position
            Vector2Int centerIndex = gridManager.GetXYIndex(hit.point);
            List<Vector2Int> freeSpots = new List<Vector2Int>();

            // 2. Search 3x3 around that point for free cells
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

            // 3. If not enough spots, add center point to fill
            while (freeSpots.Count < selectedUnitIndices.Count)
            {
                freeSpots.Add(centerIndex);
            }

            // 4. Assign one spot per unit
            for (int i = 0; i < selectedUnitIndices.Count; i++)
            {
                var unitIndex = selectedUnitIndices[i];
                var u = unitManager.Units[unitIndex];

                Vector2Int gridTarget = freeSpots[i];
                Vector3 worldTarget = gridManager.GetNode(gridTarget.x, gridTarget.y).WorldPosition;

                u.targetTransform.position = worldTarget;
                u.pathfinder.FindPath();
                u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
                u.pathIndex = 0;
            }
        }
    }



    private void OnGUI()
    {
        if (isDragging)
        {
            Rect rect = Utils.GetScreenRectForGUI(dragStartPos, Input.mousePosition);
            Utils.DrawScreenRectBorder(rect, 2, Color.red);
        }
    }
}
