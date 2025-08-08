/// <summary>
/// Handles player-driven building placement on a grid.
/// Key Usage: Integrates with GridManager to validate positions; uses ghost preview.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player‐driven placement of buildings on a grid.
/// - Left mouse  : place if valid
/// - Right mouse : cancel placement
/// - Mouse scroll: rotate ghost through 4 Y‐axis orientations,
///                 preserving prefab's original pitch and roll
/// - Placement limit per building type
/// </summary>
public class BuildingPlacementManager : MonoBehaviour
{
    public bool IsPlacing { get; private set; } = false;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CameraController cameraController;

    // Use only lower-case and no-space keys here!
    [Header("Building Placement Limits")]
    public Dictionary<string, int> buildingLimits = new Dictionary<string, int>()
    {
        {"tower", 2},
        {"wall", 10},
        {"gate", 1},
        {"fence", 10},
    };
    private Dictionary<string, int> buildingPlacedCount = new Dictionary<string, int>();

    private Building ghost;
    private BuildingData selectedData;
    private int rotationIndex;
    private Vector3 baseEuler;

    private List<Renderer> _renderers;
    private List<Material[]> _originalMaterials;

    private bool firstBuildingPlaced = false;

    private Vector2Int EffectiveSize => (rotationIndex % 2 == 0)
        ? selectedData.Size
        : new Vector2Int(selectedData.Size.y, selectedData.Size.x);

/// <summary>
    /// SelectBuilding - Select or highlight items
    /// </summary>
    public void SelectBuilding(BuildingData data)
    {
        selectedData = data;
        rotationIndex = 0;
        IsPlacing = true;

        if (ghost != null)
            Destroy(ghost.gameObject);

        if (cameraController != null)
            cameraController.allowScrollZoom = false;

        GameObject go = Instantiate(selectedData.Prefab);
        ghost = go.GetComponent<Building>();

        if (go.TryGetComponent(out MageTower ghostTower))
            ghostTower.enabled = false;

        baseEuler = ghost.transform.eulerAngles;
        ghost.transform.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y, baseEuler.z);
        SetGhostMaterialTransparent(ghost);
    }

/// <summary>
    /// Update - Update state or handle per-frame logic
    /// </summary>
    private void Update()
    {
        if (ghost == null) return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f) rotationIndex = (rotationIndex + 1) % 4;
        else if (scroll < 0f) rotationIndex = (rotationIndex + 3) % 4;

        if (scroll != 0f)
        {
            float newYaw = baseEuler.y + rotationIndex * 90f;
            ghost.transform.rotation = Quaternion.Euler(baseEuler.x, newYaw, baseEuler.z);
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
            return;
        }

        UpdateGhostPositionAndColor();

        if (Input.GetMouseButtonDown(0))
        {
            string normalizedType = NormalizeKey(selectedData.BuildingName);
            if (CanPlaceGhostAt(GetClampedOrigin()) && !HasReachedPlacementLimit(normalizedType))
            {
                PlaceBuilding();
            }
            else if (HasReachedPlacementLimit(normalizedType))
            {
                Debug.Log($"{selectedData.BuildingName} has reached its placement limit!");
                // Optional: Show UI warning
            }
        }
    }

/// <summary>
    /// GetClampedOrigin - Perform this action
    /// </summary>
    private Vector2Int GetClampedOrigin()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit))
            return Vector2Int.zero;

        Vector2Int raw = gridManager.GetXYIndex(hit.point);
        raw.x = Mathf.Clamp(raw.x, 0, gridManager.GridSettings.GridSizeX - 1);
        raw.y = Mathf.Clamp(raw.y, 0, gridManager.GridSettings.GridSizeY - 1);

        int maxOX = Mathf.Max(0, gridManager.GridSettings.GridSizeX - EffectiveSize.x);
        int maxOY = Mathf.Max(0, gridManager.GridSettings.GridSizeY - EffectiveSize.y);
        int ox = Mathf.Clamp(raw.x, 0, maxOX);
        int oy = Mathf.Clamp(raw.y, 0, maxOY);
        return new Vector2Int(ox, oy);
    }

/// <summary>
    /// UpdateGhostPositionAndColor - Update state or handle per-frame logic
    /// </summary>
    private void UpdateGhostPositionAndColor()
    {
        Vector2Int origin = GetClampedOrigin();
        Vector3 basePos = gridManager.GetNode(origin.x, origin.y).WorldPosition;
        float s = gridManager.GridSettings.NodeSize;

        Vector3 pivotOffset = new Vector3(
            (EffectiveSize.x - 1) * s * 0.5f,
            0f,
            (EffectiveSize.y - 1) * s * 0.5f
        );

        ghost.transform.position = basePos + pivotOffset;

        string normalizedType = NormalizeKey(selectedData.BuildingName);
        bool canPlace = CanPlaceGhostAt(origin) && !HasReachedPlacementLimit(normalizedType);
        SetGhostColor(canPlace ? Color.green : Color.red);
    }

/// <summary>
    /// CanPlaceGhostAt - Perform this action
    /// </summary>
    private bool CanPlaceGhostAt(Vector2Int origin)
    {
        for (int dx = 0; dx < EffectiveSize.x; dx++)
            for (int dy = 0; dy < EffectiveSize.y; dy++)
                if (!gridManager.IsCellFree(origin.x + dx, origin.y + dy))
                    return false;
        return true;
    }

/// <summary>
    /// PlaceBuilding - Place objects in the scene
    /// </summary>
    private void PlaceBuilding()
    {
        Vector2Int origin = GetClampedOrigin();
        Vector3 basePos = gridManager.GetNode(origin.x, origin.y).WorldPosition;
        float s = gridManager.GridSettings.NodeSize;

        Vector3 pivotOffset = new Vector3(
            (EffectiveSize.x - 1) * s * 0.5f,
            0f,
            (EffectiveSize.y - 1) * s * 0.5f
        );

        GameObject realGo = Instantiate(
            selectedData.Prefab,
            basePos + pivotOffset,
            ghost.transform.rotation
        );

        Building realB = realGo.GetComponent<Building>();
        realB.Initialize(origin, EffectiveSize,
                         selectedData.Health, selectedData.Team, gridManager);

        if (realGo.TryGetComponent(out MageTower tower))
            tower.StartSpawning();

        AudioManager.Instance.PlayPlaceBuilding();

        // Placement count logic
        RegisterBuildingPlacement(NormalizeKey(selectedData.BuildingName));

        if (!firstBuildingPlaced)
        {
            if (FindObjectOfType<WaveManager>() is { } wm)
                wm.StartWaveTimer();
            firstBuildingPlaced = true;
        }

        // Cleanup
        if (cameraController != null) cameraController.allowScrollZoom = true;
        Destroy(ghost.gameObject);
        ghost = null;
        selectedData = null;
        IsPlacing = false;
    }

/// <summary>
    /// CancelPlacement - Perform this action
    /// </summary>
    private void CancelPlacement()
    {
        Destroy(ghost.gameObject);
        ghost = null;
        selectedData = null;
        if (cameraController != null)
            cameraController.allowScrollZoom = true;

        IsPlacing = false;
    }

/// <summary>
    /// SetGhostMaterialTransparent - Perform this action
    /// </summary>
    private void SetGhostMaterialTransparent(Building b)
    {
        _renderers = new List<Renderer>(b.GetComponentsInChildren<Renderer>());
        _originalMaterials = new List<Material[]>();

        foreach (var r in _renderers)
        {
            _originalMaterials.Add(r.materials);

            var mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(r.materials[i]);
                mats[i].SetFloat("_Mode", 3);
                mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mats[i].SetInt("_ZWrite", 0);
                mats[i].DisableKeyword("_ALPHATEST_ON");
                mats[i].EnableKeyword("_ALPHABLEND_ON");
                mats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mats[i].renderQueue = 3000;
                Color c = mats[i].color; c.a = 0.5f; mats[i].color = c;
            }
            r.materials = mats;
        }
    }

/// <summary>
    /// SetGhostColor - Perform this action
    /// </summary>
    private void SetGhostColor(Color c)
    {
        if (_renderers == null) return;
        foreach (var r in _renderers)
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Color o = mats[i].color;
                mats[i].color = new Color(c.r, c.g, c.b, o.a);
            }
            r.materials = mats;
        }
    }

    // ======== Placement Limit Methods ========
    // Always compare with normalized key
/// <summary>
    /// HasReachedPlacementLimit - Perform this action
    /// </summary>
    private bool HasReachedPlacementLimit(string buildingType)
    {
        string key = NormalizeKey(buildingType);
        if (!buildingLimits.ContainsKey(key))
            return false;
        if (!buildingPlacedCount.ContainsKey(key))
            buildingPlacedCount[key] = 0;
        return buildingPlacedCount[key] >= buildingLimits[key];
    }

/// <summary>
    /// RegisterBuildingPlacement - Perform this action
    /// </summary>
    private void RegisterBuildingPlacement(string buildingType)
    {
        string key = NormalizeKey(buildingType);
        if (!buildingPlacedCount.ContainsKey(key))
            buildingPlacedCount[key] = 0;
        buildingPlacedCount[key]++;
    }

    // Remove spaces, use lower-case for safe key comparison
/// <summary>
    /// NormalizeKey - Perform this action
    /// </summary>
    private string NormalizeKey(string name)
    {
        return name.Replace(" ", "").ToLower();
    }
}
