/// <summary>
/// Handles selecting units via click or drag and issuing move commands.
/// Key Usage: Attach to a controller object; listens for mouse input and manages selection state.
/// </summary>
﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles unit selection and movement commands:
/// - Left click           : select single unit (Ctrl = multi-add)
/// - Drag box             : select multiple units
/// - Right click          : move all selected units
/// Only units with factionID == 0 can be selected or commanded.
/// </summary>
public class UnitSelector : MonoBehaviour
{
    private UnitManager unitManager;
    private GridManager gridManager;

    private readonly List<int> selectedUnitIndices = new();
    private readonly List<Renderer> lastRenderers = new();
    private readonly List<Color> lastColors = new();

    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        unitManager = FindObjectOfType<UnitManager>();
    }

    void Update()
    {
        HandleLeftMouse();
        HandleRightClick();
        CleanupStaleIndices();
    }

    /* ─────────────────────────────────────────────────────────────── */
    void HandleLeftMouse()
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

            bool dragged = Vector2.Distance(dragStartPos, dragEndPos) > 10f;
            bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (!ctrlHeld) ClearHighlight();

            if (dragged)
                SelectUnitsInBox(dragStartPos, dragEndPos);
            else
                SelectSingleUnit(Input.mousePosition, ctrlHeld);
        }
    }

    /* ───────────────── Select single unit ───────────────── */
    void SelectSingleUnit(Vector2 screenPos, bool ctrlHeld)
    {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(screenPos), out var hit)) return;

        for (int i = 0; i < unitManager.Units.Count; i++)
        {
            var entry = unitManager.Units[i];
            if (hit.transform.root != entry.unitTransform) continue;

            // ★ NEW: only add if factionID == 0
            Unit u = entry.unitTransform.GetComponent<Unit>();
            if (u == null || u.factionID != 0) return;

            if (!selectedUnitIndices.Contains(i))
            {
                Highlight(entry);
                selectedUnitIndices.Add(i);
            }
            return;
        }
    }

    /* ──────────────── Select units in drag box ──────────────── */
    void SelectUnitsInBox(Vector2 start, Vector2 end)
    {
        Rect rect = Utils.GetScreenRect(start, end);

        for (int i = 0; i < unitManager.Units.Count; i++)
        {
            var entry = unitManager.Units[i];
            Vector3 sp = Camera.main.WorldToScreenPoint(entry.unitTransform.position);

            if (!rect.Contains(sp, true)) continue;

            // ★ NEW: only add if factionID == 0
            Unit u = entry.unitTransform.GetComponent<Unit>();
            if (u == null || u.factionID != 0) continue;

            if (!selectedUnitIndices.Contains(i))
            {
                Highlight(entry);
                selectedUnitIndices.Add(i);
            }
        }
    }

    /* ───────────────── Right-click move ───────────────── */
    void HandleRightClick()
    {
        if (FindObjectOfType<BuildingPlacementManager>()?.IsPlacing == true) return;
        if (!Input.GetMouseButtonDown(1) || selectedUnitIndices.Count == 0) return;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit)) return;

        Vector2Int center = gridManager.GetXYIndex(hit.point);
        List<Vector2Int> spots = FindNearbyFreeCells(center, selectedUnitIndices.Count);

        for (int i = 0; i < selectedUnitIndices.Count; i++)
        {
            var ent = unitManager.Units[selectedUnitIndices[i]];
            if (ent.unitTransform == null) continue;

            Vector3 wp = gridManager.GetNode(spots[i].x, spots[i].y).WorldPosition;

            ent.targetTransform.position = wp;
            ent.pathfinder.FindPath();
            var p = ent.pathfinder.PathPositions;
            ent.path = p != null ? new List<Vector3>(p) : new List<Vector3>();

            ent.pathIndex = 0;
        }
    }

    /* ───────────────── Helpers ───────────────── */
    List<Vector2Int> FindNearbyFreeCells(Vector2Int center, int need)
    {
        var free = new List<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int c = new(center.x + dx, center.y + dy);
                if (gridManager.IsCellFree(c.x, c.y)) free.Add(c);
            }
        while (free.Count < need) free.Add(center);
        return free;
    }

    void Highlight(UnitManager.UnitEntry entry)
    {
        foreach (var r in entry.unitTransform.GetComponentsInChildren<Renderer>())
        {
            lastRenderers.Add(r);
            lastColors.Add(r.material.color);
            r.material.color = Color.green;
        }
    }

    void ClearHighlight()
    {
        for (int i = 0; i < lastRenderers.Count; i++)
            if (lastRenderers[i] != null)
                lastRenderers[i].material.color = lastColors[i];

        lastRenderers.Clear();
        lastColors.Clear();
        selectedUnitIndices.Clear();
    }

    void CleanupStaleIndices()
    {
        for (int i = selectedUnitIndices.Count - 1; i >= 0; i--)
        {
            int idx = selectedUnitIndices[i];
            if (idx >= unitManager.Units.Count || unitManager.Units[idx].unitTransform == null)
                selectedUnitIndices.RemoveAt(i);
        }
    }

    /* ───────────────── Draw drag box ───────────────── */
    void OnGUI()
    {
        if (isDragging)
        {
            Rect r = Utils.GetScreenRectForGUI(dragStartPos, Input.mousePosition);
            Utils.DrawScreenRectBorder(r, 2, Color.red);
        }
    }
}
