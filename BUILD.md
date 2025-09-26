# Build System Documentation

## Overview

The Angry Dogs game now uses Webpack 5 as its build system, providing:
- **Development server** with hot module replacement
- **Production builds** with optimization and minification
- **Asset management** for images, audio, and fonts
- **Code splitting** and caching strategies
- **Source maps** for debugging

## Quick Start

### Development
```bash
npm start          # Start dev server (http://localhost:8080)
npm run dev        # Same as start
```

### Production
```bash
npm run build      # Build optimized production files
npm run build:dev  # Build development files
npm run clean      # Clean build directory
```

## Build Configurations

### Development (`webpack.dev.cjs`)
- **Hot Module Replacement**: Changes reflect instantly
- **Source Maps**: Full source mapping for debugging
- **Error Overlay**: Visual error reporting in browser
- **Fast Builds**: Optimized for development speed
- **Asset Handling**: Direct file copying for assets

### Production (`webpack.prod.cjs`)
- **Minification**: JavaScript and CSS minified
- **Code Splitting**: Automatic vendor chunk separation
- **Tree Shaking**: Dead code elimination
- **Asset Optimization**: Hashed filenames for caching
- **Console Removal**: Debug logs removed
- **Source Maps**: Separate source map files

## Asset Management

### Supported File Types
- **Images**: `.png`, `.jpg`, `.jpeg`, `.gif`, `.svg`, `.ico`
- **Audio**: `.mp3`, `.wav`, `.ogg`, `.m4a`
- **Fonts**: `.woff`, `.woff2`, `.eot`, `.ttf`, `.otf`
- **Styles**: `.css`

### Asset Organization
```
src/assets/
├── images/     # Sprites, textures, UI elements
├── audio/      # Sound effects, music
├── fonts/      # Custom fonts
└── sprites/    # Sprite sheets, animations
```

### Using Assets in Code
```javascript
// Import images
import playerSprite from '@/assets/images/player.png';
import backgroundMusic from '@/assets/audio/background.mp3';

// Use in code
const img = new Image();
img.src = playerSprite;
```

## Path Aliases

The build system provides convenient path aliases:
- `@/` → `src/`
- `@core/` → `src/core/`
- `@entities/` → `src/entities/`
- `@systems/` → `src/systems/`
- `@assets/` → `src/assets/`

## Build Output

### Development Build
```
dist/
├── index.html
├── main.js          # Unminified, with source maps
└── assets/          # Copied assets
```

### Production Build
```
dist/
├── index.html
├── main.[hash].js   # Minified with content hash
├── main.[hash].js.map  # Source map
└── assets/          # Optimized assets with hashes
```

## Performance Optimizations

### Code Splitting
- **Vendor Chunk**: Third-party libraries separated
- **Dynamic Imports**: Lazy loading for large modules
- **Bundle Analysis**: Use `webpack-bundle-analyzer` for analysis

### Caching Strategy
- **Content Hashing**: File names include content hash
- **Long-term Caching**: Assets cached by browsers
- **Cache Busting**: Changes automatically invalidate cache

### Bundle Size
- **Development**: ~278KB (unminified)
- **Production**: ~41KB (minified + gzipped)
- **Compression**: ~85% size reduction

## Development Workflow

### 1. Start Development
```bash
npm start
```
- Opens browser automatically
- Hot reload on file changes
- Error overlay for debugging

### 2. Make Changes
- Edit files in `src/`
- Changes appear instantly
- Console shows compilation status

### 3. Test Production Build
```bash
npm run build
npm run serve  # Test production build
```

### 4. Deploy
- Upload `dist/` directory to web server
- All assets properly linked
- Optimized for production

## Troubleshooting

### Common Issues

**Build Fails**
- Check for syntax errors in source files
- Verify all imports are correct
- Clear `node_modules` and reinstall

**Assets Not Loading**
- Ensure assets are in correct directories
- Check import paths use aliases
- Verify file extensions match

**Hot Reload Not Working**
- Check browser console for errors
- Restart development server
- Clear browser cache

**Large Bundle Size**
- Use `webpack-bundle-analyzer` to analyze
- Check for unused imports
- Consider code splitting

### Debug Commands
```bash
# Verbose build output
npm run build -- --verbose

# Analyze bundle
npx webpack-bundle-analyzer dist/main.*.js

# Check for circular dependencies
npx madge --circular src/
```

## Advanced Configuration

### Custom Webpack Config
Create `webpack.custom.cjs` for project-specific needs:
```javascript
const baseConfig = require('./webpack.dev.cjs');

module.exports = {
  ...baseConfig,
  // Custom overrides
};
```

### Environment Variables
Use `.env` file for environment-specific settings:
```bash
NODE_ENV=development
PORT=8080
DEBUG_MODE=true
```

### Adding Loaders
Install and configure additional loaders:
```bash
npm install --save-dev sass-loader sass
```

## Integration with CI/CD

### GitHub Actions Example
```yaml
name: Build and Deploy
on: [push]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-node@v2
        with:
          node-version: '18'
      - run: npm ci
      - run: npm run build
      - uses: actions/deploy-pages@v1
        with:
          artifact_name: dist
```

## Performance Monitoring

### Bundle Analysis
```bash
# Install analyzer
npm install --save-dev webpack-bundle-analyzer

# Analyze bundle
npx webpack-bundle-analyzer dist/main.*.js
```

### Lighthouse Audit
- Run Lighthouse on production build
- Monitor Core Web Vitals
- Optimize based on recommendations

## Future Enhancements

### Planned Features
- **PWA Support**: Service worker and manifest
- **TypeScript**: Type checking and better IDE support
- **ESLint Integration**: Code quality checks
- **Testing Integration**: Jest/Vitest setup
- **Docker Support**: Containerized development
- **CDN Integration**: Asset optimization and delivery