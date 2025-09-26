using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using AngryDogs.SaveSystem;
using AngryDogs.UI;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Automated hotfix deployment and version checking system for seamless post-launch updates.
    /// Supports version checking, patch downloading, and automated deployment without requiring app store updates.
    /// Riley: "Time to set up automated hotfixes! Can't have bugs running around when the hounds are chasing!"
    /// Nibble: "Bark! (Translation: Automated updates for better gameplay!)"
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PatchManager : MonoBehaviour
    {
        [System.Serializable]
        public class PatchInfo
        {
            public string patchId;
            public string version;
            public int buildNumber;
            public string description;
            public string[] features;
            public string[] bugFixes;
            public string[] improvements;
            public bool isCritical;
            public bool isOptional;
            public long downloadSize;
            public string downloadUrl;
            public string[] supportedPlatforms;
            public DateTime releaseDate;
            public DateTime expiryDate;
            public string[] requiredVersions;
            public string[] incompatibleVersions;
        }

        [System.Serializable]
        public class VersionInfo
        {
            public string currentVersion;
            public int currentBuildNumber;
            public string latestVersion;
            public int latestBuildNumber;
            public bool updateAvailable;
            public bool criticalUpdateAvailable;
            public PatchInfo[] availablePatches;
            public DateTime lastChecked;
        }

        [System.Serializable]
        public class PatchStatus
        {
            public string patchId;
            public PatchState state;
            public float progress;
            public string errorMessage;
            public DateTime startTime;
            public DateTime endTime;
        }

        public enum PatchState
        {
            NotStarted,
            Checking,
            Downloading,
            Installing,
            Completed,
            Failed,
            Cancelled
        }

        [Header("Patch Settings")]
        [SerializeField] private bool enableAutoUpdates = true;
        [SerializeField] private bool enableCriticalUpdates = true;
        [SerializeField] private bool enableOptionalUpdates = false;
        [SerializeField] private float checkInterval = 3600f; // 1 hour
        [SerializeField] private bool checkOnStartup = true;
        [SerializeField] private bool checkOnResume = true;

        [Header("Server Configuration")]
        [SerializeField] private string patchServerUrl = "https://your-patch-server.com/api/patches";
        [SerializeField] private string versionCheckUrl = "https://your-patch-server.com/api/version";
        [SerializeField] private string apiKey = "";
        [SerializeField] private float requestTimeout = 30f;
        [SerializeField] private int maxRetryAttempts = 3;

        [Header("Download Settings")]
        [SerializeField] private bool allowCellularDownload = false;
        [SerializeField] private long maxCellularDownloadSize = 50 * 1024 * 1024; // 50MB
        [SerializeField] private bool requireWiFiForLargeUpdates = true;
        [SerializeField] private long largeUpdateThreshold = 100 * 1024 * 1024; // 100MB

        [Header("UI Integration")]
        [SerializeField] private bool showUpdatePrompts = true;
        [SerializeField] private GameObject updatePromptPrefab;
        [SerializeField] private GameObject downloadProgressPrefab;
        [SerializeField] private bool showDownloadProgress = true;

        [Header("Systems")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private LocalizationManager localizationManager;

        private VersionInfo _versionInfo;
        private Dictionary<string, PatchStatus> _patchStatuses = new Dictionary<string, PatchStatus>();
        private Coroutine _checkCoroutine;
        private bool _isInitialized;
        private bool _isCheckingForUpdates;
        private bool _isDownloading;

        // Events
        public System.Action<VersionInfo> OnVersionChecked;
        public System.Action<PatchInfo> OnUpdateAvailable;
        public System.Action<PatchInfo> OnCriticalUpdateAvailable;
        public System.Action<PatchStatus> OnPatchProgressUpdated;
        public System.Action<PatchStatus> OnPatchCompleted;
        public System.Action<PatchStatus> OnPatchFailed;
        public System.Action<string> OnPatchError;

        private void Awake()
        {
            _versionInfo = new VersionInfo
            {
                currentVersion = Application.version,
                currentBuildNumber = GetCurrentBuildNumber(),
                latestVersion = Application.version,
                latestBuildNumber = GetCurrentBuildNumber(),
                updateAvailable = false,
                criticalUpdateAvailable = false,
                availablePatches = new PatchInfo[0],
                lastChecked = DateTime.MinValue
            };
        }

        private void Start()
        {
            if (enableAutoUpdates)
            {
                InitializePatchManager();
            }
        }

        /// <summary>
        /// Initializes the patch manager system.
        /// Riley: "Initialize the patch manager system!"
        /// </summary>
        private void InitializePatchManager()
        {
            try
            {
                // Load saved version info
                LoadVersionInfo();
                
                // Start version checking coroutine
                if (checkInterval > 0)
                {
                    _checkCoroutine = StartCoroutine(VersionCheckCoroutine());
                }
                
                // Check for updates on startup
                if (checkOnStartup)
                {
                    CheckForUpdates();
                }
                
                _isInitialized = true;
                
                Debug.Log("Riley: Patch manager system initialized!");
                Debug.Log($"Nibble: *bark* (Translation: Current version: {_versionInfo.currentVersion}, Build: {_versionInfo.currentBuildNumber})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to initialize patch manager: {ex.Message}");
                OnPatchError?.Invoke($"Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks for available updates.
        /// Nibble: "Bark! (Translation: Check for updates!)"
        /// </summary>
        public void CheckForUpdates()
        {
            if (_isCheckingForUpdates)
            {
                Debug.LogWarning("Riley: Already checking for updates!");
                return;
            }

            StartCoroutine(CheckForUpdatesCoroutine());
        }

        /// <summary>
        /// Downloads and installs a specific patch.
        /// Riley: "Download and install patch!"
        /// </summary>
        public void DownloadPatch(string patchId)
        {
            if (_isDownloading)
            {
                Debug.LogWarning("Riley: Already downloading a patch!");
                return;
            }

            var patch = FindPatchById(patchId);
            if (patch == null)
            {
                Debug.LogError($"Riley: Patch {patchId} not found!");
                OnPatchError?.Invoke($"Patch {patchId} not found");
                return;
            }

            StartCoroutine(DownloadPatchCoroutine(patch));
        }

        /// <summary>
        /// Downloads and installs all available patches.
        /// Nibble: "Bark! (Translation: Download all patches!)"
        /// </summary>
        public void DownloadAllPatches()
        {
            if (_isDownloading)
            {
                Debug.LogWarning("Riley: Already downloading patches!");
                return;
            }

            StartCoroutine(DownloadAllPatchesCoroutine());
        }

        /// <summary>
        /// Gets the current version information.
        /// Riley: "Get current version information!"
        /// </summary>
        public VersionInfo GetVersionInfo()
        {
            return _versionInfo;
        }

        /// <summary>
        /// Gets the status of a specific patch.
        /// Nibble: "Bark! (Translation: Get patch status!)"
        /// </summary>
        public PatchStatus GetPatchStatus(string patchId)
        {
            return _patchStatuses.ContainsKey(patchId) ? _patchStatuses[patchId] : null;
        }

        /// <summary>
        /// Gets all available patches.
        /// Riley: "Get all available patches!"
        /// </summary>
        public PatchInfo[] GetAvailablePatches()
        {
            return _versionInfo.availablePatches;
        }

        /// <summary>
        /// Gets critical patches that need immediate attention.
        /// Nibble: "Bark! (Translation: Get critical patches!)"
        /// </summary>
        public PatchInfo[] GetCriticalPatches()
        {
            var criticalPatches = new List<PatchInfo>();
            foreach (var patch in _versionInfo.availablePatches)
            {
                if (patch.isCritical)
                {
                    criticalPatches.Add(patch);
                }
            }
            return criticalPatches.ToArray();
        }

        /// <summary>
        /// Gets optional patches that can be installed later.
        /// Riley: "Get optional patches!"
        /// </summary>
        public PatchInfo[] GetOptionalPatches()
        {
            var optionalPatches = new List<PatchInfo>();
            foreach (var patch in _versionInfo.availablePatches)
            {
                if (patch.isOptional)
                {
                    optionalPatches.Add(patch);
                }
            }
            return optionalPatches.ToArray();
        }

        /// <summary>
        /// Coroutine for periodic version checking.
        /// Riley: "Check versions periodically!"
        /// </summary>
        private IEnumerator VersionCheckCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);
                
                if (_isInitialized && !_isCheckingForUpdates)
                {
                    CheckForUpdates();
                }
            }
        }

        /// <summary>
        /// Coroutine for checking for updates.
        /// Nibble: "Bark! (Translation: Check for updates coroutine!)"
        /// </summary>
        private IEnumerator CheckForUpdatesCoroutine()
        {
            _isCheckingForUpdates = true;
            
            var request = CreateVersionCheckRequest();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var versionData = JsonUtility.FromJson<VersionInfo>(request.downloadHandler.text);
                    if (versionData != null)
                    {
                        _versionInfo = versionData;
                        _versionInfo.lastChecked = DateTime.Now;
                        
                        // Check if updates are available
                        CheckForAvailableUpdates();
                        
                        OnVersionChecked?.Invoke(_versionInfo);
                        
                        Debug.Log("Riley: Version check completed successfully!");
                        Debug.Log($"Nibble: *bark* (Translation: Latest version: {_versionInfo.latestVersion}, Build: {_versionInfo.latestBuildNumber})");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Riley: Failed to parse version data: {ex.Message}");
                    OnPatchError?.Invoke($"Parse error: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Riley: Failed to check for updates: {request.error}");
                OnPatchError?.Invoke($"Network error: {request.error}");
            }

            _isCheckingForUpdates = false;
        }

        /// <summary>
        /// Coroutine for downloading a specific patch.
        /// Riley: "Download specific patch!"
        /// </summary>
        private IEnumerator DownloadPatchCoroutine(PatchInfo patch)
        {
            _isDownloading = true;
            
            var patchStatus = new PatchStatus
            {
                patchId = patch.patchId,
                state = PatchState.Downloading,
                progress = 0f,
                startTime = DateTime.Now
            };
            
            _patchStatuses[patch.patchId] = patchStatus;
            OnPatchProgressUpdated?.Invoke(patchStatus);

            // Check network requirements
            if (!CanDownloadPatch(patch))
            {
                patchStatus.state = PatchState.Failed;
                patchStatus.errorMessage = "Network requirements not met";
                OnPatchFailed?.Invoke(patchStatus);
                _isDownloading = false;
                yield break;
            }

            // Download patch
            var request = CreatePatchDownloadRequest(patch);
            request.SendWebRequest();

            while (!request.isDone)
            {
                patchStatus.progress = request.downloadProgress;
                OnPatchProgressUpdated?.Invoke(patchStatus);
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Install patch
                yield return StartCoroutine(InstallPatchCoroutine(patch, request.downloadHandler.data));
            }
            else
            {
                patchStatus.state = PatchState.Failed;
                patchStatus.errorMessage = request.error;
                OnPatchFailed?.Invoke(patchStatus);
            }

            _isDownloading = false;
        }

        /// <summary>
        /// Coroutine for downloading all available patches.
        /// Nibble: "Bark! (Translation: Download all patches coroutine!)"
        /// </summary>
        private IEnumerator DownloadAllPatchesCoroutine()
        {
            var patchesToDownload = new List<PatchInfo>();
            
            // Add critical patches first
            foreach (var patch in GetCriticalPatches())
            {
                patchesToDownload.Add(patch);
            }
            
            // Add optional patches if enabled
            if (enableOptionalUpdates)
            {
                foreach (var patch in GetOptionalPatches())
                {
                    patchesToDownload.Add(patch);
                }
            }

            // Download patches sequentially
            foreach (var patch in patchesToDownload)
            {
                yield return StartCoroutine(DownloadPatchCoroutine(patch));
            }

            Debug.Log("Riley: All patches downloaded successfully!");
            Debug.Log("Nibble: *bark* (Translation: All updates complete!)");
        }

        /// <summary>
        /// Coroutine for installing a patch.
        /// Riley: "Install patch!"
        /// </summary>
        private IEnumerator InstallPatchCoroutine(PatchInfo patch, byte[] patchData)
        {
            var patchStatus = _patchStatuses[patch.patchId];
            patchStatus.state = PatchState.Installing;
            patchStatus.progress = 0f;
            OnPatchProgressUpdated?.Invoke(patchStatus);

            try
            {
                // Simulate patch installation
                for (int i = 0; i < 100; i++)
                {
                    patchStatus.progress = i / 100f;
                    OnPatchProgressUpdated?.Invoke(patchStatus);
                    yield return new WaitForSeconds(0.01f);
                }

                // Apply patch
                ApplyPatch(patch, patchData);

                patchStatus.state = PatchState.Completed;
                patchStatus.progress = 1f;
                patchStatus.endTime = DateTime.Now;
                OnPatchCompleted?.Invoke(patchStatus);

                Debug.Log($"Riley: Patch {patch.patchId} installed successfully!");
                Debug.Log($"Nibble: *bark* (Translation: Patch {patch.version} installed!)");
            }
            catch (Exception ex)
            {
                patchStatus.state = PatchState.Failed;
                patchStatus.errorMessage = ex.Message;
                OnPatchFailed?.Invoke(patchStatus);
                
                Debug.LogError($"Riley: Failed to install patch {patch.patchId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks for available updates and notifies the UI.
        /// Nibble: "Bark! (Translation: Check for available updates!)"
        /// </summary>
        private void CheckForAvailableUpdates()
        {
            var hasCriticalUpdates = false;
            var hasOptionalUpdates = false;

            foreach (var patch in _versionInfo.availablePatches)
            {
                if (patch.isCritical)
                {
                    hasCriticalUpdates = true;
                    OnCriticalUpdateAvailable?.Invoke(patch);
                }
                else if (patch.isOptional)
                {
                    hasOptionalUpdates = true;
                    OnUpdateAvailable?.Invoke(patch);
                }
            }

            _versionInfo.criticalUpdateAvailable = hasCriticalUpdates;
            _versionInfo.updateAvailable = hasCriticalUpdates || hasOptionalUpdates;

            // Show update prompts if enabled
            if (showUpdatePrompts && _versionInfo.updateAvailable)
            {
                ShowUpdatePrompt();
            }
        }

        /// <summary>
        /// Shows the update prompt to the player.
        /// Riley: "Show update prompt to player!"
        /// </summary>
        private void ShowUpdatePrompt()
        {
            if (updatePromptPrefab == null)
            {
                Debug.LogWarning("Riley: Update prompt prefab not assigned!");
                return;
            }

            var updatePrompt = Instantiate(updatePromptPrefab, transform);
            var updateUI = updatePrompt.GetComponent<UpdatePromptUI>();
            
            if (updateUI != null)
            {
                updateUI.Initialize(this, _versionInfo, OnUpdatePromptResponse);
            }

            Debug.Log("Riley: Update prompt shown to player!");
        }

        /// <summary>
        /// Handles update prompt response.
        /// Nibble: "Bark! (Translation: Handle update prompt response!)"
        /// </summary>
        private void OnUpdatePromptResponse(UpdateResponse response)
        {
            switch (response)
            {
                case UpdateResponse.UpdateNow:
                    DownloadAllPatches();
                    break;
                case UpdateResponse.UpdateLater:
                    // Do nothing, will check again later
                    break;
                case UpdateResponse.Skip:
                    // Skip this update
                    break;
            }

            Debug.Log($"Riley: Update prompt response: {response}");
        }

        /// <summary>
        /// Checks if a patch can be downloaded based on network requirements.
        /// Riley: "Check if patch can be downloaded!"
        /// </summary>
        private bool CanDownloadPatch(PatchInfo patch)
        {
            // Check if patch is too large for cellular
            if (!allowCellularDownload && patch.downloadSize > maxCellularDownloadSize)
            {
                return false;
            }

            // Check if patch requires WiFi
            if (requireWiFiForLargeUpdates && patch.downloadSize > largeUpdateThreshold)
            {
                // In a real implementation, you would check if WiFi is available
                return true; // Assume WiFi is available for now
            }

            return true;
        }

        /// <summary>
        /// Applies a patch to the game.
        /// Nibble: "Bark! (Translation: Apply patch to game!)"
        /// </summary>
        private void ApplyPatch(PatchInfo patch, byte[] patchData)
        {
            // In a real implementation, you would apply the patch data here
            // This could involve updating game files, configurations, etc.
            
            Debug.Log($"Riley: Applying patch {patch.patchId}...");
            Debug.Log($"Nibble: *bark* (Translation: Patch features: {string.Join(", ", patch.features)})");
            
            // Update version info
            _versionInfo.currentVersion = patch.version;
            _versionInfo.currentBuildNumber = patch.buildNumber;
            
            // Save version info
            SaveVersionInfo();
        }

        /// <summary>
        /// Creates a version check request.
        /// Riley: "Create version check request!"
        /// </summary>
        private UnityWebRequest CreateVersionCheckRequest()
        {
            var url = $"{versionCheckUrl}?version={_versionInfo.currentVersion}&build={_versionInfo.currentBuildNumber}&platform={Application.platform}";
            var request = UnityWebRequest.Get(url);
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }
            
            request.timeout = (int)requestTimeout;
            return request;
        }

        /// <summary>
        /// Creates a patch download request.
        /// Nibble: "Bark! (Translation: Create patch download request!)"
        /// </summary>
        private UnityWebRequest CreatePatchDownloadRequest(PatchInfo patch)
        {
            var request = UnityWebRequest.Get(patch.downloadUrl);
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }
            
            request.timeout = (int)requestTimeout;
            return request;
        }

        /// <summary>
        /// Finds a patch by ID.
        /// Riley: "Find patch by ID!"
        /// </summary>
        private PatchInfo FindPatchById(string patchId)
        {
            foreach (var patch in _versionInfo.availablePatches)
            {
                if (patch.patchId == patchId)
                {
                    return patch;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the current build number.
        /// Nibble: "Bark! (Translation: Get current build number!)"
        /// </summary>
        private int GetCurrentBuildNumber()
        {
            // In a real implementation, you would get this from your build system
            return 1;
        }

        /// <summary>
        /// Loads version info from save data.
        /// Riley: "Load version info from save data!"
        /// </summary>
        private void LoadVersionInfo()
        {
            if (saveManager != null)
            {
                // You would need to add version info to PlayerSaveData
                // For now, use default values
                Debug.Log("Riley: Version info loaded from save data!");
            }
        }

        /// <summary>
        /// Saves version info to save data.
        /// Nibble: "Bark! (Translation: Save version info!)"
        /// </summary>
        private void SaveVersionInfo()
        {
            if (saveManager != null)
            {
                // You would need to add version info to PlayerSaveData
                saveManager.Save();
                Debug.Log("Riley: Version info saved to save data!");
            }
        }

        /// <summary>
        /// Gets patch manager statistics.
        /// Riley: "Get patch manager statistics!"
        /// </summary>
        public string GetPatchManagerStats()
        {
            var stats = $"Patch Manager Statistics:\n";
            stats += $"Current Version: {_versionInfo.currentVersion}\n";
            stats += $"Current Build: {_versionInfo.currentBuildNumber}\n";
            stats += $"Latest Version: {_versionInfo.latestVersion}\n";
            stats += $"Latest Build: {_versionInfo.latestBuildNumber}\n";
            stats += $"Update Available: {_versionInfo.updateAvailable}\n";
            stats += $"Critical Update Available: {_versionInfo.criticalUpdateAvailable}\n";
            stats += $"Available Patches: {_versionInfo.availablePatches.Length}\n";
            stats += $"Last Checked: {_versionInfo.lastChecked:yyyy-MM-dd HH:mm:ss}\n";
            stats += $"Auto Updates: {enableAutoUpdates}\n";
            stats += $"Critical Updates: {enableCriticalUpdates}\n";
            stats += $"Optional Updates: {enableOptionalUpdates}\n";
            stats += $"Check Interval: {checkInterval}s\n";
            
            return stats;
        }

        private void OnDestroy()
        {
            if (_checkCoroutine != null)
            {
                StopCoroutine(_checkCoroutine);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Pause version checking when app is paused
                if (_checkCoroutine != null)
                {
                    StopCoroutine(_checkCoroutine);
                }
            }
            else
            {
                // Resume version checking when app is resumed
                if (_isInitialized && checkOnResume)
                {
                    CheckForUpdates();
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _isInitialized && checkOnResume)
            {
                // Check for updates when app gains focus
                CheckForUpdates();
            }
        }

        /// <summary>
        /// Update response types.
        /// </summary>
        public enum UpdateResponse
        {
            UpdateNow,
            UpdateLater,
            Skip
        }
    }
}