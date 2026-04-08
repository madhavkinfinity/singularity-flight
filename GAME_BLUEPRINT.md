📄 GAME_BLUEPRINT.md (Unity Version — Production Ready)

Singularity Flight — Game Design Blueprint (Unity)

1. Game Overview
Singularity Flight is a high-speed 3D procedural tunnel runner built in Unity.
The player controls a small drone flying through an infinite industrial tunnel filled with obstacles.
Forward movement is automatic
Player controls direction only
Speed is constant
Any collision results in instant death
Score = distance traveled
The game emphasizes:
fast reactions
precision control
instant restart
high replayability

2. Core Gameplay
Core Mechanics
Drone moves forward at constant speed
Player controls steering (horizontal + vertical)
No acceleration or braking
Collision with any surface = crash
Run ends immediately
Game restarts quickly (~1 second)

Gameplay Loop
Start Run
↓
Steer Drone
↓
Avoid Obstacles
↓
Distance Increases
↓
Collision Occurs
↓
Show Score
↓
Restart Immediately
👉 The loop must feel:
instant
addictive
frustration-free

3. Player (Drone)
Design
The drone is:
small relative to tunnel
fast and responsive
visually minimal
equipped with rear thruster glow

Movement Rules
Constant forward motion
Steering controls direction only
No gravity
No acceleration system

Movement Parameters
Parameter
Value
Forward Speed
constant
Max Turn Rate
~120°/sec
Steering Smoothing
~0.1


Movement Feel Requirements
responsive
smooth
predictable
slight inertia (not twitchy)

4. Controls
Mobile (Primary)
Drag input
Mapping:
drag left/right → horizontal steering
drag up/down → vertical steering

Desktop (Fallback)
Mouse movement
WASD keys

5. Camera System
Type
Third-person chase camera

Parameters
Parameter
Value
Distance
6–8 meters
Height
1–2 meters
FOV
90–105°


Behavior
Smooth follow
Slight lag
Minimal roll
Stable (avoid motion sickness)

6. World Design
Environment
Cylindrical industrial tunnel
Sci-fi mechanical aesthetic
Procedurally generated

Visual Progression
Clean Tunnel
↓
Mechanical Structures
↓
Hazard Zones
↓
Industrial Chaos

Tunnel Properties
Twisting path
Gradually narrowing radius
Increasing difficulty

Tunnel Radius Progression
Stage
Radius
Start
~8m
Mid
~6m
Late
~4m


7. Procedural Tunnel Generation
Core Concept
The tunnel path is generated deterministically using a daily seed.

Seed System
seed = SHA256(current_date)
Same tunnel for all players per day
Changes every day at 00:00 GMT

Path Generation
Uses noise-based offsets
Horizontal and vertical variation
Example:
x_offset = noise(seed, distance)
y_offset = noise(seed + 1, distance)

Constraints
Maximum curvature per segment ≈ 5°
Must always be navigable

8. Tunnel System
Segment-Based Design
Tunnel built using reusable segments

Segment Properties
Property
Value
Length
25 meters
Active Segments
20–25


Lifecycle
Spawn segment ahead
↓
Player passes segment
↓
Despawn segment
↓
Reuse segment

Optimization Rules
Object pooling required
No runtime instantiation spikes

9. Obstacles
Types
Center pillar
Twin pillars
Narrow gate
Offset gate
Rotating bar
Zig-zag gate
Compression chamber
Rotating ring
Industrial grinder

Placement Rules
Minimum reaction time ≥ 0.5 seconds
No impossible patterns
Must always allow a valid path

Difficulty Scaling
Increase obstacle density over distance
Combine obstacle patterns gradually

10. Collision System
Collision Objects
Tunnel walls
Obstacles

Drone Collider
Sphere collider

Collision Result
Collision → Crash → End Run

11. Scoring System
Metric
Distance traveled

Behavior
Increases continuously
Based on forward speed

Display Format
2450 m

12. HUD (Unity Canvas)
Elements
Distance counter (top center)
Death overlay
Restart prompt

Design Principles
Minimal
Clean
Non-intrusive

13. Daily Challenge System
Rules
New tunnel generated daily
Same for all players

Reset Time
00:00 GMT

14. Leaderboards
Data Fields
player_id
distance
date

Types
Global leaderboard
Country leaderboard
Friends (future)

15. Visual Identity
Style
Industrial Sci-Fi

Color Palette
Steel gray
Cool blue
Hazard red
Amber lights

Effects
Glow lighting
Fog
Motion blur (later stage)

16. Audio Design
Sound Types
Drone hum
Collision impact
UI feedback

Music
Genre: industrial electronic / synthwave
Loop-based

17. Performance Targets
Required
60 FPS minimum

Budgets
Metric
Target
Draw Calls
< 120
Triangles
< 200k
Active Meshes
< 300


Optimization Techniques
Object pooling
Mesh reuse
GPU instancing
Distance fog

18. Development Strategy (AI-Driven)
Workflow
Prompt → Generate → Test → Refine

AI Responsibilities
Generate scripts
Generate shaders
Generate assets
Generate UI

Human Responsibilities
Define architecture
Validate gameplay feel
Test performance
Approve changes

19. Design Philosophy
Singularity Flight is built around:
simplicity
speed
fairness
replayability

Player Experience Goal
The player should always feel:
"I can go farther next run."

20. Future Features
Replay ghosts
Seasonal tunnels
Custom seeds
Multiplayer racing mode

🔥 Final Note
This blueprint is intentionally:
minimal in rules
strict in gameplay
optimized for iteration
👉 The game’s success depends on:
movement feel
obstacle fairness
restart speed
Not visuals.

