# Angry Dogs Asset Pipeline Cheat Sheet

To keep Riley's neon playground performant on mid-range mobile devices while still looking like a synthwave fever dream, import assets with the following workflow:

## 1. Source Control & Naming
1. Place raw deliveries from artists in `Assets/Art/_Source/` and keep them out of builds.
2. Convert and re-export optimized versions into platform-ready folders (e.g., `Assets/Art/Environment/`, `Assets/Art/Characters/`).
3. Use consistent prefixes per asset type to keep the Project window sorted:
   - `ENV_` for static environment meshes (skyscrapers, billboards).
   - `OBS_` for interactive obstacles (slobber cannons, hackable drones).
   - `FX_` for VFX Graph or particle prefabs (neon explosions, hound drool).
   - `DOG_` for hound models/animations (e.g., `DOG_CyberChihuahua_Idle`).

## 2. Model Import Settings
1. Enable **Read/Write** only when deformations are required (e.g., skinned hounds). Disable it for static city meshes to halve memory.
2. Set **Mesh Compression** to `Medium` for environment pieces; keep characters at `Low` to preserve snouts and rocket tails.
3. Generate lightmap UVs during import for modular buildings so that baked GI runs cleanly on PC, but keep them disabled on tiny props to save time.
4. If an artist sends 4K neon textures, downscale to 1K for mobile variants using Texture Importer presets. Use secondary detail maps for PC if needed.

## 3. Animation & Rigging
1. Store master FBX files with all takes in `Assets/Art/_Source/Animations/`.
2. Create dedicated Animator Controllers per hound archetype in `Assets/Art/Characters/Hounds/Animators/`.
3. Use retargeting via Humanoid rigs when possible so new silly animations (e.g., "butt rocket charge") drop in without recoding.
4. Bake root motion only when hounds must sync with cinematic moments; otherwise, drive forward motion through code for pooling compatibility.

## 4. VFX & Materials
1. Prefer GPU-friendly shader graphs with single texture atlases. Group neon signage and holograms into atlases to minimise draw calls.
2. Build particle systems with **Soft Particles** disabled on mobile and capped at 15 particles per system. Keep CPU-based collision off; use trigger volumes instead.
3. For stylised neon trails, use the VFX Graph on PC/console and a baked mesh trail on mobile fallback. Gate the choice with graphics tiers.

## 5. Audio & Voice Lines
1. Store quips and barks as `.wav` in `Assets/Art/Audio/_Source/`, then compress to `.ogg` in `Assets/Art/Audio/Processed/` using Unity's importer presets.
2. Batch import Riley's voice lines and name them `VO_Riley_<Context>` so designers can hook them into timelines quickly.

## 6. Prefab Variants
1. Create master prefabs in `Assets/Art/Prefabs/Master/` and create lightweight mobile variants in `Assets/Art/Prefabs/Mobile/` with simplified materials and LODs.
2. Use nested prefabs for obstacles: base mesh + effect child + collider. Designers can then swap FX variants without touching colliders.

## 7. Performance & QA Checklist
- Run the **Mesh Simplifier** or Unity's LOD Generator for any mesh visible for less than 3 seconds.
- Audit draw calls with the Frame Debugger. Aim for < 120 draw calls on mobile scenes.
- Use the **Profiler** and **Memory Profiler** every milestone; pooled hounds should not allocate when respawning.
- Document texture budgets per biome inside `Docs/Art/` so contractors know the neon limit.

Following this pipeline keeps Angry Dogs visually loud while quietly efficient.
