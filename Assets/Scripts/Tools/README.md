# Angry Dogs - Tools Documentation

## Overview

This directory contains all the development and optimization tools for the Angry Dogs game. These tools provide comprehensive support for build automation, performance monitoring, asset optimization, and quality management.

**Riley**: "Time to document all these tools! Can't have confusion when optimizing the game!"  
**Nibble**: "Bark! (Translation: Documentation for all the tools!)"

## Tools Overview

### 🏗️ **Build System**
- **BuildManager.cs** - Main build orchestration and platform-specific configuration
- **BuildAutomation.cs** - CI/CD integration and command-line build support
- **AssetOptimizer.cs** - Asset optimization for different platforms

### 📊 **Performance & Quality**
- **PerformanceProfiler.cs** - Real-time performance monitoring and metrics collection
- **QualitySettingsManager.cs** - Dynamic quality adjustment based on device performance
- **PlatformDetector.cs** - Device capability detection and optimization
- **GameOptimizer.cs** - Main optimization coordinator

### 📈 **Analytics & Monitoring**
- **GameAnalytics.cs** - Player behavior and performance tracking
- **ErrorReporter.cs** - Crash and error reporting system

## Quick Start

### 1. Build System Setup

```bash
# Create build configuration
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.CreateBuildConfiguration

# Build for PC
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget pc -buildType release

# Build for Android
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget android -buildType release

# Build for iOS
Unity -batchmode -quit -executeMethod AngryDogs.Tools.BuildAutomation.BuildFromCommandLine -buildTarget ios -buildType release
```

### 2. Performance Monitoring

```csharp
// Get performance profiler
var profiler = FindObjectOfType<PerformanceProfiler>();

// Start profiling
profiler.StartProfiling();

// Get current metrics
var metrics = profiler.GetCurrentMetrics();
Debug.Log($"FPS: {metrics.currentFPS}, Memory: {metrics.usedMemory / 1024f / 1024f}MB");

// Export performance data
profiler.ExportToCSV("performance_data.csv");
```

### 3. Quality Management

```csharp
// Get quality manager
var qualityManager = FindObjectOfType<QualitySettingsManager>();

// Set quality level
qualityManager.SetQualityLevel(2); // Medium quality

// Enable auto-adjustment
qualityManager.EnableAutoAdjustment();

// Get performance stats
var stats = qualityManager.GetPerformanceStats();
Debug.Log(stats);
```

### 4. Platform Detection

```csharp
// Get platform detector
var detector = FindObjectOfType<PlatformDetector>();

// Detect platform
detector.DetectPlatform();

// Get device info
var deviceInfo = detector.GetDeviceInfo();
Debug.Log($"Device: {deviceInfo.deviceName}, Performance: {deviceInfo.performanceTier}");

// Check if mobile
if (detector.IsMobileDevice())
{
    Debug.Log("Running on mobile device");
}
```

## Detailed Tool Documentation

### BuildManager.cs

**Purpose**: Main build orchestration and platform-specific configuration.

**Key Features**:
- Platform-specific build configurations
- Automated build pipeline
- Asset optimization integration
- Version management
- Build validation

**Usage**:
```csharp
var buildManager = FindObjectOfType<BuildManager>();

// Build for PC
buildManager.BuildPC();

// Build for Android
buildManager.BuildAndroid();

// Build for iOS
buildManager.BuildiOS();

// Build all platforms
buildManager.BuildAllPlatforms();
```

**Configuration**:
- PC: Full graphics, IL2CPP, code stripping
- Android: ASTC textures, ARM64, mobile optimizations
- iOS: Metal graphics, iOS 12.0+, universal device

### PerformanceProfiler.cs

**Purpose**: Real-time performance monitoring and metrics collection.

**Key Features**:
- FPS monitoring (current, average, min, max)
- Memory usage tracking
- Rendering metrics (draw calls, triangles, vertices)
- Physics metrics (rigidbodies, colliders)
- Audio metrics
- Mobile-specific metrics (battery, temperature)

**Usage**:
```csharp
var profiler = FindObjectOfType<PerformanceProfiler>();

// Start profiling
profiler.StartProfiling();

// Get current metrics
var metrics = profiler.GetCurrentMetrics();
Debug.Log($"FPS: {metrics.currentFPS}");

// Export data
profiler.ExportToCSV("performance.csv");
```

**Performance Thresholds**:
- Target FPS: 60
- Warning FPS: 45
- Critical FPS: 30
- Max Memory: 1024MB
- Max Draw Calls: 100

### QualitySettingsManager.cs

**Purpose**: Dynamic quality adjustment based on device performance.

**Key Features**:
- 5 quality levels (Ultra, High, Medium, Low, Mobile)
- Automatic quality adjustment
- Performance-based optimization
- Device-specific settings
- Real-time quality switching

**Usage**:
```csharp
var qualityManager = FindObjectOfType<QualitySettingsManager>();

// Set quality level
qualityManager.SetQualityLevel(2); // Medium

// Enable auto-adjustment
qualityManager.EnableAutoAdjustment();

// Force quality level
qualityManager.ForceQualityLevel(3); // Low
```

**Quality Levels**:
- **Ultra**: 60+ FPS, full effects, 4K textures
- **High**: 60+ FPS, high effects, 2K textures
- **Medium**: 60 FPS, balanced effects, 1K textures
- **Low**: 30+ FPS, minimal effects, 512px textures
- **Mobile**: 30 FPS, mobile optimizations, 256px textures

### PlatformDetector.cs

**Purpose**: Device capability detection and optimization.

**Key Features**:
- Device information collection
- Performance tier detection
- Graphics capability detection
- Mobile device type detection
- Automatic optimization

**Usage**:
```csharp
var detector = FindObjectOfType<PlatformDetector>();

// Detect platform
detector.DetectPlatform();

// Get device info
var deviceInfo = detector.GetDeviceInfo();
Debug.Log($"Device: {deviceInfo.deviceName}");

// Check performance tier
var tier = detector.GetPerformanceTier();
Debug.Log($"Performance: {tier}");
```

**Performance Tiers**:
- **Low**: Basic graphics, 30 FPS target
- **Medium**: Balanced graphics, 60 FPS target
- **High**: High graphics, 60 FPS target
- **Ultra**: Maximum graphics, unlimited FPS

### GameOptimizer.cs

**Purpose**: Main optimization coordinator.

**Key Features**:
- Optimization profile management
- Dynamic performance optimization
- Battery optimization
- Memory optimization
- Rendering optimization

**Usage**:
```csharp
var optimizer = FindObjectOfType<GameOptimizer>();

// Set optimization profile
optimizer.SetProfile(2); // Medium profile

// Enable auto-optimization
optimizer.EnableAutoOptimization();

// Force optimization check
optimizer.ForceOptimizationCheck();
```

**Optimization Profiles**:
- **Ultra**: Maximum quality, no optimizations
- **High**: High quality, light optimizations
- **Medium**: Balanced quality and performance
- **Mobile**: Mobile-optimized, battery-friendly

### GameAnalytics.cs

**Purpose**: Player behavior and performance tracking.

**Key Features**:
- Session tracking
- Level progress tracking
- Death tracking
- Power-up usage tracking
- Boss encounter tracking
- Performance metrics tracking

**Usage**:
```csharp
var analytics = FindObjectOfType<GameAnalytics>();

// Track level progress
analytics.TrackLevelProgress(5, 0.75f, 1500);

// Track death
analytics.TrackDeath("Boss", Vector3.zero, 1200);

// Track power-up usage
analytics.TrackPowerUpUsed("Shield", Vector3.zero, 1000);
```

### ErrorReporter.cs

**Purpose**: Crash and error reporting system.

**Key Features**:
- Error collection and filtering
- Crash reporting
- Remote error reporting
- Error log file generation
- Duplicate error filtering

**Usage**:
```csharp
var errorReporter = FindObjectOfType<ErrorReporter>();

// Enable error reporting
errorReporter.EnableErrorReporting();

// Export error logs
errorReporter.ExportErrorLogs("error_log.txt");
```

## Integration Guide

### 1. Add to Scene

```csharp
// Add all tools to a GameObject
var toolsObject = new GameObject("GameTools");
toolsObject.AddComponent<BuildManager>();
toolsObject.AddComponent<PerformanceProfiler>();
toolsObject.AddComponent<QualitySettingsManager>();
toolsObject.AddComponent<PlatformDetector>();
toolsObject.AddComponent<GameOptimizer>();
toolsObject.AddComponent<GameAnalytics>();
toolsObject.AddComponent<ErrorReporter>();
```

### 2. Initialize Systems

```csharp
// Initialize in order
var platformDetector = FindObjectOfType<PlatformDetector>();
platformDetector.DetectPlatform();

var gameOptimizer = FindObjectOfType<GameOptimizer>();
gameOptimizer.EnableAutoOptimization();

var performanceProfiler = FindObjectOfType<PerformanceProfiler>();
performanceProfiler.StartProfiling();
```

### 3. Configure for Platform

```csharp
// Configure based on platform
if (Application.isMobilePlatform)
{
    var qualityManager = FindObjectOfType<QualitySettingsManager>();
    qualityManager.SetQualityLevel(4); // Mobile quality
}
else
{
    var qualityManager = FindObjectOfType<QualitySettingsManager>();
    qualityManager.SetQualityLevel(2); // Medium quality
}
```

## Performance Optimization

### Mobile Optimization
- **Target FPS**: 30-60 FPS
- **Memory Usage**: <512MB
- **Draw Calls**: <50
- **Texture Size**: <256px
- **Particle Count**: <50

### Desktop Optimization
- **Target FPS**: 60+ FPS
- **Memory Usage**: <2GB
- **Draw Calls**: <100
- **Texture Size**: <2K
- **Particle Count**: <500

### Battery Optimization
- **Frame Rate**: 30 FPS
- **Update Frequency**: Reduced
- **Effects**: Disabled
- **Shadows**: Disabled
- **Post-Processing**: Disabled

## Troubleshooting

### Common Issues

#### Build Failures
- Check Unity version compatibility
- Verify platform modules are installed
- Ensure all scenes are in Build Settings
- Check for missing dependencies

#### Performance Issues
- Enable performance profiling
- Check quality settings
- Verify optimization profiles
- Monitor memory usage

#### Mobile Issues
- Test on actual devices
- Check texture compression
- Verify touch input
- Monitor battery usage

### Debug Commands

```csharp
// Force optimization check
FindObjectOfType<GameOptimizer>().ForceOptimizationCheck();

// Export performance data
FindObjectOfType<PerformanceProfiler>().ExportToCSV("debug.csv");

// Get device info
var info = FindObjectOfType<PlatformDetector>().GetDeviceInfo();
Debug.Log(info);

// Get optimization stats
var stats = FindObjectOfType<GameOptimizer>().GetOptimizationStats();
Debug.Log(stats);
```

## Best Practices

### 1. Performance Monitoring
- Always enable performance profiling in development
- Monitor FPS, memory, and draw calls
- Set appropriate performance thresholds
- Use performance data to guide optimization

### 2. Quality Management
- Start with medium quality settings
- Enable auto-adjustment for dynamic optimization
- Test on target devices
- Provide manual quality controls

### 3. Platform Detection
- Detect platform early in initialization
- Use detection results to configure systems
- Test on multiple devices
- Handle edge cases gracefully

### 4. Error Reporting
- Enable error reporting in production
- Filter duplicate errors
- Monitor error rates
- Use error data for bug fixes

### 5. Analytics
- Track key player metrics
- Monitor performance data
- Use analytics for game balance
- Respect player privacy

## Future Enhancements

### Planned Features
- **Machine Learning Optimization**: AI-driven performance optimization
- **Real-time Quality Adjustment**: Frame-by-frame quality adjustment
- **Advanced Analytics**: Player behavior prediction
- **Cloud Optimization**: Server-side optimization recommendations

### Scalability Improvements
- **Modular Architecture**: Plugin-based tool system
- **Custom Optimization Profiles**: User-defined optimization settings
- **Performance Prediction**: Predictive performance modeling
- **Automated Testing**: Automated performance testing

---

**Riley**: "That's the complete tools documentation! Time to optimize this cyberpunk dog-chase game to perfection!"  
**Nibble**: "Bark! Bark! (Translation: Let's go optimize some games!)"