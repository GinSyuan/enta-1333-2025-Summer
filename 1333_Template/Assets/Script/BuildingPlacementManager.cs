using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player‐driven placement of buildings on a grid.
/// - Left mouse: place if valid
/// - Right mouse: cancel placement
/// - Mouse scroll: rotate ghost through 4 Y‐axis orientations, preserving prefab's original pitch and roll
/// </summary>
public class BuildingPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CameraController cameraController;

    private Building ghost;
    private BuildingData selectedData;
    private int rotationIndex;
    private Vector3 baseEuler; // prefab's original Euler angles

    // Transparent material state for ghost
    private List<Renderer> _renderers;
    private List<Material[]> _originalMaterials;

    /// <summary>
    /// Effective footprint size, swaps x/y when rotated 90° or 270°
    /// </summary>
    private Vector2Int EffectiveSize => (rotationIndex % 2 == 0)
        ? selectedData.Size
        : new Vector2Int(selectedData.Size.y, selectedData.Size.x);

    /// <summary>
    /// Called by UI: begins placement of a selected building type
    /// </summary>
    public void SelectBuilding(BuildingData data)
    {
        selectedData = data;
        rotationIndex = 0;

        if (ghost != null)
            Destroy(ghost.gameObject);

        if (cameraController != null)
            cameraController.allowScrollZoom = false;

        // Instantiate a "ghost" preview
        GameObject go = Instantiate(selectedData.Prefab);
        ghost = go.GetComponent<Building>();

        
        var ghostTower = go.GetComponent<MageTower>();
        if (ghostTower != null)
            ghostTower.enabled = false;

        // Record original Euler angles and make ghost transparent
        baseEuler = ghost.transform.eulerAngles;
        ghost.transform.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y, baseEuler.z);
        SetGhostMaterialTransparent(ghost);
    }

    private void Update()
    {
        if (ghost == null) return;

        
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
            rotationIndex = (rotationIndex + 1) % 4;
        else if (scroll < 0f)
            rotationIndex = (rotationIndex + 3) % 4;

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

        
        if (Input.GetMouseButtonDown(0) && CanPlaceGhostAt(GetClampedOrigin()))
            PlaceBuilding();
    }

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

    private void UpdateGhostPositionAndColor()
    {
        Vector2Int origin = GetClampedOrigin();
        Vector3 basePos = gridManager.GetNode(origin.x, origin.y).WorldPosition;
        float s = gridManager.GridSettings.NodeSize;
        Vector3 pivotOffset = new Vector3(
            (EffectiveSize.x - 1) * s * 0.5f,
            0,
            (EffectiveSize.y - 1) * s * 0.5f
        );

        ghost.transform.position = basePos + pivotOffset;
        SetGhostColor(CanPlaceGhostAt(origin) ? Color.green : Color.red);
    }

    private bool CanPlaceGhostAt(Vector2Int origin)
    {
        for (int dx = 0; dx < EffectiveSize.x; dx++)
            for (int dy = 0; dy < EffectiveSize.y; dy++)
                if (!gridManager.IsCellFree(origin.x + dx, origin.y + dy))
                    return false;
        return true;
    }

    private void PlaceBuilding()
    {
        Vector2Int origin = GetClampedOrigin();
        Vector3 basePos = gridManager.GetNode(origin.x, origin.y).WorldPosition;
        float s = gridManager.GridSettings.NodeSize;
        Vector3 pivotOffset = new Vector3(
            (EffectiveSize.x - 1) * s * 0.5f,
            0,
            (EffectiveSize.y - 1) * s * 0.5f
        );


        GameObject realGo = Instantiate(
            selectedData.Prefab,
            basePos + pivotOffset,
            ghost.transform.rotation
        );

        
        Building b = realGo.GetComponent<Building>();
        b.Initialize(origin, EffectiveSize, selectedData.Health, selectedData.Team, gridManager);

       
        var tower = realGo.GetComponent<MageTower>();
        if (tower != null)
            tower.StartSpawning();

        
        if (cameraController != null)
            cameraController.allowScrollZoom = true;

       
        Destroy(ghost.gameObject);
        ghost = null;
        selectedData = null;
    }

    private void CancelPlacement()
    {
        Destroy(ghost.gameObject);
        ghost = null;
        selectedData = null;
        if (cameraController != null)
            cameraController.allowScrollZoom = true;
    }

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
                Color col = mats[i].color; col.a = 0.5f; mats[i].color = col;
            }
            r.materials = mats;
        }
    }

    private void SetGhostColor(Color c)
    {
        if (_renderers == null) return;
        foreach (var r in _renderers)
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Color col = mats[i].color;
                mats[i].color = new Color(c.r, c.g, c.b, col.a);
            }
            r.materials = mats;
        }
    }
}
