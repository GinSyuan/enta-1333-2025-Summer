using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementUI : MonoBehaviour
{

    [SerializeField] private RectTransform LayoutGroupParent;
    [SerializeField] private SelectBuildingButton ButtonPrefab;
    [SerializeField] private BuildingTypesSo BuildingData;
    [SerializeField] private BuildingPlacementManager PlacementManager;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var data in BuildingData.Building)
        {
            var button = Instantiate(ButtonPrefab, LayoutGroupParent);
            button.Setup(data);

           
            button.GetComponent<UnityEngine.UI.Button>()
                  .onClick.AddListener(() => PlacementManager.SelectBuilding(data));
        }
    }

    
}
