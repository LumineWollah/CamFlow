# CamFlow — Camera Toolkit for Unity

**CamFlow** is a modular camera toolkit for Unity, designed to be easy to integrate, extensible, and immediately usable.
It provides a 3D camera system with RTS / free-fly controls, smooth target following, movement bounds, and an extensible controller override system.

---

## Package Contents

- Runtime/  
  Core scripts (CameraController, CameraMotor, CameraInputProvider, etc.)

- Documentation~/  
  This documentation file and related images.

- Samples~/  
  Ready-to-use example scenes demonstrating common camera setups.

---

## Installation

CamFlow is installed via the Unity Package Manager (UPM).

1. Open Window → Package Manager  
2. Click the + button  
3. Select “Add package from disk...”  
4. Select the package.json file at the root of com.pastis.camflow  

Alternatively (Git):
- Select “Add package from git URL...”
- Enter the repository URL

---

## Requirements

- Unity 2021.3 LTS or newer (recommended)
- Dependency: com.unity.inputsystem

---

## Limitations

- Designed for 3D perspective cameras only
- Bounds are axis-aligned box volumes

---

## Quick Start

### 1. Camera Rig

- Create an empty GameObject named CameraRig
- Add Main Camera as a child
- Reset the camera’s local transform

---

### 2. Components

Add the following to CameraRig:

- CameraController

CamFlow expects these components on the same GameObject:
- CameraMotor
- CameraInputProvider
- CameraTargetFollower
- CameraBounds (optional)

---

### 3. Controls

| Action | Input |
|------|------|
| Move | W / A / S / D |
| Vertical move | Q / E |
| Look | RMB + Mouse |
| Zoom | Mouse Wheel |
| Fast / Slow | Shift / Ctrl |
| Toggle cinematic | C |

---

## Target Following

- Add CamFlowFollowable to any GameObject
- Left-click in Play Mode to follow
- Press a movement key to release follow
- RMB + Mouse orbits the target

---

## Camera Bounds

- Add CameraBounds to CameraRig
- Configure bounds source (manual or collider-based)
- Camera position is clamped after movement

---

## Advanced: Camera Virtualization

CamFlow allows external scripts to override camera control.

### CameraCommand

A per-frame structure describing camera intent:
- Movement
- Look
- Zoom
- Speed modifiers
- Follow requests

### ICamFlowDriver

```csharp
public interface ICamFlowDriver
{
    bool TryGetCommand(out CameraCommand command);
}
```
Si `TryGetCommand` retourne `true`, CamFlow utilise la commande fournie à la place des entrées joueur.

---

### Exemple de Driver

```csharp
using UnityEngine;
using Pastis.CamFlow;

public class MyDriver : MonoBehaviour, ICamFlowDriver
{
    public bool TryGetCommand(out CameraCommand command)
    {
        command = new CameraCommand
        {
            planarMove = new Vector2(0f, 1f),
            verticalMove = 0f,
            lookDelta = Vector2.zero,
            zoomDelta = 0f,
            fast = false,
            slow = true,
            toggleCinematic = false,
            followTarget = null,
            clearFollow = false
        };

        return true;
    }
}
```

---

### Enregistrement du driver

```csharp
void OnEnable()
{
    var controller = FindObjectOfType<CameraController>();
    controller?.SetDriver(this);
}

void OnDisable()
{
    controller?.ClearDriver();
}
```

Un seul driver peut contrôler la caméra à un instant donné.

---

## Principes de conception

- Flux clair : Input → Command → Motor
- Logique de caméra déterministe et basée sur les frames
- Les systèmes externes peuvent remplacer le contrôle en toute sécurité

---

## Résumé

CamFlow fournit :
- Une caméra libre de type RTS / free-fly
- Un suivi de cible fluide avec orbit
- Des limites de déplacement configurables
- Un système de contrôle de caméra scriptable et extensible
