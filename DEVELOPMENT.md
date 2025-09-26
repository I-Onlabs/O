# Angry Dogs - Development Guide

## Project Structure

The game has been refactored into a modular architecture for better maintainability and extensibility.

```
src/
├── main.js                 # Entry point
├── Game.js                 # Main game orchestrator
├── core/
│   └── GameObject.js       # Base class for all game objects
├── entities/
│   ├── Player.js          # Riley (main character)
│   ├── Nibble.js          # Loyal companion dog
│   ├── AngryDog.js        # Hostile cybernetic hounds
│   └── Projectile.js      # Player shots
└── systems/
    ├── ParticleSystem.js  # Visual effects
    ├── CollisionSystem.js # Collision detection
    └── InputSystem.js     # Input handling
```

## Key Improvements

### 1. Modular Architecture
- **Separation of Concerns**: Each class has a single responsibility
- **Inheritance**: All entities extend GameObject base class
- **Composition**: Systems are composed into the main Game class

### 2. Enhanced Features
- **Better Input System**: Supports keyboard, mouse, and touch
- **Advanced Collision Detection**: Configurable collision callbacks
- **Particle System**: Rich visual effects
- **Debug Information**: FPS counter, object counts, etc.
- **Pause Functionality**: Press P to pause/resume

### 3. Performance Optimizations
- **Object Pooling Ready**: Structure supports easy object pooling
- **Efficient Rendering**: Only renders active objects
- **Delta Time**: Frame-rate independent updates

## Development Setup

### Option 1: Python HTTP Server (Simple)
```bash
python3 -m http.server 8000
```

### Option 2: Custom Development Server (Recommended)
```bash
node dev-server.js
```

### Option 3: Using npm scripts
```bash
npm run dev
```

Then open http://localhost:8000 in your browser.

## Development Workflow

### Adding New Entities
1. Create a new class extending `GameObject`
2. Implement `update()` and `render()` methods
3. Add to the main Game class
4. Register collision handlers if needed

### Adding New Systems
1. Create a new system class
2. Initialize in the Game constructor
3. Update and render in the main game loop

### Debugging
- Open browser DevTools (F12)
- Game instance is available as `window.game`
- Check console for error messages
- Use debug overlay (FPS, object counts)

## Code Style Guidelines

### Classes
- Use PascalCase for class names
- Use descriptive method names
- Add JSDoc comments for public methods

### Variables
- Use camelCase for variables and methods
- Use descriptive names
- Prefer const over let when possible

### File Organization
- One class per file
- Group related classes in directories
- Use index.js files for clean imports

## Testing

Currently no automated tests are implemented. Future plans include:
- Unit tests for individual classes
- Integration tests for game systems
- Performance benchmarks

## Performance Considerations

### Current Optimizations
- Only update/render active objects
- Efficient collision detection
- Particle system with limits

### Planned Optimizations
- Object pooling for frequently created objects
- Spatial partitioning for collision detection
- WebGL rendering for better performance

## Mobile Development

### Touch Controls
- Swipe gestures for movement
- Tap to shoot
- Gesture detection in InputSystem

### Responsive Design
- Canvas scaling for different screen sizes
- Touch-friendly UI elements
- Performance optimization for mobile

## Build System

Currently using ES6 modules directly in the browser. Future plans:
- Webpack for bundling
- Minification and optimization
- Asset preprocessing
- Hot module replacement for development

## Deployment

### Current
- Static file hosting
- No build step required
- Works in modern browsers

### Planned
- Automated build pipeline
- CDN deployment
- Progressive Web App features