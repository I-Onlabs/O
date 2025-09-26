# Angry Dogs - Cyberpunk Endless Runner

A prototype implementation of the "Angry Dogs" game concept - a chaotic, funny endless runner where you play as Riley, a clumsy hacker being chased by cybernetic hounds through Neo-Tokyo 2149.

## Game Concept

In 2149 Neo-Tokyo, you're Riley, a hacker who accidentally rewrote the AI of corporate guard dogs, turning them into "Angry Dogs 2.0"—a horde of cybernetic hounds with glowing eyes, hydraulic jaws, and a grudge. They're chasing you through a neon-lit megacity, and you must sprint, shoot obstacles to clear paths, and protect your glitchy pup, Nibble, who's the only dog still on your side.

## How to Play

1. **Open `index.html`** in a web browser
2. **Controls:**
   - Arrow Keys or WASD: Move Riley
   - Space: Shoot obstacles and Angry Dogs
   - R: Restart (when game over)

## Current Features

### ✅ Implemented
- **Core Movement:** Riley can move around the screen with arrow keys
- **Shooting System:** Space bar to shoot projectiles at obstacles and enemies
- **Nibble AI:** Your loyal dog follows you and can be damaged by enemies
- **Obstacle System:** Various cyberpunk obstacles spawn and move across the screen
- **Angry Dogs:** Hostile cybernetic hounds that chase the player
- **Collision Detection:** Projectiles hit obstacles and enemies
- **Health System:** Both Riley and Nibble have health that decreases when hit
- **Power-ups:** Collectible items that provide ammo, health, and speed boosts
- **Particle Effects:** Visual feedback for explosions and hits
- **Score System:** Points for destroying obstacles and enemies
- **Game Over:** When Riley or Nibble's health reaches zero

### 🚧 In Development
- **Advanced Obstacle Types:** The brainstormed obstacles (Bouncy Bone Drones, Slobber Mines, etc.)
- **Enhanced AI:** More sophisticated Angry Dogs behavior
- **Audio System:** Sound effects and music
- **Visual Polish:** Better graphics and animations
- **Progression System:** Unlocks and upgrades

## Game Design Document

See `ANGRY_DOGS_GDD.md` for the complete game design document with detailed mechanics, obstacles, and development roadmap.

## Technical Details

- **Platform:** HTML5 Canvas with JavaScript
- **Graphics:** 2D pixel art style with cyberpunk neon colors
- **Performance:** 60 FPS target, scalable for mobile devices
- **Browser Support:** Modern browsers with Canvas support

## Development Roadmap

1. **Phase 1:** Core prototype ✅
2. **Phase 2:** Advanced obstacle system and AI
3. **Phase 3:** Audio and visual polish
4. **Phase 4:** Mobile optimization and platform features

## Contributing

This is a prototype for the "Angry Dogs" game concept. The code is structured to be easily extensible for adding new obstacle types, enemy behaviors, and game mechanics.

## License

This prototype is for demonstration purposes. All game concepts and designs are original creative works.
## Unity Refactor Blueprint (Angry Dogs)

Production-ready C# scaffolding for the Unity prototype lives under `Assets/Scripts/`. Key highlights:

- `Core/` contains bootstrapper, events, and state machine helpers.
- `Input/InputManager` unifies legacy and new Input System controls with smoothing and key rebinding.
- `Player/` splits Riley's behaviour into movement, shooting, and Nibble support for SOLID compliance.
- `Enemies/HoundAIController` implements pooled-friendly hound logic with fear debuffs and attack throttling.
- `Systems/` hosts reusable services (object pooling, save/load, obstacle spawning and repurposing).
- `Data/` adds serializable save data and ScriptableObject upgrade definitions.
- `UI/HUDController` reacts to gameplay events for HUD updates.

See `Docs/AngryDogsArchitecture.md` for a detailed migration plan covering asset workflow, performance, and build considerations.
