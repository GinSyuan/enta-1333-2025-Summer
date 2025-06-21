using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PixelFormationController : MonoBehaviour
{
    [Header("References")]
    public Texture2D formationImage;
    public UnitManager unitManager;
    public GridManager gridManager;

    [Header("Pixel Mapping")]
    public Color soldierColor = Color.black;
    public Color enemyColor = Color.red;

    [Header("Image Settings")]
    [Tooltip("Flip the Y axis of the image vertically (useful if your formation appears upside down).")]
    public bool flipY = false;


    private List<Vector3> originalPositions = new List<Vector3>();
    private bool isInFormation = false;
    private bool showGizmos = true;

    private void Start()
    {
        foreach (var u in unitManager.Units)
        {
            originalPositions.Add(u.unitTransform.position);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ApplyFormation();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            RestoreOriginalPositions();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            showGizmos = !showGizmos;
        }
    }

    private void ApplyFormation()
    {
        if (formationImage == null || unitManager == null || gridManager == null)
        {
            Debug.LogError("Missing references on PixelFormationController.");
            return;
        }

        isInFormation = true;

        List<Vector2Int> blackPixels = new();
        List<Vector2Int> redPixels = new();

        int imgWidth = formationImage.width;
        int imgHeight = formationImage.height;

        Vector2Int imageCenter = new Vector2Int(imgWidth / 2, imgHeight / 2);
        Vector2Int gridCenter = new Vector2Int(
            gridManager.GridSettings.GridSizeX / 2,
            gridManager.GridSettings.GridSizeY / 2
        );

        for (int y = 0; y < imgHeight; y++)
        {
            for (int x = 0; x < imgWidth; x++)
            {
                Color c = formationImage.GetPixel(x, y);
                if (c.a < 0.1f) continue;

                int yIndex = flipY ? y : (imgHeight - 1 - y);
                Vector2Int offset = new Vector2Int(x, yIndex) - imageCenter;
                Vector2Int gridPos = gridCenter + offset;

                if (ColorsClose(c, soldierColor))
                    blackPixels.Add(gridPos);
                else if (ColorsClose(c, enemyColor))
                    redPixels.Add(gridPos);
            }
        }

        
        int blackIndex = 0;
        int redIndex = 0;

        foreach (var unit in unitManager.Units)
        {
            string name = unit.unitTransform.name.ToLower();

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



    private void RestoreOriginalPositions()
    {
        isInFormation = false;

        for (int i = 0; i < unitManager.Units.Count && i < originalPositions.Count; i++)
        {
            var unit = unitManager.Units[i];
            unit.targetTransform.position = originalPositions[i];
            unit.pathfinder.FindPath();
            unit.path = new List<Vector3>(unit.pathfinder.PathPositions ?? new List<Vector3>());
            unit.pathIndex = 0;
        }
    }

    private bool ColorsClose(Color a, Color b, float threshold = 0.2f)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }

    private void MoveUnitToGrid(UnitManager.UnitEntry unit, Vector2Int gridPos)
    {
        Vector3 worldPos = gridManager.GetNode(gridPos.x, gridPos.y).WorldPosition;
        unit.targetTransform.position = worldPos;
        unit.pathfinder.FindPath();
        unit.path = new List<Vector3>(unit.pathfinder.PathPositions ?? new List<Vector3>());
        unit.pathIndex = 0;
    }

    private bool IsGridValid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.y >= 0 &&
               gridPos.x < gridManager.GridSettings.GridSizeX &&
               gridPos.y < gridManager.GridSettings.GridSizeY;
    }

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
                    Gizmos.color = new Color(0f, 0f, 0f, 0.7f); // 黑色
                else if (ColorsClose(c, enemyColor))
                    Gizmos.color = new Color(1f, 0f, 0f, 0.6f); // 紅色
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
