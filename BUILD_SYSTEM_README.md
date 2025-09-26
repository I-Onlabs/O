# Angry Dogs - Build System Documentation

## Overview

This document describes the automated build system for Angry Dogs, a humorous cyberpunk dog-chase game. The build system supports iOS, Android, and PC deployment with platform-specific optimizations and automated CI/CD integration.

**Riley**: "Time to document this build system! Can't have confusion when deploying to multiple platforms!"  
**Nibble**: "Bark! (Translation: Documentation for everyone!)"

## Features

### ✅ Completed Tasks

1. **Save/Load System Validation** - Enhanced with edge case handling, unit tests, and mobile optimization
2. **Boss Encounter Enhancement** - Added second phase "Overclocked Yap Mode" and Nibble interactions
3. **UI Responsiveness Testing** - Cross-resolution support with Quip Toggle functionality
4. **Obstacle Variety Expansion** - New "Neon Slobber Cannons" obstacle type with defense mechanics
5. **Build System Preparation** - Automated build pipeline for iOS/Android/PC deployment

## Build System Architecture

### Core Components

- **BuildManager.cs** - Main build orchestration and platform-specific configuration
- **BuildAutomation.cs** - CI/CD integration and command-line build support
- **Build Configuration** - JSON-based configuration for different platforms

### Platform Support

| Platform | Status | Optimizations |
|----------|--------|---------------|
| **PC (Windows)** | ✅ Complete | Full graphics, IL2CPP, code stripping |
| **PC (macOS)** | ✅ Complete | Metal graphics, universal binary |
| **PC (Linux)** | ✅ Complete | Vulkan/OpenGL, optimized for Steam |
| **Android** | ✅ Complete | ASTC textures, ARM64, mobile optimizations |
| **iOS** | ✅ Complete | Metal graphics, iOS 12.0+, universal device |

## Quick Start

### 1. Setup Build Configuration

```bash
# Create build configuration
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.CreateBuildConfiguration
```

### 2. Build for Specific Platform

```bash
# PC Build
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget pc -buildType release

# Android Build
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget android -buildType release

# iOS Build
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget ios -buildType release
```

### 3. Build All Platforms

```bash
# Build everything
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget all -buildType release
```

## Build Configuration

### JSON Configuration Structure

```json
{
  "buildName": "AngryDogs",
  "version": "1.0.0",
  "buildNumber": 1,
  "developmentBuild": false,
  "allowDebugging": false,
  "targetPlatform": "StandaloneWindows64",
  "outputPath": "Builds",
  "createFolderPerPlatform": true,
  "optimizeForMobile": true,
  "compressTextures": true,
  "stripUnusedCode": true,
  "enableIL2CPP": true,
  "useASTC": true,
  "useETC2": false,
  "textureCompressionQuality": 50,
  "optimizeMeshData": true,
  "autoIncrementBuildNumber": true,
  "updateReadme": true,
  "readmePath": "README.md"
}
```

### Platform-Specific Settings

#### PC (Windows/macOS/Linux)
- **Graphics APIs**: Direct3D11, OpenGL Core, Vulkan
- **Scripting Backend**: IL2CPP
- **Code Stripping**: Master (Release builds)
- **Optimization**: Full desktop optimizations

#### Android
- **Architecture**: ARM64
- **Graphics APIs**: OpenGL ES 3.0, Vulkan
- **Texture Compression**: ASTC, ETC2
- **Optimization**: Mobile-optimized, reduced draw calls

#### iOS
- **Graphics API**: Metal
- **Target iOS Version**: 12.0+
- **Device Support**: iPhone and iPad
- **Texture Compression**: ASTC
- **Optimization**: iOS-specific optimizations

## Mobile Optimization Features

### Performance Optimizations
- **Texture Compression**: ASTC for iOS, ETC2 for Android
- **Mesh Optimization**: Reduced polygon count for mobile
- **Code Stripping**: Removes unused code in release builds
- **IL2CPP**: Improved performance and security

### Battery Life Optimizations
- **Reduced Update Frequency**: UI updates throttled on mobile
- **Cloud Sync Queuing**: Batched cloud saves to reduce network usage
- **Particle System Optimization**: Reduced particle counts on mobile

### Memory Management
- **Object Pooling**: Reused objects to reduce GC pressure
- **Texture Size Limits**: Maximum 512px textures on mobile
- **Asset Streaming**: Load assets on-demand

## Save System Enhancements

### Edge Case Handling
- **Corrupted Save Recovery**: Automatic fallback to default data
- **JSON Integrity Validation**: Prevents corrupted saves
- **Atomic File Operations**: Prevents partial writes
- **Cloud Sync Conflict Resolution**: Smart merging of local/cloud data

### Mobile Optimizations
- **Offline Mode**: Queue saves when cloud unavailable
- **Batched Cloud Sync**: Reduce battery drain
- **Retry Logic**: Automatic retry with exponential backoff

### Unit Tests
- **Comprehensive Coverage**: All save/load scenarios tested
- **Edge Case Testing**: Corrupted data, network failures
- **Cross-Platform Testing**: iOS, Android, PC compatibility

## Boss Encounter Enhancements

### Second Phase: "Overclocked Yap Mode"
- **Trigger**: 15% health remaining
- **Duration**: 15 seconds
- **Effects**: 3x damage, 2x speed, 0.5x attack interval
- **Special Attacks**: Area-of-effect yap attacks

### Nibble Interactions
- **Decoy Bone Fetching**: Distract boss for 8 seconds
- **Cooldown**: 10 seconds between uses
- **Strategic Timing**: Use during dangerous phases

### Performance Optimizations
- **60 FPS Target**: Optimized for mid-range mobile devices
- **Efficient Collision Detection**: Reduced physics calculations
- **Smart AI**: Reduced update frequency when not in combat

## UI Responsiveness Features

### Cross-Resolution Support
- **Tested Resolutions**: iPhone SE to 4K desktop
- **Aspect Ratio Handling**: Portrait, landscape, ultra-wide
- **Canvas Scaling**: Automatic UI scaling for all devices

### Quip System
- **Toggle Control**: Enable/disable Riley and Nibble dialogue
- **Performance Option**: Reduce UI updates on low-end devices
- **Humor Integration**: Cyberpunk-themed quips and barks

### Mobile Optimizations
- **Touch Target Sizing**: Minimum 60px touch targets
- **Neon Effects Toggle**: Disable for performance
- **Device Detection**: Automatic optimization per device type

## Obstacle System Expansion

### Neon Slobber Cannons
- **Obstacle Type**: New obstacle that spawns goo traps
- **Repurposing**: Goo traps become defensive shields
- **Nibble Protection**: Shields protect Nibble from hounds
- **Visual Effects**: Neon goo with particle systems

### Defense Mechanics
- **Goo Trap Effects**: Slow and stun hounds
- **Shield Duration**: 8 seconds of protection
- **Area of Effect**: 2-unit radius around Nibble
- **Strategic Use**: Timing-based defensive gameplay

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build Angry Dogs
on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [windows-latest, macos-latest, ubuntu-latest]
        platform: [pc, android, ios]
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Unity
      uses: game-ci/unity-setup@v2
      with:
        unity-version: '2022.3.0f1'
    
    - name: Build
      run: |
        Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget ${{ matrix.platform }} -buildType release
    
    - name: Upload Build
      uses: actions/upload-artifact@v3
      with:
        name: build-${{ matrix.platform }}-${{ matrix.os }}
        path: Builds/
```

### Jenkins Pipeline Example

```groovy
pipeline {
    agent any
    
    stages {
        stage('Build PC') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget pc -buildType release'
            }
        }
        
        stage('Build Android') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget android -buildType release'
            }
        }
        
        stage('Build iOS') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget ios -buildType release'
            }
        }
    }
    
    post {
        always {
            archiveArtifacts artifacts: 'Builds/**/*', fingerprint: true
        }
    }
}
```

## Troubleshooting

### Common Issues

#### Build Failures
- **Check Unity Version**: Ensure 2022.3.0f1 or later
- **Verify Scenes**: All scenes must be added to Build Settings
- **Platform Modules**: Install required platform modules
- **Dependencies**: Ensure all required packages are installed
- **DeployManager Checks**: Run automated deployment validation

#### Mobile Performance Issues
- **Texture Compression**: Enable ASTC/ETC2 compression
- **Code Stripping**: Enable in release builds
- **Particle Systems**: Reduce particle counts on mobile
- **UI Updates**: Enable mobile optimization mode
- **Battery Usage**: Monitor with enhanced analytics

#### Save System Issues
- **Corrupted Saves**: System automatically recovers with defaults
- **Cloud Sync Failures**: Check network connectivity and API keys
- **Cross-Platform Saves**: Ensure consistent data format
- **Stress Testing**: Run comprehensive stress tests

#### Localization Issues
- **Language Switching**: Check LocalizationManager initialization
- **Quip Display**: Verify quip toggle functionality
- **Neon Text**: Ensure neon-themed text remains readable
- **RTL Support**: Test right-to-left language support

#### Daily Challenge Issues
- **Challenge Generation**: Check DailyChallengeManager initialization
- **KibbleCoin Rewards**: Verify reward distribution
- **Progress Tracking**: Ensure challenge progress is saved
- **Reroll Functionality**: Test challenge rerolling

### Debug Commands

```bash
# Validate build configuration
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.ValidateBuildConfiguration

# Run deployment checks
Unity -batchmode -quit -executeMethod AngryDogs.Tools.DeployManager.RunDeploymentChecksFromMenu

# Test UI responsiveness
Unity -batchmode -quit -executeMethod AngryDogs.UI.UIManager.TestUIResponsiveness

# Run save system tests
Unity -batchmode -quit -executeMethod UnityEditor.TestRunner.TestRunnerApi.RunTests

# Run stress tests
Unity -batchmode -quit -executeMethod AngryDogs.Tests.StressTests.RunAllStressTests

# Test localization
Unity -batchmode -quit -executeMethod AngryDogs.UI.LocalizationManager.TestLocalization

# Test daily challenges
Unity -batchmode -quit -executeMethod AngryDogs.Gameplay.DailyChallengeManager.TestDailyChallenges
```

### Launch Checklist

#### Pre-Launch Validation
- [ ] **Deployment Checks**: All automated checks pass
- [ ] **Version Validation**: Version numbers are correct
- [ ] **Asset Integrity**: All required assets present
- [ ] **Performance Targets**: 60 FPS on target devices
- [ ] **Memory Usage**: Within acceptable limits
- [ ] **Mobile Optimizations**: Enabled for mobile builds
- [ ] **Texture Compression**: Properly configured
- [ ] **Save System**: Stress tested and validated
- [ ] **Localization**: All languages working correctly
- [ ] **Daily Challenges**: Functioning properly
- [ ] **Analytics**: Tracking enabled and working
- [ ] **Error Reporting**: Bug reporting functional

#### Platform-Specific Checks

##### PC (Windows/macOS/Linux)
- [ ] **Graphics APIs**: Direct3D11, OpenGL Core, Vulkan
- [ ] **Scripting Backend**: IL2CPP enabled
- [ ] **Code Stripping**: Master level for release
- [ ] **Resolution Support**: 1080p to 4K
- [ ] **Input Support**: Keyboard and mouse
- [ ] **Fullscreen Support**: Proper fullscreen handling

##### Android
- [ ] **Architecture**: ARM64 only
- [ ] **Graphics APIs**: OpenGL ES 3.0, Vulkan
- [ ] **Texture Compression**: ASTC, ETC2
- [ ] **Min SDK**: API 21 (Android 5.0)
- [ ] **Target SDK**: API 33 (Android 13)
- [ ] **Permissions**: Properly configured
- [ ] **Battery Optimization**: Minimized battery drain

##### iOS
- [ ] **Graphics API**: Metal only
- [ ] **Target iOS**: 12.0+
- [ ] **Device Support**: iPhone and iPad
- [ ] **Texture Compression**: ASTC
- [ ] **App Store**: Compliance with guidelines
- [ ] **Privacy**: Proper privacy descriptions

#### Post-Launch Monitoring
- [ ] **Analytics**: Monitor player retention
- [ ] **Error Reporting**: Track crashes and bugs
- [ ] **Performance**: Monitor FPS and memory usage
- [ ] **Battery Usage**: Track mobile battery consumption
- [ ] **Localization**: Monitor language usage
- [ ] **Daily Challenges**: Track completion rates
- [ ] **Save System**: Monitor cloud sync success
- [ ] **UI Interactions**: Track quip toggle usage

### CI/CD Pipeline Integration

#### GitHub Actions Enhanced Workflow
```yaml
name: Enhanced Build and Deploy
on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-validate:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [windows-latest, macos-latest, ubuntu-latest]
        platform: [pc, android, ios]
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Unity
      uses: game-ci/unity-setup@v2
      with:
        unity-version: '2022.3.0f1'
    
    - name: Run Stress Tests
      run: |
        Unity -batchmode -quit -executeMethod AngryDogs.Tests.StressTests.RunAllStressTests
    
    - name: Run Deployment Checks
      run: |
        Unity -batchmode -quit -executeMethod AngryDogs.Tools.DeployManager.RunDeploymentChecksFromMenu
    
    - name: Build
      run: |
        Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget ${{ matrix.platform }} -buildType release
    
    - name: Validate Build
      run: |
        # Run post-build validation
        Unity -batchmode -quit -executeMethod AngryDogs.Tools.DeployManager.ValidateBuildOutput
    
    - name: Upload Build
      uses: actions/upload-artifact@v3
      with:
        name: build-${{ matrix.platform }}-${{ matrix.os }}
        path: Builds/
```

#### Jenkins Enhanced Pipeline
```groovy
pipeline {
    agent any
    
    stages {
        stage('Stress Testing') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tests.StressTests.RunAllStressTests'
            }
        }
        
        stage('Deployment Validation') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tools.DeployManager.RunDeploymentChecksFromMenu'
            }
        }
        
        stage('Build PC') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget pc -buildType release'
            }
        }
        
        stage('Build Android') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget android -buildType release'
            }
        }
        
        stage('Build iOS') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget ios -buildType release'
            }
        }
        
        stage('Post-Build Validation') {
            steps {
                sh 'Unity -batchmode -quit -executeMethod AngryDogs.Tools.DeployManager.ValidateBuildOutput'
            }
        }
    }
    
    post {
        always {
            archiveArtifacts artifacts: 'Builds/**/*', fingerprint: true
            publishTestResults testResultsPattern: 'TestResults.xml'
        }
    }
}
```

## Performance Metrics

### Target Performance
- **PC**: 60+ FPS at 1080p, 30+ FPS at 4K
- **Mobile**: 60 FPS on mid-range devices (iPhone 8, Galaxy S9)
- **Memory Usage**: <2GB on PC, <1GB on mobile
- **Build Size**: <500MB for mobile, <1GB for PC

### Optimization Checklist
- [ ] Texture compression enabled
- [ ] Code stripping enabled (release builds)
- [ ] IL2CPP enabled
- [ ] Mobile optimizations enabled
- [ ] Particle systems optimized
- [ ] UI update frequency throttled
- [ ] Object pooling implemented
- [ ] Asset streaming configured

## Future Enhancements

### Planned Features
- **Cloud Build Integration**: Direct Unity Cloud Build support
- **Automated Testing**: Unit tests in CI/CD pipeline
- **Performance Profiling**: Automated performance regression testing
- **Asset Bundles**: Dynamic content loading
- **Localization**: Multi-language support

### Scalability Improvements
- **Modular Build System**: Plugin-based architecture
- **Custom Build Steps**: User-defined build processes
- **Build Caching**: Incremental build support
- **Parallel Builds**: Multi-platform simultaneous builds

## Support

For issues or questions about the build system:

- **Riley**: "Check the logs first! They usually tell you what's wrong!"
- **Nibble**: "Bark! (Translation: Ask for help if you need it!)"

### Resources
- Unity Build System Documentation
- Platform-Specific Optimization Guides
- Performance Profiling Tools
- CI/CD Best Practices

---

**Riley**: "That's the complete build system documentation! Time to deploy this cyberpunk dog-chase game to the world!"  
**Nibble**: "Bark! (Translation: Let's go build some games!)"