#!/usr/bin/env node

/**
 * Development environment setup script
 * Sets up the project for development with all necessary configurations
 */

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

console.log('🚀 Setting up Angry Dogs development environment...\n');

// Check if node_modules exists
if (!fs.existsSync('node_modules')) {
  console.log('📦 Installing dependencies...');
  try {
    execSync('npm install', { stdio: 'inherit' });
    console.log('✅ Dependencies installed successfully\n');
  } catch (error) {
    console.error('❌ Failed to install dependencies:', error.message);
    process.exit(1);
  }
} else {
  console.log('✅ Dependencies already installed\n');
}

// Create assets directory structure
const assetsDir = path.join(__dirname, 'src', 'assets');
const assetSubdirs = ['images', 'audio', 'fonts', 'sprites'];

console.log('📁 Creating asset directories...');
assetSubdirs.forEach(subdir => {
  const dirPath = path.join(assetsDir, subdir);
  if (!fs.existsSync(dirPath)) {
    fs.mkdirSync(dirPath, { recursive: true });
    console.log(`  Created: ${dirPath}`);
  }
});
console.log('✅ Asset directories ready\n');

// Create .env file if it doesn't exist
const envFile = path.join(__dirname, '.env');
if (!fs.existsSync(envFile)) {
  console.log('⚙️  Creating environment file...');
  const envContent = `# Development Environment Variables
NODE_ENV=development
PORT=8080
HOT_RELOAD=true
DEBUG_MODE=true
`;
  fs.writeFileSync(envFile, envContent);
  console.log('✅ Environment file created\n');
}

// Create development README
const devReadme = path.join(__dirname, 'DEV-README.md');
if (!fs.existsSync(devReadme)) {
  console.log('📝 Creating development README...');
  const devReadmeContent = `# Development Quick Start

## Available Scripts

- \`npm start\` - Start development server with hot reload
- \`npm run dev\` - Same as start
- \`npm run build\` - Build for production
- \`npm run build:dev\` - Build for development
- \`npm run clean\` - Clean build directory
- \`npm run serve\` - Serve with Python (fallback)

## Development Server

The development server runs on http://localhost:8080 with:
- Hot module replacement
- Source maps for debugging
- Error overlay
- Auto-reload on file changes

## Project Structure

\`\`\`
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
\`\`\`

## Adding Assets

1. Place images in \`src/assets/images/\`
2. Place audio in \`src/assets/audio/\`
3. Import in your code: \`import sprite from '@/assets/images/sprite.png'\`

## Debugging

- Open browser DevTools (F12)
- Game instance available as \`window.game\`
- Check console for errors and debug info
- Use debug overlay in game (FPS, object counts)

## Building for Production

\`npm run build\` creates optimized files in \`dist/\` directory.

## Troubleshooting

- Clear node_modules and reinstall if issues persist
- Check browser console for errors
- Ensure all imports use correct paths
- Verify file extensions in imports
`;
  fs.writeFileSync(devReadme, devReadmeContent);
  console.log('✅ Development README created\n');
}

console.log('🎉 Development environment setup complete!');
console.log('\nNext steps:');
console.log('1. Run \`npm start\` to start the development server');
console.log('2. Open http://localhost:8080 in your browser');
console.log('3. Start coding! 🎮\n');

console.log('Available commands:');
console.log('  npm start     - Start dev server');
console.log('  npm run build - Build for production');
console.log('  npm run clean - Clean build files');
console.log('  npm run serve - Fallback Python server\n');