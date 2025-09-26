# Angry Dogs – Production Architecture & Pipelines

This document outlines a production-ready refactor plan for the **Angry Dogs** prototype. It covers architecture guidelines, feature-specific implementation notes, asset and build pipelines, as well as performance considerations for mobile and PC.

---

## 1. Architectural Overview

### 1.1 Layered, Component-Oriented Approach
- **Presentation Layer** – `MonoBehaviour` components responsible for visuals, Unity callbacks, and lightweight orchestration (e.g., `PlayerController`, `HUDController`).
- **Domain Layer** – Plain C# classes encapsulating gameplay logic (e.g., `HoundBehaviour`, weapon logic, upgrade rules). These classes are unit-testable.
- **Systems Layer** – Services accessed via dependency injection or a simple service locator (`GameBootstrapper`). Examples: input abstraction, save system, object pooling, spawn manager.
- **Data Layer** – `ScriptableObject` assets for tunable data (upgrades, enemy stats, spawn tables) + serializable POCOs for runtime state (`PlayerProgressData`).

> **Why not ECS?** The hybrid runner/shooter loop relies heavily on Unity components (animations, VFX, NavMesh). A carefully composed component-based design keeps the project approachable for designers and accommodates Unity tooling (Timeline, Animator). ECS can be selectively introduced later for extremely hot paths (e.g., crowd simulation) if profiling proves it necessary.

### 1.2 Key Patterns
- **Command Pattern for Input** – Map touch/mouse/keyboard into high-level commands consumed by controllers.
- **State Machines** – Reusable `GameStateMachine` handles menu, gameplay, pause, and game-over states. Enemy state machines remain lightweight to minimize per-frame allocations.
- **Event Aggregation** – `GameEvents` centralizes gameplay events (score, health, hound defeated) to decouple systems like UI and audio.
- **Object Pooling** – All projectiles, obstacles, and hounds should be pooled to keep allocation-free gameplay on mobile.

---

## 2. Step-by-Step Refactor Plan

1. **Bootstrap Services**
   - Implement `GameBootstrapper` prefab in the root of the main scene. It registers the input handler, object pooler, save system, and audio facade.
   - Expose initialization order and ensure components survive scene reloads.

2. **Refactor Player Flow**
   - Split responsibilities across dedicated components:
     - `PlayerController` (lightweight wiring between coordinators).
     - `PlayerMovementCoordinator` (subscribes to `InputManager` and feeds `PlayerMovementController`).
     - `PlayerShootingCoordinator` (aim + fire handling, reacts to Nibble buffs).
     - `NibbleInteractionCoordinator` (ability triggers and bark-worthy callbacks).
     - `PlayerHealthResponder` (health events, cooldown throttling, `GameEvents`).
     - `PlayerMovementController` & `PlayerShooter` keep the heavy lifting logic.
   - Route input exclusively through the shared `InputManager` to allow rebinding and platform-specific smoothing.

3. **Enhance Input Handling**
   - `InputManager` wraps the Unity Input System when present and falls back to `Input` APIs on mobile/legacy builds.
   - Provide swipe-driven dodge controls, smooth aim interpolation, and PlayerPrefs-backed key rebinding.

4. **Enemy & Obstacle Systems**
   - Create reusable `HoundAIController` with deterministic update order, grouping, and distance-based LOD.
   - Create `ObstacleSpawner` + `ObstacleRepurposer`. Keep the physics colliders simple (box/capsule) and pre-bake navmesh obstacles.

5. **UI & Game States**
   - Connect `HUDController`, `PauseMenuController`, `MainMenuController` to `GameStateMachine`.
   - Use `ScriptableObject` view models for upgrades to easily populate shop UI.

6. **Persistence**
   - Implement `SaveManager` storing JSON in `Application.persistentDataPath`. Obfuscate payload for tamper resistance and persist key bindings.

7. **Testing**
   - Extract logic from `MonoBehaviour` classes into plain classes and write `EditMode` tests for scoring, enemy waves, and upgrade calculations.
   - Use Unity Test Runner with `NUnit`.

---

## 3. Asset & Scene Organization

### 3.1 Folder Structure (`Assets/`)
```
Art/
  Characters/
  Environment/
  FX/
Audio/
Prefabs/
  Player/
  Enemies/
  Obstacles/
  UI/
Scenes/
  MainMenu.unity
  Gameplay.unity
Scripts/
  Core/
  Input/
  Player/
  Enemies/
  Systems/
  Data/
  UI/
ScriptableObjects/
  Upgrades/
  EnemyWaves/
  DifficultyCurves/
Addressables/
```

### 3.2 Asset Pipeline Tips
- **Neon Cyberpunk Models** – Keep emissive textures in a separate channel. Use HDR emissive colours with bloom in URP. Pack secondary detail maps into channel masks to reduce texture count.
- **Particle Effects** – Bake loops into SpriteSheets or VFX Graph outputs; use GPU instancing for repeated effects (muzzle flashes, neon sparks).
- **Animation Imports** – Disable unnecessary curves (scale, unused bones) to reduce data size. Use retargetable humanoid rigs for hounds if they share animations.
- **Addressables** – Group large environment sets and high-resolution textures under Addressables for progressive loading on mobile.

### 3.3 Scene Organization
- Keep a single root `Environment` object with subgroups for static meshes (`StaticGeometry`), dynamic obstacles (`DynamicGeometry`), and VFX (`CityFX`).
- Use `DontDestroyOnLoad` only for persistent managers (`GameBootstrapper`).
- Prefabs should be modular: `PlayerRoot` prefab contains child prefabs for movement, shooter, VFX, audio.

---

## 4. Performance Guidelines

### 4.1 Mobile Targets
- Limit hound skinned mesh renderers to ~15k triangles each. Provide lower LOD (static mesh impostors) for distant packs.
- Cap simultaneous particle systems; pre-warm loops and reuse them via pooling.
- Enable URP forward renderer with **dynamic resolution** (80–100%) on mobile.
- Use `LateUpdate` only when necessary; prefer `FixedUpdate` for physics and `Update` for AI.

### 4.2 Enemy Optimization
- Batch AI updates: process hounds in groups every other frame using a modulo index to reduce CPU spikes.
- Use `Physics.OverlapSphereNonAlloc` for detection cones to avoid GC.
- Disable `NavMeshAgent` when hounds perform melee to reduce path recalculations.

### 4.3 Dynamic Obstacles & VFX
- Bake occlusion culling volumes for neon skyscrapers.
- Combine static neon signage meshes by material to reduce draw calls.
- Use shader LODs to swap between full neon glow and cheap fresnel on low-end devices.

---

## 5. Build & Deployment

1. **Platform Profiles**
   - Maintain platform-specific quality presets (PC Ultra, PC Low, Mobile High, Mobile Low). Toggle bloom, shadow cascades, and dynamic resolution accordingly.

2. **Automated Builds**
   - Use Unity Cloud Build or a CI pipeline invoking `Unity -batchmode -executeMethod BuildPipeline` to output iOS/Android/PC builds.
   - Keep secrets (keystores, provisioning profiles) out of source control.

3. **Resolution & UI Scaling**
   - Canvas set to `Scale With Screen Size` and anchored for safe areas (iOS notch).
   - Provide control remapping UI for PC.

---

## 6. Future Enhancements
- Integrate analytics events (anonymized) to track upgrade usage and hound defeat rates.
- Implement daily challenges via server-configured JSON to avoid app updates.
- Investigate DOTS for large-scale hound stampedes if target count exceeds 60 concurrently.

---

## 7. Quick Checklist
- [ ] Replace prototype `PlayerController` with orchestrator + modular components.
- [ ] Migrate input to `InputManager` with smoothing.
- [ ] Pool projectiles, obstacles, hounds.
- [ ] Implement `SaveManager` with versioned JSON.
- [ ] Configure Addressables for heavy neon assets.
- [ ] Profile on mobile hardware (60 FPS, 20 hounds minimum).
- [ ] Validate UI across aspect ratios.

