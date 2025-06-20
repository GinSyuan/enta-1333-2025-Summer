using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectBuildingButton : MonoBehaviour
{
    [SerializeField] private Image _buttonImage;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private Button _button;

    private BuildingData _buildingDataForButton;

    public void Setup(BuildingData buildingData)
    {
        _buildingDataForButton = buildingData;

        _buttonText.text = _buildingDataForButton.BuildingName;

        _buttonImage.sprite = _buildingDataForButton.BuildingIcon;
    }

 
}
