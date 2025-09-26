# Angry Dogs - Cyberpunk Endless Runner

A production-ready Unity prototype of the "Angry Dogs" game concept - a chaotic, funny endless runner where you play as Riley, a clumsy hacker being chased by cybernetic hounds through Neo-Tokyo 2149. Now featuring cloud saves, boss encounters, and mobile optimization!

## Game Concept

In 2149 Neo-Tokyo, you're Riley, a hacker who accidentally rewrote the AI of corporate guard dogs, turning them into "Angry Dogs 2.0"—a horde of cybernetic hounds with glowing eyes, hydraulic jaws, and a grudge. They're chasing you through a neon-lit megacity, and you must sprint, shoot obstacles to clear paths, and protect your glitchy pup, Nibble, who's the only dog still on your side.

## How to Play

### Unity Version (Recommended)
1. **Open the project in Unity 2022.3 LTS or later**
2. **Controls:**
   - Arrow Keys or WASD: Move Riley
   - Space: Shoot obstacles and Angry Dogs
   - R: Restart (when game over)
   - P: Pause/Resume
   - Settings: Access performance and audio settings

### Web Version (Legacy)
1. **Open `index.html`** in a web browser
2. **Same controls as Unity version**

## Current Features

### ✅ Production-Ready Features

#### **Core Gameplay**
- **Core Movement:** Riley can move around with arrow keys/WASD
- **Shooting System:** Space bar to shoot projectiles at obstacles and enemies
- **Nibble AI:** Your loyal dog follows you and can be damaged by enemies
- **Health System:** Both Riley and Nibble have health that decreases when hit
- **Score System:** Points for destroying obstacles and enemies
- **Game Over:** When Riley or Nibble's health reaches zero

#### **Advanced Obstacle System** 🆕
- **Kibble Vending Bots:** Spawn sticky pellets that make hounds slip comically
- **Holo-Paw Prints:** Create decoy trails to misguide hounds away from Nibble
- **Slobber Cannons:** Classic obstacles that can be repurposed into slime traps
- **Weighted Spawning:** Deterministic obstacle selection for balanced gameplay
- **Object Pooling:** Optimized performance with obstacle recycling

#### **Boss Encounters** 🆕
- **Cyber-Chihuahua King:** Tiny hound in a massive mech-suit with weak points
- **Treat Tantrum Defense:** Rage mode with increased damage and speed
- **Dynamic Arenas:** ObstacleManager integration for epic boss fight environments
- **Weak Point System:** Strategic targeting for maximum damage
- **Mobile Optimized:** 60 FPS performance on mid-range devices

#### **Save System** 🆕
- **Cloud Syncing:** Cross-device progress synchronization
- **Offline Fallback:** Play without internet, sync when available
- **JSON Integrity:** Validation to prevent save corruption
- **Settings Persistence:** Audio, haptics, and key bindings saved
- **Smart Merging:** Preserves highest scores and unlocks across devices

#### **Mobile Optimization** 🆕
- **60 FPS Performance:** Optimized for mid-range mobile devices
- **Settings Menu:** Toggle neon effects and performance options
- **Reduced Draw Calls:** Optimized UI and texture management
- **Memory Management:** Automatic texture size optimization
- **Canvas Optimization:** Throttled updates for better performance

#### **Asset Pipeline** 🆕
- **Level Dressing Saturation:** Automatic detection of too many props
- **Texture Optimization:** Category-specific compression (props, neon, hounds)
- **Performance Tracking:** Real-time statistics and warnings
- **Mobile-First:** ASTC compression and size limits for iOS/Android

### 🎮 **Platform Support**
- **Unity 2022.3 LTS+:** Full Unity implementation
- **iOS/Android:** Mobile-optimized builds
- **PC:** Desktop support with enhanced graphics
- **Web (Legacy):** HTML5 Canvas version

## Game Design Document

See `ANGRY_DOGS_GDD.md` for the complete game design document with detailed mechanics, obstacles, and development roadmap.

## Technical Details

### **Unity Implementation (Primary)**
- **Platform:** Unity 2022.3 LTS+ (C#)
- **Graphics:** 3D cyberpunk neon aesthetic with particle effects
- **Performance:** 60 FPS on mobile, 120+ FPS on desktop
- **Architecture:** SOLID principles with modular component system
- **Mobile Support:** iOS/Android with platform-specific optimizations

### **Web Implementation (Legacy)**
- **Platform:** HTML5 Canvas with JavaScript
- **Graphics:** 2D pixel art style with cyberpunk neon colors
- **Performance:** 60 FPS target, scalable for mobile devices
- **Browser Support:** Modern browsers with Canvas support

## Development Roadmap

1. **Phase 1:** Core prototype ✅
2. **Phase 2:** Advanced obstacle system and AI ✅
3. **Phase 3:** Audio and visual polish ✅
4. **Phase 4:** Mobile optimization and platform features ✅
5. **Phase 5:** Production deployment and cloud services 🚧

## Quick Start

### **Unity Setup**
1. Clone the repository
2. Open in Unity 2022.3 LTS or later
3. Open the main scene in `Assets/Scenes/`
4. Press Play to start the game

### **Web Setup (Legacy)**
1. Open `index.html` in a modern web browser
2. Start playing immediately!

## Architecture

### **Unity Scripts Structure**
```
Assets/Scripts/
├── Core/                    # Game state management and events
├── Player/                  # Riley and Nibble controllers
├── Enemies/                 # Hound AI and boss systems
├── Obstacles/               # Obstacle spawning and interactions
├── UI/                      # Mobile-optimized UI manager
├── SaveSystem/              # Cloud save and settings persistence
├── Systems/                 # Object pooling and game services
├── Data/                    # Save data and ScriptableObjects
├── Tools/                   # Asset importer and development tools
└── Tests/                   # Unit tests for all systems
```

### **Key Features**
- **Cloud Save System:** Cross-device progress with offline fallback
- **Boss Encounters:** Cyber-Chihuahua King with weak points and tantrums
- **Mobile Optimization:** 60 FPS performance with configurable settings
- **Asset Pipeline:** Automated optimization with saturation warnings
- **Unit Testing:** Comprehensive test coverage for all systems

## Contributing

This is a production-ready prototype for the "Angry Dogs" game concept. The code follows SOLID principles and is structured for easy extension:

- **New Obstacles:** Add to `Obstacles/` with repurposing mechanics
- **Boss Encounters:** Extend `Enemies/BossHound.cs` for new boss types
- **UI Features:** Enhance `UI/UIManager.cs` with mobile optimization
- **Save Data:** Extend `Data/PlayerSaveData.cs` for new persistent features

## License

This prototype is for demonstration purposes. All game concepts and designs are original creative works.

## Documentation

- **Game Design:** `ANGRY_DOGS_GDD.md` - Complete game design document
- **Architecture:** `Docs/AngryDogsArchitecture.md` - Technical architecture details
- **Development:** `DEVELOPMENT.md` - Development workflow and guidelines
