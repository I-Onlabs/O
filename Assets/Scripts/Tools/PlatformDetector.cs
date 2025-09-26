using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Platform detector for identifying device capabilities and optimizing accordingly.
    /// Riley: "Gotta detect what platform we're running on and optimize accordingly!"
    /// Nibble: "Bark! (Translation: Detect the platform!)"
    /// </summary>
    public class PlatformDetector : MonoBehaviour
    {
        [System.Serializable]
        public class DeviceInfo
        {
            [Header("Basic Info")]
            public string deviceName;
            public string deviceModel;
            public string operatingSystem;
            public string processorType;
            public int processorCount;
            public int systemMemorySize;
            
            [Header("Graphics")]
            public string graphicsDeviceName;
            public string graphicsDeviceVersion;
            public int graphicsMemorySize;
            public GraphicsDeviceType graphicsDeviceType;
            public bool supportsInstancing;
            public bool supportsComputeShaders;
            public bool supportsGeometryShaders;
            public bool supportsTessellationShaders;
            
            [Header("Mobile Specific")]
            public bool isMobile;
            public bool isTablet;
            public bool isPhone;
            public float screenDPI;
            public int screenWidth;
            public int screenHeight;
            public float aspectRatio;
            
            [Header("Performance Tier")]
            public PerformanceTier performanceTier;
            public bool isLowEndDevice;
            public bool isMidRangeDevice;
            public bool isHighEndDevice;
        }

        public enum PerformanceTier
        {
            Low,        // Low-end devices, basic graphics
            Medium,     // Mid-range devices, balanced graphics
            High,       // High-end devices, full graphics
            Ultra       // Ultra-high-end devices, maximum graphics
        }

        [Header("Detection Settings")]
        [SerializeField] private bool enableDetailedDetection = true;
        [SerializeField] private bool logDeviceInfo = true;
        [SerializeField] private bool autoOptimizeOnDetection = true;

        private DeviceInfo _deviceInfo;
        private bool _isDetected;

        // Events
        public System.Action<DeviceInfo> OnDeviceDetected;
        public System.Action<PerformanceTier> OnPerformanceTierDetected;
        public System.Action<string> OnDeviceInfoLog;

        private void Awake()
        {
            _deviceInfo = new DeviceInfo();
        }

        private void Start()
        {
            DetectPlatform();
            
            if (autoOptimizeOnDetection)
            {
                OptimizeForDetectedPlatform();
            }
        }

        /// <summary>
        /// Detects the current platform and device capabilities.
        /// Riley: "Time to detect what we're running on!"
        /// </summary>
        public void DetectPlatform()
        {
            if (_isDetected) return;

            try
            {
                DetectBasicInfo();
                DetectGraphicsInfo();
                DetectMobileInfo();
                DetectPerformanceTier();
                
                _isDetected = true;
                
                if (logDeviceInfo)
                {
                    LogDeviceInfo();
                }
                
                OnDeviceDetected?.Invoke(_deviceInfo);
                OnPerformanceTierDetected?.Invoke(_deviceInfo.performanceTier);
                
                Debug.Log("Riley: Platform detection complete!");
                Debug.Log($"Nibble: *bark* (Translation: Running on {_deviceInfo.deviceName}!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Platform detection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Detects basic device information.
        /// Riley: "Get the basic device info!"
        /// </summary>
        private void DetectBasicInfo()
        {
            _deviceInfo.deviceName = SystemInfo.deviceName;
            _deviceInfo.deviceModel = SystemInfo.deviceModel;
            _deviceInfo.operatingSystem = SystemInfo.operatingSystem;
            _deviceInfo.processorType = SystemInfo.processorType;
            _deviceInfo.processorCount = SystemInfo.processorCount;
            _deviceInfo.systemMemorySize = SystemInfo.systemMemorySize;
        }

        /// <summary>
        /// Detects graphics device information.
        /// Nibble: "Bark! (Translation: Detect graphics info!)"
        /// </summary>
        private void DetectGraphicsInfo()
        {
            _deviceInfo.graphicsDeviceName = SystemInfo.graphicsDeviceName;
            _deviceInfo.graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
            _deviceInfo.graphicsMemorySize = SystemInfo.graphicsMemorySize;
            _deviceInfo.graphicsDeviceType = SystemInfo.graphicsDeviceType;
            _deviceInfo.supportsInstancing = SystemInfo.supportsInstancing;
            _deviceInfo.supportsComputeShaders = SystemInfo.supportsComputeShaders;
            _deviceInfo.supportsGeometryShaders = SystemInfo.supportsGeometryShaders;
            _deviceInfo.supportsTessellationShaders = SystemInfo.supportsTessellationShaders;
        }

        /// <summary>
        /// Detects mobile-specific information.
        /// Riley: "Detect mobile info!"
        /// </summary>
        private void DetectMobileInfo()
        {
            _deviceInfo.isMobile = Application.isMobilePlatform;
            _deviceInfo.screenDPI = Screen.dpi;
            _deviceInfo.screenWidth = Screen.width;
            _deviceInfo.screenHeight = Screen.height;
            _deviceInfo.aspectRatio = (float)Screen.width / Screen.height;
            
            if (_deviceInfo.isMobile)
            {
                DetectMobileDeviceType();
            }
        }

        /// <summary>
        /// Detects mobile device type (phone vs tablet).
        /// Nibble: "Bark! (Translation: Detect mobile type!)"
        /// </summary>
        private void DetectMobileDeviceType()
        {
            var screenSize = Mathf.Sqrt(Mathf.Pow(Screen.width / Screen.dpi, 2) + Mathf.Pow(Screen.height / Screen.dpi, 2));
            
            if (screenSize >= 7f)
            {
                _deviceInfo.isTablet = true;
                _deviceInfo.isPhone = false;
            }
            else
            {
                _deviceInfo.isTablet = false;
                _deviceInfo.isPhone = true;
            }
        }

        /// <summary>
        /// Detects performance tier based on device capabilities.
        /// Riley: "Detect performance tier!"
        /// </summary>
        private void DetectPerformanceTier()
        {
            var score = CalculatePerformanceScore();
            
            if (score >= 80)
            {
                _deviceInfo.performanceTier = PerformanceTier.Ultra;
                _deviceInfo.isHighEndDevice = true;
            }
            else if (score >= 60)
            {
                _deviceInfo.performanceTier = PerformanceTier.High;
                _deviceInfo.isHighEndDevice = true;
            }
            else if (score >= 40)
            {
                _deviceInfo.performanceTier = PerformanceTier.Medium;
                _deviceInfo.isMidRangeDevice = true;
            }
            else
            {
                _deviceInfo.performanceTier = PerformanceTier.Low;
                _deviceInfo.isLowEndDevice = true;
            }
        }

        /// <summary>
        /// Calculates performance score based on device capabilities.
        /// Riley: "Calculate performance score!"
        /// </summary>
        private int CalculatePerformanceScore()
        {
            var score = 0;
            
            // Memory score (0-20 points)
            if (_deviceInfo.systemMemorySize >= 8192) score += 20;
            else if (_deviceInfo.systemMemorySize >= 4096) score += 15;
            else if (_deviceInfo.systemMemorySize >= 2048) score += 10;
            else if (_deviceInfo.systemMemorySize >= 1024) score += 5;
            
            // Graphics memory score (0-20 points)
            if (_deviceInfo.graphicsMemorySize >= 8192) score += 20;
            else if (_deviceInfo.graphicsMemorySize >= 4096) score += 15;
            else if (_deviceInfo.graphicsMemorySize >= 2048) score += 10;
            else if (_deviceInfo.graphicsMemorySize >= 1024) score += 5;
            
            // Processor count score (0-15 points)
            if (_deviceInfo.processorCount >= 8) score += 15;
            else if (_deviceInfo.processorCount >= 4) score += 10;
            else if (_deviceInfo.processorCount >= 2) score += 5;
            
            // Graphics features score (0-25 points)
            if (_deviceInfo.supportsInstancing) score += 5;
            if (_deviceInfo.supportsComputeShaders) score += 5;
            if (_deviceInfo.supportsGeometryShaders) score += 5;
            if (_deviceInfo.supportsTessellationShaders) score += 5;
            
            // Graphics device type score (0-10 points)
            switch (_deviceInfo.graphicsDeviceType)
            {
                case GraphicsDeviceType.Direct3D12:
                case GraphicsDeviceType.Vulkan:
                    score += 10;
                    break;
                case GraphicsDeviceType.Direct3D11:
                case GraphicsDeviceType.Metal:
                    score += 8;
                    break;
                case GraphicsDeviceType.OpenGLCore:
                    score += 6;
                    break;
                case GraphicsDeviceType.OpenGLES3:
                    score += 4;
                    break;
                case GraphicsDeviceType.OpenGLES2:
                    score += 2;
                    break;
            }
            
            // Mobile penalty
            if (_deviceInfo.isMobile)
            {
                score = Mathf.RoundToInt(score * 0.8f);
            }
            
            return Mathf.Clamp(score, 0, 100);
        }

        /// <summary>
        /// Optimizes game settings for the detected platform.
        /// Riley: "Optimize for the detected platform!"
        /// </summary>
        private void OptimizeForDetectedPlatform()
        {
            Debug.Log($"Riley: Optimizing for {_deviceInfo.performanceTier} tier device...");
            
            switch (_deviceInfo.performanceTier)
            {
                case PerformanceTier.Low:
                    OptimizeForLowEndDevice();
                    break;
                case PerformanceTier.Medium:
                    OptimizeForMidRangeDevice();
                    break;
                case PerformanceTier.High:
                    OptimizeForHighEndDevice();
                    break;
                case PerformanceTier.Ultra:
                    OptimizeForUltraHighEndDevice();
                    break;
            }
        }

        /// <summary>
        /// Optimizes for low-end devices.
        /// Nibble: "Bark! (Translation: Optimize for low-end!)"
        /// </summary>
        private void OptimizeForLowEndDevice()
        {
            // Set low quality settings
            QualitySettings.SetQualityLevel(0);
            QualitySettings.pixelLightCount = 1;
            QualitySettings.masterTextureLimit = 2;
            QualitySettings.antiAliasing = 0;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.shadowDistance = 20f;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
            
            Debug.Log("Riley: Optimized for low-end device - 30 FPS target, minimal effects");
        }

        /// <summary>
        /// Optimizes for mid-range devices.
        /// Riley: "Optimize for mid-range device!"
        /// </summary>
        private void OptimizeForMidRangeDevice()
        {
            // Set medium quality settings
            QualitySettings.SetQualityLevel(2);
            QualitySettings.pixelLightCount = 4;
            QualitySettings.masterTextureLimit = 1;
            QualitySettings.antiAliasing = 1;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowDistance = 50f;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            
            Debug.Log("Riley: Optimized for mid-range device - 60 FPS target, balanced effects");
        }

        /// <summary>
        /// Optimizes for high-end devices.
        /// Nibble: "Bark! (Translation: Optimize for high-end!)"
        /// </summary>
        private void OptimizeForHighEndDevice()
        {
            // Set high quality settings
            QualitySettings.SetQualityLevel(3);
            QualitySettings.pixelLightCount = 6;
            QualitySettings.masterTextureLimit = 0;
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowDistance = 75f;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            
            Debug.Log("Riley: Optimized for high-end device - 60 FPS target, high effects");
        }

        /// <summary>
        /// Optimizes for ultra-high-end devices.
        /// Riley: "Optimize for ultra-high-end device!"
        /// </summary>
        private void OptimizeForUltraHighEndDevice()
        {
            // Set ultra quality settings
            QualitySettings.SetQualityLevel(4);
            QualitySettings.pixelLightCount = 8;
            QualitySettings.masterTextureLimit = 0;
            QualitySettings.antiAliasing = 3;
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            QualitySettings.shadowDistance = 100f;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1; // No limit
            
            Debug.Log("Riley: Optimized for ultra-high-end device - unlimited FPS, maximum effects");
        }

        /// <summary>
        /// Logs device information.
        /// Riley: "Log the device info!"
        /// </summary>
        private void LogDeviceInfo()
        {
            var log = $"Device Information:\n" +
                     $"Device: {_deviceInfo.deviceName}\n" +
                     $"Model: {_deviceInfo.deviceModel}\n" +
                     $"OS: {_deviceInfo.operatingSystem}\n" +
                     $"Processor: {_deviceInfo.processorType} ({_deviceInfo.processorCount} cores)\n" +
                     $"Memory: {_deviceInfo.systemMemorySize}MB\n" +
                     $"Graphics: {_deviceInfo.graphicsDeviceName}\n" +
                     $"Graphics Memory: {_deviceInfo.graphicsMemorySize}MB\n" +
                     $"Graphics Type: {_deviceInfo.graphicsDeviceType}\n" +
                     $"Performance Tier: {_deviceInfo.performanceTier}\n" +
                     $"Mobile: {_deviceInfo.isMobile}\n" +
                     $"Screen: {_deviceInfo.screenWidth}x{_deviceInfo.screenHeight} @ {_deviceInfo.screenDPI} DPI\n" +
                     $"Aspect Ratio: {_deviceInfo.aspectRatio:F2}";
            
            Debug.Log(log);
            OnDeviceInfoLog?.Invoke(log);
        }

        /// <summary>
        /// Gets the detected device information.
        /// Riley: "Get the device info!"
        /// </summary>
        public DeviceInfo GetDeviceInfo()
        {
            return _deviceInfo;
        }

        /// <summary>
        /// Gets the detected performance tier.
        /// Nibble: "Bark! (Translation: Get performance tier!)"
        /// </summary>
        public PerformanceTier GetPerformanceTier()
        {
            return _deviceInfo.performanceTier;
        }

        /// <summary>
        /// Checks if the device is mobile.
        /// Riley: "Is this a mobile device?"
        /// </summary>
        public bool IsMobileDevice()
        {
            return _deviceInfo.isMobile;
        }

        /// <summary>
        /// Checks if the device is a tablet.
        /// Riley: "Is this a tablet?"
        /// </summary>
        public bool IsTabletDevice()
        {
            return _deviceInfo.isTablet;
        }

        /// <summary>
        /// Checks if the device is a phone.
        /// Nibble: "Bark! (Translation: Is this a phone?)"
        /// </summary>
        public bool IsPhoneDevice()
        {
            return _deviceInfo.isPhone;
        }

        /// <summary>
        /// Checks if the device is low-end.
        /// Riley: "Is this a low-end device?"
        /// </summary>
        public bool IsLowEndDevice()
        {
            return _deviceInfo.isLowEndDevice;
        }

        /// <summary>
        /// Checks if the device is mid-range.
        /// Riley: "Is this a mid-range device?"
        /// </summary>
        public bool IsMidRangeDevice()
        {
            return _deviceInfo.isMidRangeDevice;
        }

        /// <summary>
        /// Checks if the device is high-end.
        /// Nibble: "Bark! (Translation: Is this a high-end device?)"
        /// </summary>
        public bool IsHighEndDevice()
        {
            return _deviceInfo.isHighEndDevice;
        }

        /// <summary>
        /// Gets a summary of device capabilities.
        /// Riley: "Get device capabilities summary!"
        /// </summary>
        public string GetDeviceCapabilitiesSummary()
        {
            var capabilities = new System.Collections.Generic.List<string>();
            
            if (_deviceInfo.supportsInstancing) capabilities.Add("Instancing");
            if (_deviceInfo.supportsComputeShaders) capabilities.Add("Compute Shaders");
            if (_deviceInfo.supportsGeometryShaders) capabilities.Add("Geometry Shaders");
            if (_deviceInfo.supportsTessellationShaders) capabilities.Add("Tessellation Shaders");
            
            return string.Join(", ", capabilities);
        }

        /// <summary>
        /// Forces re-detection of platform.
        /// Riley: "Re-detect the platform!"
        /// </summary>
        public void RedetectPlatform()
        {
            _isDetected = false;
            DetectPlatform();
        }
    }
}