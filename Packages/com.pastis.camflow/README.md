# Unity Package — *CamFlow – Unity Camera Motion Toolkit*

## Pitch
A ready-to-use Unity package for **controlling a 3D camera**, designed to be **easy to integrate**, **educational**, and **extensible**.

---

## Problem & Context
In many student projects or early prototypes, the camera is often implemented quickly using project-specific scripts, which leads to:
- inconsistent behavior across scenes  
- hard-to-reconfigure controls  
- jerky or unnatural movements  
- no spatial constraints (camera going out of bounds)

This package aims to provide a **standardized, modular, and reusable foundation** to cover the most common camera management needs.

---

## Target Audience
- Course projects / Unity students  
- Prototypes and game jams  
- Top-down, RTS, city builders, exploration games  
- Debug tools or editor scenes

---

## Use Cases

1. **Prototype or Course Project**  
   When starting a new project (game or tool), the team needs a functional camera quickly without spending time rewriting a custom controller.  
   The package allows setting up a usable camera in minutes, with consistent and configurable controls.

2. **RTS / City Builder / Top-Down Game**  
   The camera needs to move freely over a map while remaining confined to the playable area.  
   The package is used to manage movement, zoom, and prevent the camera from leaving level boundaries.

3. **Game with a Main Entity to Follow**  
   In an exploration game or a third-person/top-down prototype, the camera must smoothly follow a player or a specific entity.  
   The package allows dynamically assigning a target to follow while keeping camera motion natural and stable.

4. **Exploration or Demonstration Mode**  
   For presentations, playable demos, or free exploration phases, the camera should produce smoother, more cinematic movements.  
   The package is used to enable a “cinematic” mode without changing the game’s core logic.

5. **Internal Tools and Debug Scenes**  
   In technical scenes (debugging, level inspection, asset placement), a free, reliable, and quick-to-configure camera is required.  
   The package acts as a generic scene navigation tool that can be reused across multiple projects.

6. **Educational Project**  
   The package can be used as a learning support to understand:
   - separation of responsibilities (input, movement, camera logic)
   - modular architecture in Unity
   - creating and distributing a package via UPM

---

## Core Features (MVP)
| Features |
| -------- |
| Dynamic object tracking |
| Defined movement area |
| Smooth camera motion |
| Bindable input keys |

---

## Secondary Features (Nice-to-have)
- Presets using `ScriptableObject`
- Edge scrolling (RTS-style)
- Dead zone / soft follow
- Simple obstacle collision handling
- Gizmos (bounds, target, directions)
- Events (OnTargetChanged, OnModeChanged…)
- Optional bridge with Cinemachine

---

## Non-Goals
- Fully replacing Cinemachine  
- Handling complex cinematic systems (rails, timeline, advanced blends)

---

## Architecture (Technical Overview)
- **CameraInputProvider**  
  Handles inputs and exposes normalized values.

- **CameraMotor**  
  Applies movement, speeds, and smoothing.

- **CameraTargetFollower**  
  Computes position/orientation relative to a target.

- **CameraBounds**  
  Handles spatial limits (position clamping).

- **CameraController**  
  Main facade and public API (toggle cinematic, set target, etc.).

---

## Integration
- Distributed via **Unity Package Manager** (Git URL)
- `Runtime/`, `Editor/`, `Samples~` folders
- **Camera Rig** prefab
- Example scenes:
  - Free Camera
  - Follow Camera
  - RTS Camera
- “Quick Start” documentation (< 5 minutes)

---

## Existing Solutions / Benchmark
- **Unity Cinemachine**  
  Powerful and feature-rich solution, but often too complex for simple projects.

- Open-source:
  - FreeCameraController-Unity  
  - RTSCameraController-Cinemachine  
  - ImpossibleOdds TacticalCamera  

- Unity Asset Store:
  - Universal Camera Controller (2D/3D)
  - RTS / Top-Down Camera Controller

---

## Differentiation
- Simpler and more educational than Cinemachine
- Clear and modular architecture
- Designed for learning and reuse
- Essential camera features grouped into a single, coherent toolkit

---

## Expected Deliverables
- Unity package installable via UPM
- README and documentation
- Example scenes
- One-pager (this document)
- Demo video or GIF (optional)
