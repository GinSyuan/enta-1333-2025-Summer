using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Controls arranging units into a formation based on a pixel image.
/// </summary>
public class PixelFormationController : MonoBehaviour
{
    [Header("References")]
    public Texture2D formationImage;      // The formation image; each pixel defines a unit position
    public UnitManager unitManager;       // Reference to the unit manager holding all units
    public GridManager gridManager;       // Reference to the grid manager

    [Header("Pixel Mapping")]
    public Color soldierColor = Color.black;  // Color to match for soldier-type units
    public Color enemyColor = Color.red;      // Color to match for enemy-type units

    [Header("Image Settings")]
    [Tooltip("Flip the Y axis of the image vertically (useful if your formation appears upside down).")]
    public bool flipY = false;

    private List<Vector3> originalPositions = new List<Vector3>(); // Stores the original positions of units
    private bool isInFormation = false;                            // Tracks whether units are in formation
    private bool showGizmos = true;                                // Controls whether to draw gizmos for debug

    private void Start()
    {
        // Store each unit’s initial position
        foreach (var u in unitManager.Units)
        {
            originalPositions.Add(u.unitTransform.position);
        }

        showGizmos = false;
    }

    private void Update()
    {
        // Press 1 to apply formation
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ApplyFormation();
        }

        // Press C to restore original positions
        if (Input.GetKeyDown(KeyCode.C))
        {
            RestoreOriginalPositions();
        }

        // Press G to toggle gizmo display
        if (Input.GetKeyDown(KeyCode.G))
        {
            showGizmos = !showGizmos;
        }
    }

    /// <summary>
    /// Applies the pixel-based formation to the units.
    /// </summary>
    private void ApplyFormation()
    {
        // Validate references
        if (formationImage == null || unitManager == null || gridManager == null)
        {
            Debug.LogError("Missing references on PixelFormationController.");
            return;
        }

        isInFormation = true;

        List<Vector2Int> blackPixels = new();  // Stores grid positions for soldier pixels
        List<Vector2Int> redPixels = new();    // Stores grid positions for enemy pixels

        int imgWidth = formationImage.width;
        int imgHeight = formationImage.height;

        Vector2Int imageCenter = new Vector2Int(imgWidth / 2, imgHeight / 2);
        Vector2Int gridCenter = new Vector2Int(
            gridManager.GridSettings.GridSizeX / 2,
            gridManager.GridSettings.GridSizeY / 2
        );

        // Scan each pixel in the image
        for (int y = 0; y < imgHeight; y++)
        {
            for (int x = 0; x < imgWidth; x++)
            {
                Color c = formationImage.GetPixel(x, y);

                // Skip transparent pixels
                if (c.a < 0.1f) continue;

                // Optionally flip the Y coordinate
                int yIndex = flipY ? y : (imgHeight - 1 - y);
                Vector2Int offset = new Vector2Int(x, yIndex) - imageCenter;
                Vector2Int gridPos = gridCenter + offset;

                // Categorize by color
                if (ColorsClose(c, soldierColor))
                    blackPixels.Add(gridPos);
                else if (ColorsClose(c, enemyColor))
                    redPixels.Add(gridPos);
            }
        }

        int blackIndex = 0;
        int redIndex = 0;

        // Assign units to positions based on their type
        foreach (var unit in unitManager.Units)
        {
            string name = unit.unitTransform.name.ToLower();

            // Identify enemy-type units by name
            if (name.Contains("enemy") || name.Contains("grunt") || name.Contains("red"))
            {
                if (redIndex < redPixels.Count && IsGridValid(redPixels[redIndex]))
                {
                    MoveUnitToGrid(unit, redPixels[redIndex]);
                    redIndex++;
                }
            }
            else
            {
                if (blackIndex < blackPixels.Count && IsGridValid(blackPixels[blackIndex]))
                {
                    MoveUnitToGrid(unit, blackPixels[blackIndex]);
                    blackIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Restores all units to their original positions.
    /// </summary>
    private void RestoreOriginalPositions()
    {
        isInFormation = false;

        for (int i = 0; i < unitManager.Units.Count && i < originalPositions.Count; i++)
        {
            var unit = unitManager.Units[i];
            unit.targetTransform.position = originalPositions[i];

            // Recalculate pathfinding after resetting position
            unit.pathfinder.FindPath();
            unit.path = new List<Vector3>(unit.pathfinder.PathPositions ?? new List<Vector3>());
            unit.pathIndex = 0;
        }
    }

    /// <summary>
    /// Checks whether two colors are approximately close.
    /// </summary>
    private bool ColorsClose(Color a, Color b, float threshold = 0.2f)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }

    /// <summary>
    /// Moves a unit to a specified grid position.
    /// </summary>
    private void MoveUnitToGrid(UnitManager.UnitEntry unit, Vector2Int gridPos)
    {
        Vector3 worldPos = gridManager.GetNode(gridPos.x, gridPos.y).WorldPosition;
        unit.targetTransform.position = worldPos;

        // Recalculate pathfinding after moving
        unit.pathfinder.FindPath();
        unit.path = new List<Vector3>(unit.pathfinder.PathPositions ?? new List<Vector3>());
        unit.pathIndex = 0;
    }

    /// <summary>
    /// Validates if the grid position is within bounds.
    /// </summary>
    private bool IsGridValid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.y >= 0 &&
               gridPos.x < gridManager.GridSettings.GridSizeX &&
               gridPos.y < gridManager.GridSettings.GridSizeY;
    }

    /// <summary>
    /// Draws gizmos in the editor for visualizing the formation.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos || formationImage == null || gridManager == null)
            return;

        int imgWidth = formationImage.width;
        int imgHeight = formationImage.height;

        Vector2Int imageCenter = new Vector2Int(imgWidth / 2, imgHeight / 2);
        Vector2Int gridCenter = new Vector2Int(
            gridManager.GridSettings.GridSizeX / 2,
            gridManager.GridSettings.GridSizeY / 2
        );

        float size = gridManager.GridSettings.NodeSize;
        bool isXZ = gridManager.GridSettings.UseXZPlane;

        // Draw a cube gizmo for each valid pixel
        for (int y = 0; y < imgHeight; y++)
        {
            for (int x = 0; x < imgWidth; x++)
            {
                Color c = formationImage.GetPixel(x, y);

                if (c.a < 0.1f) continue;

                int yIndex = flipY ? y : (imgHeight - 1 - y);
                Vector2Int offset = new Vector2Int(x, yIndex) - imageCenter;
                Vector2Int gridPos = gridCenter + offset;

                if (gridPos.x < 0 || gridPos.y < 0 ||
                    gridPos.x >= gridManager.GridSettings.GridSizeX ||
                    gridPos.y >= gridManager.GridSettings.GridSizeY)
                    continue;

                Vector3 worldPos = gridManager.GetNode(gridPos.x, gridPos.y).WorldPosition;

                if (ColorsClose(c, soldierColor))
                    Gizmos.color = new Color(0f, 0f, 0f, 0.7f); // semi-transparent black
                else if (ColorsClose(c, enemyColor))
                    Gizmos.color = new Color(1f, 0f, 0f, 0.6f); // semi-transparent red
                else
                    continue;

                Gizmos.DrawCube(
                    worldPos + (isXZ ? Vector3.up * 0.1f : Vector3.forward * 0.1f),
                    Vector3.one * size * 0.9f
                );
            }
        }
    }
}
