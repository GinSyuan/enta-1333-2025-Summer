using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    private Building ghost;
    private BuildingData selectedData;
    private List<Renderer> _renderers;
    private List<Material[]> _originalMaterials;

    public void SelectBuilding(BuildingData data)
    {
        selectedData = data;
        if (ghost != null) Destroy(ghost.gameObject);

        
        GameObject go = Instantiate(data.Prefab);
        ghost = go.GetComponent<Building>();
        SetGhostMaterialTransparent(ghost);
    }

    private void Update()
    {
        if (ghost == null) return;

        UpdateGhostPositionAndColor();

     
        if (Input.GetMouseButtonDown(0) && CanPlaceGhostAt(GetClampedOrigin()))
            PlaceBuilding();
    }

  
    private Vector2Int GetClampedOrigin()
    {
     
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit))
            return new Vector2Int(0, 0);


        Vector2Int raw = gridManager.GetXYIndex(hit.point);


        int maxX = gridManager.GridSettings.GridSizeX - selectedData.Size.x;
        int maxY = gridManager.GridSettings.GridSizeY - selectedData.Size.y;


        int x = Mathf.Clamp(raw.x, 0, maxX);
        int y = Mathf.Clamp(raw.y, 0, maxY);
        return new Vector2Int(x, y);
    }


    private void UpdateGhostPositionAndColor()
    {
        Vector2Int origin = GetClampedOrigin();
      
        Vector3 basePos = gridManager.GetNode(origin.x, origin.y).WorldPosition;

       
        float nodeSize = gridManager.GridSettings.NodeSize;
        Vector3 pivotOffset = new Vector3(
            (selectedData.Size.x - 1) * nodeSize * 0.5f,
            0,
            (selectedData.Size.y - 1) * nodeSize * 0.5f
        );

      
        ghost.transform.position = basePos + pivotOffset;
        SetGhostColor(CanPlaceGhostAt(origin) ? Color.green : Color.red);
    }

    
    private bool CanPlaceGhostAt(Vector2Int origin)
    {
        for (int dx = 0; dx < selectedData.Size.x; dx++)
        {
            for (int dy = 0; dy < selectedData.Size.y; dy++)
            {
                int x = origin.x + dx;
                int y = origin.y + dy;
                if (!gridManager.IsCellFree(x, y))
                    return false;
            }
        }
        return true;
    }


    private void PlaceBuilding()
    {
        Vector2Int origin = GetClampedOrigin();
        Vector3 basePos = gridManager.GetNode(origin.x, origin.y).WorldPosition;

        float nodeSize = gridManager.GridSettings.NodeSize;
        Vector3 pivotOffset = new Vector3(
            (selectedData.Size.x - 1) * nodeSize * 0.5f,
            0,
            (selectedData.Size.y - 1) * nodeSize * 0.5f
        );

       
        GameObject realGo = Instantiate(
            selectedData.Prefab,
            basePos + pivotOffset,
            ghost.transform.rotation
        );

      
        Building b = realGo.GetComponent<Building>();
        b.Initialize(origin,
                     selectedData.Size,
                     selectedData.Health,
                     selectedData.Team,
                     gridManager);

        Destroy(ghost.gameObject);
        ghost = null;
    }


    private void SetGhostMaterialTransparent(Building b)
    {
        // find all renderers on the ghost
        _renderers = new List<Renderer>(b.GetComponentsInChildren<Renderer>());
        _originalMaterials = new List<Material[]>();

        foreach (var r in _renderers)
        {
            // keep a copy of the original mats
            _originalMaterials.Add(r.materials);

            // make new instances so we don’t modify the prefab’s shared materials
            var mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(r.materials[i]);
                // switch to a transparent mode (assumes Standard Shader)
                mats[i].SetFloat("_Mode", 3);                   // 3 = Transparent
                mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mats[i].SetInt("_ZWrite", 0);
                mats[i].DisableKeyword("_ALPHATEST_ON");
                mats[i].EnableKeyword("_ALPHABLEND_ON");
                mats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mats[i].renderQueue = 3000;
                // start semi-opaque so you can still see the model
                Color col = mats[i].color;
                col.a = 0.5f;
                mats[i].color = col;
            }
            // assign our new transparent materials
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
                mats[i].color = new Color(c.r, c.g, c.b, col.a); // keep the same alpha
            }
            r.materials = mats;
        }
    }
}
