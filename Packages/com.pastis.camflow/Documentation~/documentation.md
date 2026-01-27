# Documentation CamFlow

**CamFlow** est un toolkit de caméra pour Unity conçu pour être modulaire, facile à intégrer et agréable à utiliser immédiatement. Il permet de gérer une caméra 3D avec des contrôles de style RTS/Free-Fly, un suivi de cible fluide et des limites de mouvement (bounds).

---

## 📦 Installation

Ce package s'installe via le Unity Package Manager (UPM).

![Page d'installation]("Images\demo scene.png")

1. Ouvrez le **Package Manager** dans Unity (`Window > Package Manager`).
2. Cliquez sur le bouton **+** en haut à gauche.
3. Sélectionnez **"Add package from disk..."**.
4. Pointez vers le fichier `package.json` à la racine du dossier `com.pastis.camflow`.
5. *(Alternativement, si c'est un package git)* : Sélectionnez "Add package from git URL..." et entrez l'URL du dépôt.

> **Dépendance** : Ce package dépend de `com.unity.inputsystem`. Unity l'installera automatiquement s'il n'est pas présent.

---

## 🚀 Démarrage Rapide

1. **Créer un Rig Caméra** :
   - Créez un objet vide nommé `CameraRig`.
   - Placez votre `Main Camera` en tant qu'enfant de ce `CameraRig`.
   - Réinitialisez la position de la caméra locale à `(0, 0, 0)` (ou ajustez selon vos besoins, par ex. reculez-la un peu).

2. **Ajouter les composants** :
   - Sélectionnez `CameraRig`.
   - Ajoutez le composant `CameraController`.
   ![Camera Controller]("../Documentation~\Images\camera_controller.png")
   - Unity ajoutera automatiquement les dépendances : `CameraMotor`, `CameraInputProvider`, `CameraTargetFollower`, et `CameraBounds`.
   ![CameraMotor]("Images\demo_scene.png")
   ![CameraInputProvider]("Images\camera_input_provider.png")
   ![CameraTargetFollower]("Images\camera_target_follower.png")
   ![CameraBound]("Images\camera_bound.png")

3. **Configurer** :
   - Sur `CameraMotor`, assurez-vous que `Target Camera` pointe bien vers votre caméra enfant.
   - Sur `CameraInputProvider`, les contrôles par défaut sont :
     - **ZQSD / WASD** : Déplacement
     - **Clic Droit + Souris** : Rotation (Yaw/Pitch)
     - **Molette** : Zoom
     - **Shift** : Accélérer
     - **Ctrl** : Ralentir
     - **C** : Basculer le mode Cinématique (lissage)

4. **Jouer** : Lancez la scène. Vous pouvez maintenant voler librement.

---

## 🧩 Composants

Voici le détail de chaque composant et ses paramètres.

### 1. CameraController
Le "cerveau" du rig. C'est le composant principal à configurer. Il fait le lien entre les entrées, le moteur de mouvement et le suivi de cible.

- **Cinematic Enabled** : Active ou désactive le lissage (smoothing) des mouvements pour un rendu plus fluide.
- **Click To Follow Enabled** : Permet de cliquer sur un objet dans la scène (clic gauche) pour que la caméra le suive automatiquement.
- **Stop Follow On Move Input** : Si coché, dès que l'utilisateur appuie sur une touche de déplacement (ZQSD), le suivi de cible s'arrête et la caméra repasse en mode libre.

### 2. CameraMotor
Gère la physique et le mouvement de la caméra.

- **Movement** : Vitesse de base, multiplicateurs pour le run (Shift) et le slow (Ctrl).
- **Rotation** : Sensibilité de la souris et limites verticales (min/max Pitch).
- **Zoom** : Vitesse du zoom et limites du champ de vision (FOV).
- **Cinematic (Smoothing)** : Temps de lissage pour la position, la rotation et le zoom. Plus la valeur est élevée, plus le mouvement est "lourd" et fluide.

### 3. CameraTargetFollower
Gère la logique de suivi d'une cible (Target).

- **Offset** : Décalage de position par rapport à la cible suivie (ex: `(0, 10, -10)` pour une vue du dessus).
- **Look At Target** : Si coché, la caméra s'orientera toujours vers la cible. Sinon, elle gardera sa rotation actuelle.

### 4. CameraBounds
Définit une zone de laquelle la caméra ne peut pas sortir.

- **Enabled** : Active ou désactive les limites.
- **Center / Size** : Définit la boîte (AABB) autorisée. Utile pour les jeux de stratégie (RTS) pour ne pas sortir de la carte.
- *Astuce : Utilisez les Gizmos dans la vue Scene pour visualiser la boîte.*

### 5. CameraInputProvider
Gère les entrées (Input System).

- Il expose les champs pour remapper les touches si besoin (ex: changer la touche de toggle cinématique).
- Par défaut, il utilise le clavier et la souris.

### 6. CamFlowFollowable
Un script utilitaire à placer sur les objets de votre scène que vous voulez rendre "cliquables" pour le suivi caméra.

- Ajoutez ce composant sur vos unités ou bâtiments.
- **Follow Transform** : (Optionnel) Si vous voulez que la caméra vise une partie spécifique (ex: la tête) plutôt que le pivot de l'objet.

---

## 🎮 Contrôles (Par défaut)

| Action | Touche(s) |
|Struture|Description|
|---|---|
| **Déplacement** | `W`, `A`, `S`, `D` (ou ZQSD selon layout) |
| **Rotation** | Maintenir `Clic Droit` + Souris |
| **Zoom** | Molette Souris |
| **Vitesse Rapide** | Maintenir `Shift` |
| **Vitesse Lente** | Maintenir `Ctrl` |
| **Mode Cinématique** | `C` (Toggle) |
| **Suivre une cible** | `Clic Gauche` sur un objet (avec `CamFlowFollowable`) |

---

## 💻 Scripting API

Vous pouvez piloter la caméra par code via le `CameraController`.

```csharp
// Récupérer le contrôleur
var camController = myRig.GetComponent<CameraController>();

// Définir une cible à suivre
camController.SetTarget(someTransform);

// Arrêter le suivi (passer null)
camController.SetTarget(null);

// Activer/Désactiver le mode cinématique
camController.SetCinematic(true);
```

### Événements et Extension

Le système est conçu pour être étendu.
- **CameraMotor** expose des méthodes comme `TickFree` et `TickFollow` si vous voulez écrire votre propre logique de contrôle tout en gardant le moteur physique.
- **CameraBounds** utilise une simple AABB pour l'instant mais peut être étendu pour des formes plus complexes.
