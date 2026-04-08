# singularity-flight
Singularity flight game
📄 README.md — Singularity Flight (Unity)

🚀 Singularity Flight
Singularity Flight is a high-speed 3D procedural tunnel runner built in Unity.
The player pilots a drone through an ever-changing industrial tunnel, avoiding obstacles and trying to survive as long as possible.

🎮 Core Concept
The drone moves forward automatically
The player controls direction only
Speed is constant
Collision results in instant death
Score is based on distance traveled
👉 Designed for fast, addictive, instant-restart gameplay

🌍 Daily Challenge System
Every day, a new tunnel is generated globally.
Generated using the current date as a seed
Same tunnel for all players
Resets at 00:00 GMT
This creates:
daily competition
leaderboard races
replayability

📱 Target Platforms
iOS
Android
Future:
Desktop
Web

🛠 Technology Stack
Engine
Unity (3D URP)
Language
C#
Backend (optional)
Firebase / Supabase
Development Style
AI-assisted (Codex-driven)

🧠 Architecture Overview
Singularity Flight uses a modular Unity architecture.
GameManager
├── InputController
├── DroneController
├── CameraFollow
├── TunnelManager
├── ObstacleSystem
├── CollisionSystem
├── DistanceTracker
└── HUDController

📂 Project Structure
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

⚙️ How to Run the Project
1. Install Unity
Download:
👉 Unity Hub
Install:
Unity Editor (latest LTS)
Android Build Support
iOS Build Support

2. Open Project
Open Unity Hub
Add project folder
Open SingularityFlight

3. Open Main Scene
Navigate to:
Assets/Scenes/Main.unity

4. Run the Game
Press ▶ Play in Unity Editor

📦 Build Instructions
Android
File → Build Settings → Android
Switch Platform
Build APK / AAB

iOS
File → Build Settings → iOS
Build → Open in Xcode
Run on device

🎯 Gameplay Systems
🚁 Drone Movement
Constant forward motion
Steering-based control
Smooth, responsive feel

🌀 Procedural Tunnel
Segment-based generation
Deterministic (seeded)
Infinite streaming system

⚠️ Obstacles
Multiple obstacle types
Fair placement rules
Increasing difficulty

💥 Collision System
Sphere-based collision
Instant crash on contact

📏 Scoring
Distance-based
Real-time update

🧾 HUD
Distance counter
Death overlay
Minimal UI

⚡ Performance Targets
The game is optimized for mobile devices.
Metric
Target
FPS
60
Draw Calls
< 120
Triangles
< 200k


🤖 AI-Driven Development
This project is designed for vibe coding with Codex.

Workflow
Prompt → Generate → Run → Verify → Fix

Rules
One system per prompt
Always test immediately
Never stack bugs
Keep systems modular

🧪 Development Roadmap
Phase 1 — Core Prototype
Drone movement
Camera system
Basic tunnel

Phase 2 — Gameplay Systems
Procedural generation
Obstacles
Collision

Phase 3 — Polish
Visual identity
Effects
Audio

Phase 4 — Online Systems
Daily challenges
Leaderboards

🔮 Future Features
Replay ghosts
Seasonal tunnels
Custom seeds
Multiplayer racing

🎨 Design Philosophy
Singularity Flight is built around:
simplicity
speed
fairness
replayability

Player Experience Goal
“I can go farther next run.”

⚠️ Important Notes
Gameplay feel > visuals
Performance is critical
Keep systems simple and modular

👨‍💻 Contributors
Developed using AI-assisted coding (Codex)

📄 License
(To be defined)

🔥 Final note
This README is intentionally:
clean
minimal
high-signal
So both:
👉 humans
👉 AI agents
can immediately understand the project.


