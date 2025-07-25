How to Play

Top-down grid-based strategy game

Select units with click or box select

Right-click to move or attack

Place buildings during build phase 

Health bars and team colors show unit status


-----------------------------------------------------------------------------------------------------------------------------------------------


Combat System

Units scan for enemies within attack range

If enemy in range and attack cooldown ready → apply damage

Damage = subtract attacker damage from target health

If health ≤ 0 → destroy unit/building

Larger-range units move closer if enemy is too far


-----------------------------------------------------------------------------------------------------------------------------------------------



How Units Decide to Move and Attack
Scan for closest enemy in range

If no enemy in range → idle

If enemy found but out of range → pathfind closer

Uses grid-based pathfinding to avoid obstacles and other units



-----------------------------------------------------------------------------------------------------------------------------------------------


How to Run
Open the project in Unity

Go to Assets → 1333_RTS → StudentWork → Scenes and open the main scene

Press Play



-----------------------------------------------------------------------------------------------------------------------------------------------



Scene Setup
GridManager: generates the walkable grid

ArmyManager: spawns two armies based on its settings

UnitManager: handles unit pathfinding and movement

CameraController: lets you pan/zoom/rotate the camera

UnitSelector: click to select and command a unit



-----------------------------------------------------------------------------------------------------------------------------------------------


Configuring Armies
Select the ArmyManager GameObject in Hierarchy

For each army:

	Army Name (e.g. PlayerArmy or EnemyArmy)

	Spawn Center: assign an empty Transform for unit line-up

	Spacing: distance between units (default 1.5 on X-axis)

	Army Material: color for units

	Facing: choose PositiveX, NegativeX, PositiveZ, NegativeZ

	Unit Counts: use + to add roles, set UnitType asset + amount



-----------------------------------------------------------------------------------------------------------------------------------------------



Pixel Formation Details
When the player presses 1, the system:

Loads a preset image (e.g., PNG) that represents the formation

Checks each pixel in the image

Black pixels → mark positions for friendly units

Red pixels → can represent enemy positions

Converts the pixel coordinates to grid cell coordinates

Moves friendly units to align with the black pixel positions

Stores each unit’s previous position so they can return later


When the player presses C, the system:

Moves units back to their saved previous positions



-----------------------------------------------------------------------------------------------------------------------------------------------



Controls

Camera

	WASD / arrow keys → pan

	Middle-mouse drag → pan

	Scroll wheel → zoom

	Q/E → rotate around Y-axis

Unit Commands

	Left-click unit → select

	Right-click on ground (with Collider) → move selected unit

Grid Regeneration

	Press R → randomize grid seed, rebuild walkable cells, reset units
Gizmos

	Press G → toggle gizmos on/off

Pixel Formation

	Press 1 → move units to match a pixel-art formation (based on a preset image)

	Press C → cancel pixel formation, return units to their previous positions


-----------------------------------------------------------------------------------------------------------------------------------------------



Plan to Finish
Core systems (grid, placement, combat, pathfinding) complete

Next:

Improve enemy AI 
Imporve Pixel Formation system

Continue game loop work
Audio system
Main Manue 
Start new game 

