using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private void Awake()
    {
        gridManager.InitializeGrid();
    }
}
