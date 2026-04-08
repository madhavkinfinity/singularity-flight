📄 AGENTS.md (Unity + Codex — Production Version)

Singularity Flight — AI Agent Development Guide (Unity)

1. Purpose
This document defines rules, architecture, and coding standards for AI agents contributing to the Singularity Flight Unity project.
All AI-generated code MUST follow this document.

2. Project Overview
Singularity Flight is a mobile-first 3D procedural tunnel runner built in Unity.
Core gameplay rules:
Constant forward speed
Player controls direction only
Collision = instant death
Tunnel is procedurally generated
Tunnel is seeded daily
All players experience the same daily level

3. Technology Stack
Engine
Unity (3D URP)
Language
C#
Platforms
iOS
Android
Optional Backend
Firebase / Supabase

4. Architecture Overview
The project follows a modular Unity architecture.

Core Systems
GameManager
├── InputController
├── DroneController
├── CameraFollow
├── TunnelManager
├── ObstacleSystem
├── CollisionSystem
├── DistanceTracker
└── HUDController

Responsibilities
System
Responsibility
GameManager
Game state control
InputController
Player input
DroneController
Movement
CameraFollow
Camera logic
TunnelManager
Tunnel generation
ObstacleSystem
Obstacle spawning
CollisionSystem
Crash detection
DistanceTracker
Score
HUDController
UI display


5. Folder Structure (STRICT)
Agents MUST follow this structure:
Assets/
├── Scripts/
│   ├── Core/
│   ├── Player/
│   ├── Camera/
│   ├── Tunnel/
│   ├── Obstacles/
│   └── UI/
├── Prefabs/
├── Materials/
├── Scenes/
├── UI/
├── Audio/
❌ Do NOT create new root folders
❌ Do NOT move existing structure

6. Coding Standards
General Rules
All code MUST be:
clean
readable
modular
minimal

File Requirements
Every script MUST include:
/// <ScriptName>
/// Purpose: What this script does
/// Responsibilities:
/// - item
/// - item

Code Style
Use descriptive names
Avoid deep nesting
Small functions
No unnecessary abstraction

Forbidden
❌ Monolithic scripts (>300 lines)
❌ Hidden dependencies
❌ Magic numbers (use constants)
❌ Tight coupling between systems

7. Performance Constraints
The game MUST maintain:
60 FPS on mid-range mobile

Budgets
Metric
Limit
Draw Calls
< 120
Triangles
< 200k
Active Objects
< 300


Optimization Rules
Use object pooling
Avoid frequent allocations
Reuse prefabs
Avoid Update() overuse

8. Game Loop Rules
The game loop MUST follow:
process_input
update_drone
update_camera
update_tunnel
update_obstacles
detect_collisions
update_score
render_frame

Unity Mapping
Step
Unity Method
Input
Update()
Physics
Update() (manual)
Rendering
automatic


9. Input System Rules
Input MUST be normalized (-1 to 1)
Must support:
touch
mouse

Output Format
Vector2 input; // x = horizontal, y = vertical

10. Drone Physics Rules
Forward speed = constant
Steering only (no acceleration)
Smooth interpolation required

Parameters
forward_speed = constant
max_turn_rate ≈ 120°/sec
steering_smoothing ≈ 0.1

Feel Requirements
responsive
smooth
predictable

11. Tunnel System Rules
Segment-based system
Segment length = 25 units
Active segments = 20–25

Lifecycle
spawn → move → despawn → reuse

Constraints
No runtime spikes
Must use pooling

12. Procedural Generation Rules
Seed System
seed = SHA256(date)

Determinism
Same input → same output
No randomness without seed

Path Constraints
Smooth curves
No sharp turns
Always playable

13. Obstacle System Rules
Requirements
Must always allow survival
Minimum reaction time ≥ 0.5 sec

Types
center pillar
gates
rotating hazards

Forbidden
❌ Impossible patterns
❌ Blind unavoidable obstacles

14. Collision Rules
Collider
Drone = sphere collider

Collision Result
collision → crash → restart

Restart Timing
~1 second

15. Scoring Rules
Score = distance
Continuous update
Reset on restart

16. UI Rules
Use Unity Canvas only
Minimal UI

HUD
Distance at top center

17. AI Agent Workflow (CRITICAL)
Agents MUST follow:
1. Understand task
2. Generate minimal code
3. Ensure compilation
4. Add comments
5. Return explanation

18. Prompt Rules (VERY IMPORTANT)
ALWAYS:
One system per prompt
Small scope
Test immediately

NEVER:
❌ “Build full game”
❌ Multi-system prompts
❌ Large refactors

Example Good Prompt
Create DroneController.

Requirements:
- forward movement
- steering input
- smoothing

Return script only.

19. Testing Requirements
Each system must be:
independently testable
verifiable in Unity

Required Tests
movement feel
tunnel continuity
collision accuracy

20. Code Safety Rules
Agents MUST NOT:
delete unrelated files
break working systems
introduce large dependencies

21. Future Expansion Guidelines
Design must support:
multiplayer
ghost replays
seasonal content

22. Final Principle
The primary goal of Singularity Flight is:
Fast, fair, addictive gameplay with instant restart.

Priority Order
Gameplay feel
Performance
Stability
Visuals

🔥 FINAL WARNING (Important)
If agents violate this doc:
👉 The project will become:
buggy
slow
unmaintainable
If agents follow this doc:
👉 You will:
build fast
iterate cleanly
actually ship

