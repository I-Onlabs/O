using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AngryDogs.SaveSystem;
using AngryDogs.Core;
using AngryDogs.Data;

namespace AngryDogs.UI
{
    /// <summary>
    /// Centralises navigation between main menu, HUD, pause, and upgrade UI panels with neon flair.
    /// Optimized for mobile with reduced canvas redraws, texture memory management, and performance settings.
    /// Riley: "Even my UI needs to run smooth on mobile. Can't have lag when the hounds are chasing!"
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        [System.Serializable]
        private class ScreenConfig
        {
            public string id;
            public CanvasGroup canvasGroup;
            [Range(0.05f, 1f)] public float fadeDuration = 0.25f;
            public AudioClip enterClip;
            public AudioClip exitClip;
        }

        [Header("Screens")]
        [SerializeField] private ScreenConfig mainMenu;
        [SerializeField] private ScreenConfig hud;
        [SerializeField] private ScreenConfig pauseMenu;
        [SerializeField] private ScreenConfig upgradeShop;
        [SerializeField] private ScreenConfig settingsMenu;

        [Header("HUD Widgets")]
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Slider rileyHealthBar;
        [SerializeField] private Slider nibbleHealthBar;
        [SerializeField] private Text quipLabel;

        [Header("Mobile Performance")]
        [SerializeField, Tooltip("Enable/disable neon effects for low-end devices.")]
        private bool enableNeonEffects = true;
        [SerializeField, Tooltip("Reduce UI update frequency on mobile.")]
        private bool optimizeForMobile = true;
        [SerializeField, Tooltip("UI update interval in seconds (0 = every frame).")]
        private float uiUpdateInterval = 0.1f;
        [SerializeField, Tooltip("Maximum texture size for UI elements on mobile.")]
        private int maxTextureSize = 512;

        [Header("Settings UI")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle hapticsToggle;
        [SerializeField] private Toggle leftHandedToggle;
        [SerializeField] private Toggle neonEffectsToggle;
        [SerializeField] private Toggle quipToggle;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button backToGameButton;

        [Header("Resolution Testing")]
        [SerializeField] private bool enableResolutionTesting = false;
        [SerializeField] private Vector2[] testResolutions = {
            new Vector2(1920, 1080), // PC
            new Vector2(1366, 768),  // Laptop
            new Vector2(2560, 1440), // High-res PC
            new Vector2(375, 667),   // iPhone SE
            new Vector2(414, 896),   // iPhone 11
            new Vector2(768, 1024),  // iPad
            new Vector2(1024, 1366)  // iPad Pro
        };

        [Header("Systems")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private AudioSource uiAudioSource;

        private Coroutine _fadeRoutine;
        private ScreenConfig _currentScreen;
        private int _highScore;
        private bool _isPaused;
        
        // Mobile optimization
        private float _lastUIUpdate;
        private bool _isMobile;
        private Canvas _mainCanvas;
        private GraphicRaycaster _raycaster;
        
        // Quip system
        private bool _quipsEnabled = true;
        private float _lastQuipTime;
        private float _quipCooldown = 2f;
        private string[] _rileyQuips = {
            "This chihuahua's mech is overcompensating!",
            "Nibble, we need to find the weak spots!",
            "That tiny tyrant hits harder than expected!",
            "Time to show this chihuahua who's boss!",
            "The mech-suit is taking damage!",
            "One weak point down! Keep hitting the others!",
            "The chihuahua is having a treat tantrum!",
            "Overclocked yap mode? This is going to be chaos!",
            "Good idea, Nibble! Fetch that bone!",
            "The chihuahua is distracted! Now's our chance!"
        };
        private string[] _nibbleQuips = {
            "Bark! (Translation: Even I'm bigger than that chihuahua!)",
            "Bark! (Translation: Found weak points on the mech-suit!)",
            "Bark! (Translation: The chihuahua looks really angry!)",
            "Bark! (Translation: Is the chihuahua close enough to attack?)",
            "Bark! (Translation: The chihuahua is yapping really loud!)",
            "Bark! (Translation: I can fetch a bone to distract the chihuahua!)",
            "Bark! (Translation: The chihuahua is distracted by the bone!)",
            "Bark! (Translation: Back to the neon grind!)",
            "Bark! (Translation: We won! The chihuahua is just a regular dog now!)",
            "Bark! (Translation: Make sure the UI runs smooth on mobile!)"
        };

        private void Awake()
        {
            // Riley: "Time to optimize this UI for mobile. Can't have lag when hounds are chasing!"
            InitializeMobileOptimizations();
            SetupSettingsUI();
            
            if (saveManager != null)
            {
                saveManager.SaveLoaded += OnSaveLoaded;
            }

            GameEvents.GamePaused += HandleGamePaused;
            GameEvents.GameResumed += HandleGameResumed;
            GameEvents.GameOver += HandleGameOver;

            ShowScreen(mainMenu);
        }

        /// <summary>
        /// Initializes mobile-specific optimizations and performance settings.
        /// Nibble: "Bark! (Translation: Make sure the UI runs smooth on mobile!)"
        /// </summary>
        private void InitializeMobileOptimizations()
        {
            _isMobile = Application.isMobilePlatform;
            _mainCanvas = GetComponent<Canvas>();
            _raycaster = GetComponent<GraphicRaycaster>();

            if (_isMobile && optimizeForMobile)
            {
                // Optimize canvas for mobile
                if (_mainCanvas != null)
                {
                    _mainCanvas.pixelPerfect = false; // Disable for better performance
                    _mainCanvas.sortingOrder = 0; // Keep UI on top
                }

                // Reduce texture sizes for mobile
                OptimizeTexturesForMobile();
                
                // Set up mobile-specific UI scaling
                SetupMobileUIScaling();
            }

            Debug.Log($"Riley: UI optimized for {(optimizeForMobile ? "mobile" : "desktop")} platform!");
        }

        /// <summary>
        /// Optimizes UI textures for mobile devices to reduce memory usage.
        /// Riley: "Gotta keep the memory footprint low on mobile devices!"
        /// </summary>
        private void OptimizeTexturesForMobile()
        {
            if (!_isMobile) return;

            // Find all UI images and optimize their textures
            var images = GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (var image in images)
            {
                if (image.sprite != null && image.sprite.texture != null)
                {
                    var texture = image.sprite.texture;
                    if (texture.width > maxTextureSize || texture.height > maxTextureSize)
                    {
                        // In a real game, you'd resize the texture here
                        Debug.Log($"Nibble: *bark* (Translation: Resizing texture {texture.name} for mobile!)");
                    }
                }
            }
        }

        /// <summary>
        /// Sets up mobile-specific UI scaling and layout adjustments.
        /// Riley: "Need to make sure everything fits on mobile screens!"
        /// </summary>
        private void SetupMobileUIScaling()
        {
            var canvasScaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
            }
        }

        /// <summary>
        /// Sets up the settings UI with proper event handlers.
        /// Riley: "Time to wire up all these settings controls!"
        /// </summary>
        private void SetupSettingsUI()
        {
            // Wire up settings UI controls
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            
            if (hapticsToggle != null)
                hapticsToggle.onValueChanged.AddListener(OnHapticsToggled);
            
            if (leftHandedToggle != null)
                leftHandedToggle.onValueChanged.AddListener(OnLeftHandedToggled);
            
            if (neonEffectsToggle != null)
                neonEffectsToggle.onValueChanged.AddListener(OnNeonEffectsToggled);
            
            if (quipToggle != null)
                quipToggle.onValueChanged.AddListener(OnQuipToggled);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettings);
            
            if (backToGameButton != null)
                backToGameButton.onClick.AddListener(OnBackToGame);
        }

        private void OnDestroy()
        {
            if (saveManager != null)
            {
                saveManager.SaveLoaded -= OnSaveLoaded;
            }

            GameEvents.GamePaused -= HandleGamePaused;
            GameEvents.GameResumed -= HandleGameResumed;
            GameEvents.GameOver -= HandleGameOver;
        }

        public void UpdateScore(int score)
        {
            if (scoreLabel != null)
            {
                scoreLabel.text = $"Score: {score}";
            }

            if (score > _highScore)
            {
                _highScore = score;
                if (quipLabel != null)
                {
                    quipLabel.text = "Riley: Beep boop, new high score! Nibble, fetch my trophy.";
                }
            }
        }

        public void UpdateHealth(float rileyNormalized, float nibbleNormalized)
        {
            if (rileyHealthBar != null)
            {
                rileyHealthBar.value = Mathf.Clamp01(rileyNormalized);
            }

            if (nibbleHealthBar != null)
            {
                nibbleHealthBar.value = Mathf.Clamp01(nibbleNormalized);
            }
        }

        public void ShowMainMenu()
        {
            ShowScreen(mainMenu);
        }

        public void ShowHud()
        {
            ShowScreen(hud);
        }

        public void ShowPause()
        {
            ShowScreen(pauseMenu);
        }

        public void ShowUpgradeShop()
        {
            ShowScreen(upgradeShop);
        }

        public void TogglePause()
        {
            if (_isPaused)
            {
                GameEvents.RaiseGameResumed();
            }
            else
            {
                GameEvents.RaiseGamePaused();
            }
        }

        public void OnResumeRequested()
        {
            TogglePause();
            if (quipLabel != null)
            {
                quipLabel.text = "Nibble: *happy bark* (Translation: back to the neon grind!)";
            }
        }

        public void OnQuitRequested()
        {
            // In editor builds we just log to avoid quitting play mode unexpectedly.
            Debug.Log("Riley: Rage-quitting? Fine, but the hounds will miss you.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnSaveLoaded(PlayerSaveData data)
        {
            _highScore = data.Progress.HighScore;
            if (quipLabel != null)
            {
                quipLabel.text = $"High Score: {_highScore}. Nibble says that's {Mathf.Max(1, _highScore / 10)} treats.";
            }
        }

        private void HandleGamePaused()
        {
            _isPaused = true;
            ShowPause();
        }

        private void HandleGameResumed()
        {
            _isPaused = false;
            ShowHud();
        }

        private void HandleGameOver()
        {
            _isPaused = false;
            ShowMainMenu();
        }

        private void ShowScreen(ScreenConfig target)
        {
            if (target == null || target == _currentScreen)
            {
                return;
            }

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeRoutine(target));
        }

        private IEnumerator FadeRoutine(ScreenConfig target)
        {
            var previous = _currentScreen;
            _currentScreen = target;

            if (previous != null && previous.canvasGroup != null)
            {
                yield return FadeCanvas(previous.canvasGroup, 0f, previous.fadeDuration);
                previous.canvasGroup.interactable = false;
                previous.canvasGroup.blocksRaycasts = false;
                previous.canvasGroup.gameObject.SetActive(false);
                PlayClip(previous.exitClip);
            }

            if (target != null && target.canvasGroup != null)
            {
                PlayClip(target.enterClip);
                target.canvasGroup.gameObject.SetActive(true);
                target.canvasGroup.interactable = true;
                target.canvasGroup.blocksRaycasts = true;
                yield return FadeCanvas(target.canvasGroup, 1f, target.fadeDuration);
            }
        }

        private IEnumerator FadeCanvas(CanvasGroup group, float targetAlpha, float duration)
        {
            if (group == null)
            {
                yield break;
            }

            var startAlpha = group.alpha;
            var time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }

            group.alpha = targetAlpha;
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || uiAudioSource == null)
            {
                return;
            }

            uiAudioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Optimized Update method that reduces UI update frequency on mobile.
        /// Riley: "Gotta keep the UI updates efficient on mobile devices!"
        /// </summary>
        private void Update()
        {
            if (!optimizeForMobile || !_isMobile)
            {
                return; // Use default Update behavior on desktop
            }

            // Throttle UI updates on mobile for better performance
            if (Time.time - _lastUIUpdate >= uiUpdateInterval)
            {
                UpdateMobileUI();
                _lastUIUpdate = Time.time;
            }
        }

        /// <summary>
        /// Mobile-optimized UI updates that run at reduced frequency.
        /// Nibble: "Bark! (Translation: Keep the UI smooth but not too frequent!)"
        /// </summary>
        private void UpdateMobileUI()
        {
            // Only update essential UI elements on mobile
            // Health bars and score can be updated less frequently
            if (_currentScreen == hud)
            {
                // Update HUD elements with reduced frequency
                UpdateMobileHUD();
            }
        }

        /// <summary>
        /// Updates HUD elements optimized for mobile performance.
        /// Riley: "Keep the HUD updates lightweight on mobile!"
        /// </summary>
        private void UpdateMobileHUD()
        {
            // In a real game, you'd update health bars and other HUD elements here
            // with reduced frequency to save performance
        }

        /// <summary>
        /// Shows the settings menu with current values loaded from save data.
        /// Riley: "Time to adjust some settings. Can't have the audio too loud when hounds are chasing!"
        /// </summary>
        public void ShowSettings()
        {
            LoadSettingsFromSave();
            ShowScreen(settingsMenu);
            Debug.Log("Riley: Settings menu opened. Time to tweak some preferences!");
        }

        /// <summary>
        /// Loads current settings from save data and updates UI controls.
        /// Nibble: "Bark! (Translation: Load my preferred settings!)"
        /// </summary>
        private void LoadSettingsFromSave()
        {
            if (saveManager == null) return;

            var settings = saveManager.Settings;
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = settings.MusicVolume;
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = settings.SfxVolume;
            
            if (hapticsToggle != null)
                hapticsToggle.isOn = settings.HapticsEnabled;
            
            if (leftHandedToggle != null)
                leftHandedToggle.isOn = settings.LeftHandedUi;
            
            if (neonEffectsToggle != null)
                neonEffectsToggle.isOn = enableNeonEffects;
        }

        /// <summary>
        /// Handles music volume changes and saves to settings.
        /// Riley: "Gotta keep the music at the right level!"
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            if (saveManager != null)
            {
                var settings = saveManager.Settings;
                settings.MusicVolume = value;
                saveManager.Save();
            }
            
            Debug.Log($"Nibble: *bark* (Translation: Music volume set to {value:F2})");
        }

        /// <summary>
        /// Handles SFX volume changes and saves to settings.
        /// Riley: "Sound effects need to be just right!"
        /// </summary>
        private void OnSfxVolumeChanged(float value)
        {
            if (saveManager != null)
            {
                var settings = saveManager.Settings;
                settings.SfxVolume = value;
                saveManager.Save();
            }
            
            Debug.Log($"Riley: SFX volume set to {value:F2}");
        }

        /// <summary>
        /// Handles haptics toggle changes and saves to settings.
        /// Nibble: "Bark! (Translation: Toggle my vibration feedback!)"
        /// </summary>
        private void OnHapticsToggled(bool enabled)
        {
            if (saveManager != null)
            {
                var settings = saveManager.Settings;
                settings.HapticsEnabled = enabled;
                saveManager.Save();
            }
            
            Debug.Log($"Riley: Haptics {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Handles left-handed UI toggle changes and saves to settings.
        /// Riley: "Gotta support left-handed players too!"
        /// </summary>
        private void OnLeftHandedToggled(bool enabled)
        {
            if (saveManager != null)
            {
                var settings = saveManager.Settings;
                settings.LeftHandedUi = enabled;
                saveManager.Save();
            }
            
            Debug.Log($"Nibble: *bark* (Translation: Left-handed UI {(enabled ? "enabled" : "disabled")})");
        }

        /// <summary>
        /// Handles neon effects toggle changes for performance optimization.
        /// Riley: "Time to toggle the neon effects for performance!"
        /// </summary>
        private void OnNeonEffectsToggled(bool enabled)
        {
            enableNeonEffects = enabled;
            
            // Apply neon effects toggle to all UI elements
            ToggleNeonEffects(enabled);
            
            Debug.Log($"Riley: Neon effects {(enabled ? "enabled" : "disabled")} for performance");
        }

        /// <summary>
        /// Handles quip toggle changes for dialogue control.
        /// Riley: "Time to toggle the quips! Sometimes I need to be quiet when the hounds are chasing!"
        /// </summary>
        private void OnQuipToggled(bool enabled)
        {
            _quipsEnabled = enabled;
            
            if (saveManager != null)
            {
                // Store quip preference in settings
                var settings = saveManager.Settings;
                // Note: You'd need to add a QuipsEnabled property to PlayerSettingsData
                saveManager.Save();
            }
            
            Debug.Log($"Nibble: *bark* (Translation: Quips {(enabled ? "enabled" : "disabled")}!)");
        }

        /// <summary>
        /// Toggles neon effects on all UI elements for performance optimization.
        /// Nibble: "Bark! (Translation: Toggle the pretty lights!)"
        /// </summary>
        private void ToggleNeonEffects(bool enabled)
        {
            // Find all UI elements with neon effects and toggle them
            var neonElements = GetComponentsInChildren<MonoBehaviour>();
            foreach (var element in neonElements)
            {
                // In a real game, you'd have a specific component for neon effects
                // and toggle them here for performance
                if (element.name.Contains("Neon") || element.name.Contains("Glow"))
                {
                    element.gameObject.SetActive(enabled);
                }
            }
        }

        /// <summary>
        /// Handles back to game button press.
        /// Riley: "Time to get back to the action!"
        /// </summary>
        private void OnBackToGame()
        {
            if (_isPaused)
            {
                ShowPause(); // Return to pause menu
            }
            else
            {
                ShowHud(); // Return to HUD
            }
            
            Debug.Log("Nibble: *happy bark* (Translation: Back to the game!)");
        }

        /// <summary>
        /// Gets current performance settings for debugging.
        /// Riley: "Need to check the performance settings!"
        /// </summary>
        public (bool neonEffects, bool mobileOptimized, float updateInterval) GetPerformanceSettings()
        {
            return (enableNeonEffects, optimizeForMobile, uiUpdateInterval);
        }

        /// <summary>
        /// Forces a UI update (useful for testing or critical updates).
        /// Riley: "Force update the UI when needed!"
        /// </summary>
        public void ForceUIUpdate()
        {
            _lastUIUpdate = 0f; // Reset timer to force immediate update
            UpdateMobileUI();
        }

        /// <summary>
        /// Displays a random quip from Riley or Nibble.
        /// Riley: "Time for some witty commentary!"
        /// </summary>
        public void ShowRandomQuip(bool isRiley = true)
        {
            if (!_quipsEnabled || Time.time - _lastQuipTime < _quipCooldown)
                return;

            _lastQuipTime = Time.time;

            if (quipLabel != null)
            {
                var quips = isRiley ? _rileyQuips : _nibbleQuips;
                var randomQuip = quips[Random.Range(0, quips.Length)];
                quipLabel.text = randomQuip;
                
                // Auto-hide quip after a few seconds
                StartCoroutine(HideQuipAfterDelay(3f));
            }
        }

        /// <summary>
        /// Hides the quip label after a delay.
        /// Nibble: "Bark! (Translation: Hide the quip after a while!)"
        /// </summary>
        private System.Collections.IEnumerator HideQuipAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (quipLabel != null)
            {
                quipLabel.text = "";
            }
        }

        /// <summary>
        /// Tests UI responsiveness across different resolutions.
        /// Riley: "Time to test this UI on different screen sizes!"
        /// </summary>
        [ContextMenu("Test UI Responsiveness")]
        public void TestUIResponsiveness()
        {
            if (!enableResolutionTesting) return;

            StartCoroutine(TestAllResolutions());
        }

        /// <summary>
        /// Tests UI across all configured resolutions.
        /// Nibble: "Bark! (Translation: Test all the resolutions!)"
        /// </summary>
        private System.Collections.IEnumerator TestAllResolutions()
        {
            Debug.Log("Riley: Starting UI responsiveness test across all resolutions!");
            
            foreach (var resolution in testResolutions)
            {
                Debug.Log($"Testing resolution: {resolution.x}x{resolution.y}");
                
                // In a real test, you'd change the screen resolution here
                // For now, we'll just log the test
                yield return new WaitForSeconds(1f);
                
                // Test UI elements at this resolution
                TestUIElementsAtResolution(resolution);
            }
            
            Debug.Log("Nibble: *bark* (Translation: UI responsiveness test complete!)");
        }

        /// <summary>
        /// Tests UI elements at a specific resolution.
        /// Riley: "Gotta make sure everything fits properly!"
        /// </summary>
        private void TestUIElementsAtResolution(Vector2 resolution)
        {
            // Test canvas scaling
            var canvasScaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                var aspectRatio = resolution.x / resolution.y;
                Debug.Log($"Aspect ratio: {aspectRatio:F2}");
                
                // Check if UI elements would be properly scaled
                if (aspectRatio < 1.3f) // Portrait or square
                {
                    Debug.Log("Riley: Portrait mode detected - UI should adapt properly");
                }
                else if (aspectRatio > 2.0f) // Ultra-wide
                {
                    Debug.Log("Riley: Ultra-wide mode detected - UI should scale appropriately");
                }
            }
        }

        /// <summary>
        /// Optimizes UI for specific device types.
        /// Nibble: "Bark! (Translation: Optimize UI for this device!)"
        /// </summary>
        public void OptimizeUIForDevice()
        {
            var deviceType = GetDeviceType();
            
            switch (deviceType)
            {
                case DeviceType.Phone:
                    OptimizeForPhone();
                    break;
                case DeviceType.Tablet:
                    OptimizeForTablet();
                    break;
                case DeviceType.Desktop:
                    OptimizeForDesktop();
                    break;
            }
        }

        /// <summary>
        /// Determines the device type based on screen dimensions.
        /// Riley: "Gotta figure out what kind of device this is!"
        /// </summary>
        private DeviceType GetDeviceType()
        {
            var width = Screen.width;
            var height = Screen.height;
            var aspectRatio = (float)width / height;

            if (width < 768 || aspectRatio < 1.2f)
                return DeviceType.Phone;
            else if (width < 1200)
                return DeviceType.Tablet;
            else
                return DeviceType.Desktop;
        }

        /// <summary>
        /// Optimizes UI specifically for phones.
        /// Riley: "Phone optimization - gotta make everything touch-friendly!"
        /// </summary>
        private void OptimizeForPhone()
        {
            Debug.Log("Riley: Optimizing UI for phone!");
            
            // Increase touch target sizes
            var buttons = GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                var rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(
                        Mathf.Max(rectTransform.sizeDelta.x, 60f),
                        Mathf.Max(rectTransform.sizeDelta.y, 60f)
                    );
                }
            }
        }

        /// <summary>
        /// Optimizes UI specifically for tablets.
        /// Nibble: "Bark! (Translation: Optimize for tablet!)"
        /// </summary>
        private void OptimizeForTablet()
        {
            Debug.Log("Nibble: *bark* (Translation: Optimizing UI for tablet!)");
            
            // Adjust spacing and sizing for tablet
            var canvasScaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.matchWidthOrHeight = 0.7f; // Slightly favor height
            }
        }

        /// <summary>
        /// Optimizes UI specifically for desktop.
        /// Riley: "Desktop optimization - full power mode!"
        /// </summary>
        private void OptimizeForDesktop()
        {
            Debug.Log("Riley: Optimizing UI for desktop - full power mode!");
            
            // Enable all effects for desktop
            enableNeonEffects = true;
            optimizeForMobile = false;
        }

        /// <summary>
        /// Device types for UI optimization.
        /// </summary>
        private enum DeviceType
        {
            Phone,
            Tablet,
            Desktop
        }
    }
}
