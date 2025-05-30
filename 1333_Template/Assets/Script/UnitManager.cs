using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [System.Serializable]
    public class UnitEntry
    {
        public Transform unitTransform;
        public Transform targetTransform;
        [HideInInspector] public Pathfinder pathfinder;
        [HideInInspector] public List<Vector3> path;
        [HideInInspector] public int pathIndex;
        [HideInInspector] public Vector3 initialPosition;
    }

    [SerializeField] private List<UnitEntry> units = new List<UnitEntry>(); //List of all units to manage.
    [SerializeField] private float moveSpeed = 2f; //Movement speed of each unit.
    private GridManager gm; // Reference to the GridManager, used for pathfinding.

    // Called once at the start: cache GridManager, initialize each unit, compute initial paths.
    void Start()
    {
        gm = FindObjectOfType<GridManager>();
        foreach (var u in units)
        {
            //Store the starting position so we can reset later.
            u.initialPosition = u.unitTransform.position;

            //Add and initialize a Pathfinder component for this unit.
            u.pathfinder = u.unitTransform.gameObject.AddComponent<Pathfinder>();
            u.pathfinder.Init(gm, u.unitTransform, u.targetTransform);

            //Compute the path now.
            u.pathfinder.FindPath();

            //Copy the computed path into our local list.
            u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
            u.pathIndex = 0;
        }
    }


    //Called once per frame: handle reseeding on R key and move each unit along its path.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Randomize seed, rebuild grid and paths, reset unit positions.
            gm.RandomizeSeedAndRebuild();
            ResetUnits();
            return;
        }

        //Move each unit toward its next path point.
        foreach (var u in units)
        {
            if (u.pathIndex >= (u.path?.Count ?? 0)) continue;
            Vector3 next = u.path[u.pathIndex];
            u.unitTransform.position = Vector3.MoveTowards(u.unitTransform.position, next, moveSpeed * Time.deltaTime);
            
            // Once close enough, advance to the next waypoint.
            if (Vector3.Distance(u.unitTransform.position, next) < 0.05f)
                u.pathIndex++;
        }
    }


    // Resets all units back to their starting positions and recalculates paths.
    private void ResetUnits()
    {
        foreach (var u in units)
        {
            u.unitTransform.position = u.initialPosition;
            u.pathfinder.FindPath();
            u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
            u.pathIndex = 0;
        }
    }
}
