# Development Quick Start

## Available Scripts

- `npm start` - Start development server with hot reload
- `npm run dev` - Same as start
- `npm run build` - Build for production
- `npm run build:dev` - Build for development
- `npm run clean` - Clean build directory
- `npm run serve` - Serve with Python (fallback)

## Development Server

The development server runs on http://localhost:8080 with:
- Hot module replacement
- Source maps for debugging
- Error overlay
- Auto-reload on file changes

## Project Structure

```
src/
├── main.js              # Entry point
├── Game.js              # Main game class
├── core/                # Base classes
├── entities/            # Game entities
├── systems/             # Game systems
└── assets/              # Game assets
    ├── images/          # Sprites, textures
    ├── audio/           # Sound effects, music
    ├── fonts/           # Custom fonts
    └── sprites/         # Sprite sheets
```

## Adding Assets

1. Place images in `src/assets/images/`
2. Place audio in `src/assets/audio/`
3. Import in your code: `import sprite from '@/assets/images/sprite.png'`

## Debugging

- Open browser DevTools (F12)
- Game instance available as `window.game`
- Check console for errors and debug info
- Use debug overlay in game (FPS, object counts)

## Building for Production

`npm run build` creates optimized files in `dist/` directory.

## Troubleshooting

- Clear node_modules and reinstall if issues persist
- Check browser console for errors
- Ensure all imports use correct paths
- Verify file extensions in imports
