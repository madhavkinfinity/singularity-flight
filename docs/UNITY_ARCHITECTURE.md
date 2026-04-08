📄 UNITY_ARCHITECTURE.md — Singularity Flight

🧠 Singularity Flight — Unity Architecture Document

1. Purpose
This document defines the technical architecture, system boundaries, and data flow for Singularity Flight.
It ensures:
clean modular design
compatibility with AI-generated code
high performance on mobile
long-term scalability

2. Core Architecture Philosophy
Singularity Flight follows:
✅ Modular Component Architecture
✅ Data-driven systems
✅ Deterministic gameplay
✅ Performance-first design

3. High-Level System Architecture
GameManager
│
├── Input Layer
│   └── InputController
│
├── Player Layer
│   └── DroneController
│
├── Camera Layer
│   └── CameraFollow
│
├── World Layer
│   ├── TunnelManager
│   └── SegmentPool
│
├── Gameplay Layer
│   ├── ObstacleSystem
│   ├── CollisionSystem
│   └── DistanceTracker
│
└── UI Layer
   └── HUDController

4. System Responsibilities

4.1 GameManager (Central Orchestrator)
Responsibilities:
Game state control (Playing, Crashed)
System initialization
Restart logic
Global coordination

4.2 InputController
Responsibilities:
Capture touch/mouse input
Normalize input (-1 to 1)
Provide input to other systems

4.3 DroneController
Responsibilities:
Forward movement
Steering logic
Movement smoothing

4.4 CameraFollow
Responsibilities:
Track drone
Smooth follow
Maintain stable view

4.5 TunnelManager
Responsibilities:
Generate tunnel segments
Maintain active segment list
Handle procedural path
Coordinate segment pooling

4.6 SegmentPool
Responsibilities:
Reuse tunnel segments
Prevent instantiation spikes
Maintain pool size

4.7 ObstacleSystem
Responsibilities:
Spawn obstacles
Manage difficulty scaling
Ensure fair placement

4.8 CollisionSystem
Responsibilities:
Detect collisions
Trigger crash events

4.9 DistanceTracker
Responsibilities:
Track player progress
Calculate score
Reset on restart

4.10 HUDController
Responsibilities:
Display distance
Handle UI updates

5. Data Flow Architecture

Frame Execution Order
InputController → DroneController → CameraFollow → TunnelManager → ObstacleSystem → CollisionSystem → DistanceTracker → HUD

Data Dependencies
System
Depends On
DroneController
InputController
CameraFollow
DroneController
TunnelManager
DroneController
ObstacleSystem
TunnelManager
CollisionSystem
Drone + Obstacles
DistanceTracker
Drone
HUDController
DistanceTracker


6. Game Loop Mapping (Unity)

Unity Execution Model
Phase
Method
Input
Update()
Movement
Update()
Camera
LateUpdate()
Rendering
Unity


Recommended Flow
Update():
   Read input
   Update drone
   Update tunnel
   Update obstacles
   Detect collisions
   Update score

LateUpdate():
   Update camera

7. Scene Architecture

Main Scene Composition
Main Scene
│
├── GameManager
├── Drone
├── Main Camera
├── TunnelManager
├── UI Canvas
└── Lighting

Object Rules
Drone stays near origin
World moves relative to drone
Avoid large world coordinates

8. Coordinate System Strategy (IMPORTANT)

Floating Origin Technique
Instead of moving the drone infinitely:
Drone remains near (0,0,0)
Tunnel moves backward

Benefits
Prevents floating point errors
Improves stability
Simplifies procedural logic

9. Procedural Generation Architecture

Core Components
SeedGenerator
↓
PathGenerator
↓
SegmentBuilder
↓
TunnelManager

Deterministic Rules
seed = SHA256(date)
Same seed = same tunnel
No random calls without seed

Path Generation
Noise-based offsets
Limited curvature
Smooth transitions

10. Tunnel System Design

Segment Model
Each segment:
Fixed length (25 units)
Contains mesh + collider
May contain obstacles

Lifecycle
Spawn → Move → Exit view → Recycle

Pooling System
Preallocate segments
Reuse objects
Avoid Instantiate/Destroy

11. Obstacle System Architecture

Structure
ObstacleBase
├── CenterPillar
├── GateObstacle
├── RotatingObstacle

Spawn Logic
Based on distance
Based on difficulty tier

Constraints
Always leave valid path
Maintain reaction time ≥ 0.5 sec

12. Collision System Design

Detection
Unity colliders (no physics simulation)
Trigger-based or simple collision

Flow
Collision → GameManager.CrashGame() → Stop systems → Restart

13. Scoring System Architecture

Logic
distance += speed * deltaTime

Reset
Reset on crash
Persist best score

14. UI Architecture

Canvas Structure
Canvas
├── DistanceText
├── DeathOverlay

Rules
Update only when needed
Avoid heavy UI updates

15. Performance Architecture

Critical Rules
No per-frame allocations
Avoid LINQ
Avoid Find() in Update()

Optimization Systems
Object pooling
Mesh reuse
Instancing

16. Memory Management

Strategy
Preload assets
Reuse objects
Limit runtime allocations

17. Build Architecture

Android
APK / AAB output

iOS
Xcode build pipeline

18. Error Handling Strategy

Principles
Fail fast
Log clearly
Avoid silent failures

19. AI Integration Guidelines

Codex Interaction Rules
One script per prompt
No multi-system changes
Always compile-safe

Code Expectations
Modular
Documented
Testable

20. Scalability Considerations

Future Systems
Multiplayer
Ghost replay
Custom seeds

Design Requirement
Systems must remain:
loosely coupled
easily extendable

21. Anti-Patterns (STRICTLY FORBIDDEN)

❌ God classes
❌ Tight coupling
❌ Runtime instantiation spikes
❌ Hidden dependencies
❌ Non-deterministic generation

22. Final Architecture Principle
The system must always prioritize:
Performance → Gameplay Feel → Simplicity → Visuals

🔥 Final Note
This architecture is designed to:
work with AI agents
scale cleanly
maintain performance
avoid technical debt

