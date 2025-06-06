# enta-1333-2025-Summer
How to Run

1. Open the project in Unity.
2. Open the main scene (Assets -> 1333_RTS -> StudentWork -> Scenes -> SampleScenes).
3. Press Play.



Scene Setup

-GridManager: generates the walkable grid.
-ArmyManager: spawns two armies based on its settings.
-UnitManager: handles all unit pathfinding and movement.
-CameraController: lets you pan/zoom/rotate the camera.
-UnitSelector: lets you click to select and command a unit.



Configuring Armies

-Select the ArmyManager GameObject in the Hierarchy
-For each army element:
-Army Name: e.g. “PlayerArmy” or “EnemyArmy”
-Spawn Center: drag in an empty Transform (where units will line up).
-Spacing: distance between units on the X-axis (default 1.5)
-Army Material: a Material to color all units of that army
-Facing: dropdown (PositiveX, NegativeX, PositiveZ, NegativeZ)
-Unit Counts: click “+” to add roles (drag in a UnitType asset and set how many to spawn)




Controls

-Camera
    -WASD or arrow keys → pan
    -Middle-mouse drag → pan
    -Scroll wheel → zoom in/out
    -Q/E keys → rotate around Y-axis

-Unit Commands
    -Left-click on a unit to select it (console will log which unit).
    -Right-click on any ground surface (must have a Collider) to tell the selected unit where to go.

-Grid Regeneration
    -Press R to randomize the grid’s seed, rebuild walkable cells, and reset all units to their starting positions.
