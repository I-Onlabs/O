# Angry Dogs Unity Codebase – Analysis

## Architectural Skeleton
- A persistent `GameBootstrapper` prefab spins up cross-scene services such as the input manager, pooled object factory, and optional save manager to keep runtime singletons deterministic without manual scene wiring.【F:Assets/Scripts/Core/GameBootstrapper.cs†L8-L49】
- Global `GameEvents` publish score, health, hound pack counts, and pause/game-over notifications so UI and audio layers remain decoupled from gameplay controllers.【F:Assets/Scripts/Core/GameEvents.cs†L5-L24】
- A lightweight `GameStateMachine` coordinates transitions between boot, menu, gameplay, pause, shop, and game-over states while allowing systems to register enter/exit hooks for clean separation.【F:Assets/Scripts/Core/GameStateMachine.cs†L6-L65】
- The shipped architecture blueprint in `Docs/AngryDogsArchitecture.md` reinforces this layered approach (presentation, systems, data) and lists the coordinator components used across the player, enemy, and UI stacks.【F:Docs/AngryDogsArchitecture.md†L7-L58】

## Core Systems Overview
### Input Layer
- `InputManager` unifies keyboard, mouse, controller, and touch gestures with smoothing, emitting movement/aim vectors plus fire and ability events that downstream components subscribe to; it also persists custom bindings via the save manager or PlayerPrefs on platforms without persistence.【F:Assets/Scripts/Input/InputManager.cs†L12-L377】
- Legacy `PlayerInputHandler` remains for migration but is flagged obsolete in favour of the new service-based flow.【F:Assets/Scripts/Input/PlayerInputHandler.cs†L10-L162】

### Persistence
- `SaveManager` stores progress and settings as JSON (with optional XOR obfuscation), tracks keybindings through PlayerPrefs, and surfaces helper methods for run results, upgrade unlocks, and reset flows with WebGL fallbacks.【F:Assets/Scripts/SaveSystem/SaveManager.cs†L10-L296】
- Progress/settings payloads are split into `PlayerProgressData` for high scores, soft currency, and unlocked upgrades, and `PlayerSettingsData` for audio sliders, handedness, and serialized keybinding maps.【F:Assets/Scripts/Data/PlayerProgressData.cs†L7-L87】【F:Assets/Scripts/Data/PlayerSettingsData.cs†L7-L122】

### Player Stack
- `PlayerController` now only orchestrates dedicated coordinators for movement, shooting, health reactions, and Nibble interactions, wiring events without owning gameplay logic directly.【F:Assets/Scripts/Player/PlayerController.cs†L5-L92】
- `PlayerMovementCoordinator` bridges the shared input stream into the `PlayerMovementController`, which handles autorun physics, lane lerps, and gravity via a `CharacterController` for mobile-friendly dodging.【F:Assets/Scripts/Player/PlayerMovementCoordinator.cs†L6-L54】【F:Assets/Scripts/Player/PlayerMovementController.cs†L5-L78】
- `PlayerShootingCoordinator` and `PlayerShooter` convert aim vectors into forward rotation, spawn pooled projectiles, and accelerate fire rate when Nibble barks encouragement.【F:Assets/Scripts/Player/PlayerShootingCoordinator.cs†L6-L78】【F:Assets/Scripts/Player/PlayerShooter.cs†L7-L106】
- Companion-specific behaviours live in `NibbleInteractionCoordinator` and `NibbleCompanionController`, which translate ability inputs into crowd-control barks that fear nearby hounds and provide barky feedback when Riley is hurt.【F:Assets/Scripts/Player/NibbleInteractionCoordinator.cs†L6-L68】【F:Assets/Scripts/Player/NibbleCompanionController.cs†L8-L72】
- Health notifications flow through `PlayerHealthResponder`, throttling repeated events, relaying damage to `GameEvents`, and signaling game over transitions for coordinators to react to.【F:Assets/Scripts/Player/PlayerHealthResponder.cs†L8-L88】

### Enemies
- `HoundAIController` wraps a pooled-friendly NavMesh agent with a tiny state machine that staggers chasing logic across frames, throttles repaths, and applies attack/fear states while updating pack counts for the HUD.【F:Assets/Scripts/Enemies/HoundAIController.cs†L9-L283】

### Obstacles, Projectiles, and Pooling
- `ObstacleManager` owns spawn definitions, weighted selection, pooling, and repurposing (turning destroyed props into traps) while warning when active counts hit the configured ceiling to flag draw-call risks.【F:Assets/Scripts/Obstacles/ObstacleManager.cs†L8-L257】
- `WeightedObstaclePicker` precomputes cumulative weights to give deterministic sampling without allocations, and edit-mode tests assert the selection behaviour and repurposing pipeline.【F:Assets/Scripts/Obstacles/WeightedObstaclePicker.cs†L7-L62】 【F:Assets/Tests/EditMode/ObstacleManagerTests.cs†L9-L110】
- `ObstacleRepurposer`, `ObstacleSpawner`, and the generic `ObjectPooler`/`Projectile` combo round out the reuse story by re-skinning obstacles into traps, streaming lane obstacles based on difficulty curves, and recycling projectiles/props without GC pressure.【F:Assets/Scripts/Systems/ObstacleRepurposer.cs†L5-L40】【F:Assets/Scripts/Systems/ObstacleSpawner.cs†L6-L99】【F:Assets/Scripts/Systems/ObjectPooler.cs†L6-L118】【F:Assets/Scripts/Systems/Projectile.cs†L6-L71】

### UI & Game Flow
- `UIManager` swaps between main menu, HUD, pause, and upgrade screens with CanvasGroup fades, reacts to save-loaded quips/high score updates, and relays pause toggles through `GameEvents` for platform-neutral handling.【F:Assets/Scripts/UI/UIManager.cs†L10-L248】
- `HUDController` listens to the same global events to update score, health bars, and the hound pack counter, keeping the HUD passive and data-driven.【F:Assets/Scripts/UI/HUDController.cs†L7-L64】

### Tooling & Pipelines
- The editor-only `AssetImporter` enforces mesh compression, texture presets (ASTC for mobile), and auto-adds lightweight LODGroups for neon skyscrapers, plus warns on oversized imports to keep mobile builds lean.【F:Assets/Scripts/Tools/AssetImporter.cs†L8-L138】
- Asset-pipeline and architecture cheat sheets in `Docs/` provide actionable guidance for folder structure, LOD budgets, naming, and performance targets, complementing the in-code tooling.【F:Docs/AssetPipeline.md†L1-L45】 【F:Docs/AngryDogsArchitecture.md†L62-L156】

## Strengths Observed
- Responsibilities are consistently isolated—coordinators subscribe to `InputManager` and relay into focused controllers, minimising duplication and easing unit testing as advocated in the architecture plan.【F:Docs/AngryDogsArchitecture.md†L31-L39】 【F:Assets/Scripts/Player/PlayerMovementCoordinator.cs†L6-L54】
- Object pooling is pervasive (obstacles, projectiles, hounds), reducing per-frame allocations that could tank mobile frame times.【F:Assets/Scripts/Systems/ObjectPooler.cs†L6-L118】 【F:Assets/Scripts/Obstacles/ObstacleManager.cs†L57-L205】
- Performance guardrails (staggered AI updates, spawn caps, fade-based UI transitions) already target 20+ hounds and mobile-friendly draw-call budgets.【F:Assets/Scripts/Enemies/HoundAIController.cs†L103-L176】 【F:Assets/Scripts/Obstacles/ObstacleManager.cs†L103-L177】 【F:Assets/Scripts/UI/UIManager.cs†L182-L218】

## Risks & Opportunities
- Save obfuscation uses a simple XOR mask before Base64 encoding; while enough to deter casual tampering, sensitive leaderboards or live ops might need real encryption or server validation later.【F:Assets/Scripts/SaveSystem/SaveManager.cs†L241-L285】
- `ObstacleRepurposer` only checks a single layer mask and applies a fixed scale, so designers wanting richer trap customisation (FX callbacks, dynamic scaling) will need extension points or events added.【F:Assets/Scripts/Systems/ObstacleRepurposer.cs†L11-L39】
- `InputManager` assumes a single active touch for dodge/aim; multi-touch gestures or split-stick virtual controls would require expanding its sampling logic and rebinding UI flows.【F:Assets/Scripts/Input/InputManager.cs†L199-L307】
- Hound AI currently disables rotation updates for cheaper animation-driven steering; if future behaviours require tight turning or strafing, revisit NavMeshAgent settings and consider blend-tree driven rotation cost.【F:Assets/Scripts/Enemies/HoundAIController.cs†L57-L133】
