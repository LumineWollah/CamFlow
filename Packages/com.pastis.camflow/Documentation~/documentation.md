# Documentation CamFlow

**CamFlow** est un toolkit de caméra pour Unity conçu pour être modulaire, facile à intégrer et agréable à utiliser immédiatement. Il permet de gérer une caméra 3D avec des contrôles de style RTS/Free-Fly, un suivi de cible fluide et des limites de mouvement (bounds).

---

## Contenu du package (Package contents)

Le package contient les éléments suivants :
* **Runtime** : Scripts principaux (`CameraController`, `CameraMotor`, etc.).
* **Documentation~** : Ce fichier de documentation.
* **Samples~** : Scènes d'exemples prêtes à l'emploi.

---

## Instructions d'installation

Ce package s'installe via le Unity Package Manager (UPM).

<p align="center">
  <img src="Images/demo_scene.png" width="600" alt="Page d'installation">
</p>

1. Ouvrez le **Package Manager** dans Unity (`Window > Package Manager`).
2. Cliquez sur le bouton **+** en haut à gauche.
3. Sélectionnez **"Add package from disk..."**.
4. Pointez vers le fichier `package.json` à la racine du dossier `com.pastis.camflow`.
5. *(Alternativement)* : Sélectionnez "Add package from git URL..." et entrez l'URL du dépôt.

---

## Prérequis (Requirements)

* **Unity Version** : 2021.3 ou supérieur recommandé.
* **Dépendance** : Ce package dépend de `com.unity.inputsystem`. Unity l'installera automatiquement s'il n'est pas présent.

---

## Limitations

* **2D** : Conçu pour la 3D, le système n'est pas optimisé pour les vues orthographiques 2D.
* **NavMesh** : Le système de limites (`CameraBounds`) utilise une simple boîte (AABB).

---

## Workflows (Démarrage Rapide)

Suivez ces étapes pour configurer la caméra :

### 1. Ajouter les composants à la main camera
- Sélectionnez `Main camera`.
- Ajoutez les composants `CameraController`, `CameraMotor`, `CameraInputProvider`, `CameraTargetFollower`.

<p align="center">
    <img src="Images/camera_controller.png" width="500" alt="Camera Controller Inspector">
    <br>
    <em>Le composant principal CameraController</em>
</p>

|                                                      **Camera Motor** & **Input Provider**                                                       |
|:------------------------------------------------------------------------------------------------------------------------------------------------:|
| <img src="Images/camera_motor.png" width="40%" alt="Camera Motor"> <img src="Images/camera_input_provider.png" width="46%" alt="Input Provider"> |

| **Target Follower** |
| :---: |
| <img src="Images/camera_target_follower.png" width="35%" alt="Target Follower">|

### 2. Configurer
- Sur `CameraMotor`, assurez-vous que `Target Camera` pointe bien vers `Main Camera`.
- Sur `CameraInputProvider`, les contrôles par défaut sont déjà actifs.
- (Optionnel) Sur un cube GameObject, désactiver le Mesh Renderer et ajouter `CameraBounds`, puis drag & drop le cube dans l'attribut `Bounds Root`

|                          **Camera Bounds**                          | 
|:-------------------------------------------------------------------:|
| <img src="Images/camera_bound.png" width="35%" alt="Camera Bounds"> |

### 3. Contrôles

| Action | Entrée |
|------|------|
| Déplacement | W / A / S / D |
| Déplacement vertical | Q / E |
| Regarder | Clic droit (RMB) + souris |
| Zoom | Molette de la souris |
| Rapide / Lent | Shift / Ctrl |
| Activer / désactiver le mode cinématique | C |

---

## Suivi de cible (Target Following)

- Ajouter `CamFlowFollowable` à n’importe quel GameObject
- Cliquer gauche en mode Play pour suivre l’objet
- Appuyer sur une touche de déplacement pour arrêter le suivi
- Clic droit (RMB) + souris pour orbiter autour de la cible

---

## Limites de caméra (Camera Bounds)

- Ajouter `CameraBounds` à la Main Camera
- Configurer la source des limites :
    - Manuellement via les attributs : Entrer un centre puis les dimensions de la Bounding Box (AABB)
    - Via un Cube GameObject : Créer un Cube, désactiver le Mesh Renderer, ajouter le transform du Cube à l’attribut `Bounds Root` du composant `CameraBounds`
- La position de la caméra est contrainte après l’application du mouvement

---

## Avancé : Virtualisation de la caméra

CamFlow permet à des scripts externes de **remplacer le contrôle de la caméra**.

### CameraCommand

Structure évaluée à chaque frame décrivant l’intention de la caméra :
- Déplacement
- Rotation / regard
- Zoom
- Modificateurs de vitesse
- Requêtes de suivi de cible

### ICamFlowDriver

```csharp
public interface ICamFlowDriver
{
    bool TryGetCommand(out CameraCommand command);
}

---

## Cas d'usage (Use Cases)

- **