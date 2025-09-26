using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using AngryDogs.SaveSystem;

namespace AngryDogs.UI
{
    /// <summary>
    /// Localization manager for Riley's quips and Nibble's barks with neon-themed text support.
    /// Riley: "Time to make this game speak multiple languages! These hounds don't habla español?"
    /// Nibble: "Bark! (Translation: ¡Kibble por favor!)"
    /// </summary>
    public sealed class LocalizationManager : MonoBehaviour
    {
        [System.Serializable]
        public class LocalizedQuip
        {
            public string key;
            public string englishText;
            public string spanishText;
            public string japaneseText;
            public QuipType quipType;
            public bool isNeonThemed;
        }

        public enum QuipType
        {
            RileyQuip,
            NibbleBark,
            UIElement,
            Achievement,
            Error
        }

        [System.Serializable]
        public class LanguageOption
        {
            public string languageCode;
            public string displayName;
            public Sprite flagIcon;
            public bool isRTL; // Right-to-left text support
        }

        [Header("Localization Settings")]
        [SerializeField] private bool enableLocalization = true;
        [SerializeField] private string defaultLanguage = "en";
        [SerializeField] private LanguageOption[] supportedLanguages = {
            new LanguageOption { languageCode = "en", displayName = "English", isRTL = false },
            new LanguageOption { languageCode = "es", displayName = "Español", isRTL = false },
            new LanguageOption { languageCode = "ja", displayName = "日本語", isRTL = false }
        };

        [Header("Riley's Quips")]
        [SerializeField] private LocalizedQuip[] rileyQuips = {
            new LocalizedQuip {
                key = "riley_boss_mech",
                englishText = "This chihuahua's mech is overcompensating!",
                spanishText = "¡El mech de este chihuahua está compensando demasiado!",
                japaneseText = "このチワワのメックは過度に補償している！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_weak_spots",
                englishText = "Nibble, we need to find the weak spots!",
                spanishText = "¡Nibble, necesitamos encontrar los puntos débiles!",
                japaneseText = "ニブル、弱点を見つける必要がある！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_tiny_tyrant",
                englishText = "That tiny tyrant hits harder than expected!",
                spanishText = "¡Ese tirano diminuto golpea más fuerte de lo esperado!",
                japaneseText = "その小さな暴君は予想以上に強く打つ！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_show_who_boss",
                englishText = "Time to show this chihuahua who's boss!",
                spanishText = "¡Hora de mostrarle a este chihuahua quién manda!",
                japaneseText = "このチワワに誰がボスかを見せてやる時だ！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_mech_damage",
                englishText = "The mech-suit is taking damage!",
                spanishText = "¡El traje mech está recibiendo daño!",
                japaneseText = "メックスーツがダメージを受けている！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_weak_point_down",
                englishText = "One weak point down! Keep hitting the others!",
                spanishText = "¡Un punto débil abajo! ¡Sigue golpeando los otros!",
                japaneseText = "弱点を一つ破壊！他のも続けて攻撃！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_treat_tantrum",
                englishText = "The chihuahua is having a treat tantrum!",
                spanishText = "¡El chihuahua está teniendo una rabieta por golosinas!",
                japaneseText = "チワワがおやつの癇癪を起こしている！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_overclocked_yap",
                englishText = "Overclocked yap mode? This is going to be chaos!",
                spanishText = "¿Modo de ladrido overclockeado? ¡Esto va a ser un caos!",
                japaneseText = "オーバークロック吠えモード？これは大混乱になる！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_good_idea_nibble",
                englishText = "Good idea, Nibble! Fetch that bone!",
                spanishText = "¡Buena idea, Nibble! ¡Ve por ese hueso!",
                japaneseText = "いいアイデアだ、ニブル！その骨を取ってこい！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "riley_distracted_chance",
                englishText = "The chihuahua is distracted! Now's our chance!",
                spanishText = "¡El chihuahua está distraído! ¡Ahora es nuestra oportunidad!",
                japaneseText = "チワワが気を取られている！今がチャンスだ！",
                quipType = QuipType.RileyQuip,
                isNeonThemed = true
            }
        };

        [Header("Nibble's Barks")]
        [SerializeField] private LocalizedQuip[] nibbleBarks = {
            new LocalizedQuip {
                key = "nibble_bigger_chihuahua",
                englishText = "Bark! (Translation: Even I'm bigger than that chihuahua!)",
                spanishText = "¡Guau! (Traducción: ¡Incluso yo soy más grande que ese chihuahua!)",
                japaneseText = "ワン！(翻訳: 僕だってそのチワワより大きいよ！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_found_weak_points",
                englishText = "Bark! (Translation: Found weak points on the mech-suit!)",
                spanishText = "¡Guau! (Traducción: ¡Encontré puntos débiles en el traje mech!)",
                japaneseText = "ワン！(翻訳: メックスーツの弱点を見つけた！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_chihuahua_angry",
                englishText = "Bark! (Translation: The chihuahua looks really angry!)",
                spanishText = "¡Guau! (Traducción: ¡El chihuahua se ve muy enojado!)",
                japaneseText = "ワン！(翻訳: チワワがすごく怒ってる！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_close_enough_attack",
                englishText = "Bark! (Translation: Is the chihuahua close enough to attack?)",
                spanishText = "¡Guau! (Traducción: ¿El chihuahua está lo suficientemente cerca para atacar?)",
                japaneseText = "ワン！(翻訳: チワワは攻撃するのに十分近い？)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_yapping_loud",
                englishText = "Bark! (Translation: The chihuahua is yapping really loud!)",
                spanishText = "¡Guau! (Traducción: ¡El chihuahua está ladrando muy fuerte!)",
                japaneseText = "ワン！(翻訳: チワワがすごく大きな声で吠えてる！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_fetch_bone",
                englishText = "Bark! (Translation: I can fetch a bone to distract the chihuahua!)",
                spanishText = "¡Guau! (Traducción: ¡Puedo traer un hueso para distraer al chihuahua!)",
                japaneseText = "ワン！(翻訳: チワワを気を取らせるために骨を取ってこられる！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_distracted_bone",
                englishText = "Bark! (Translation: The chihuahua is distracted by the bone!)",
                spanishText = "¡Guau! (Traducción: ¡El chihuahua está distraído por el hueso!)",
                japaneseText = "ワン！(翻訳: チワワが骨に気を取られている！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_back_neon_grind",
                englishText = "Bark! (Translation: Back to the neon grind!)",
                spanishText = "¡Guau! (Traducción: ¡De vuelta al trabajo neón!)",
                japaneseText = "ワン！(翻訳: ネオンの作業に戻ろう！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_victory_regular_dog",
                englishText = "Bark! (Translation: We won! The chihuahua is just a regular dog now!)",
                spanishText = "¡Guau! (Traducción: ¡Ganamos! ¡El chihuahua ahora es solo un perro normal!)",
                japaneseText = "ワン！(翻訳: 勝った！チワワは今は普通の犬だ！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "nibble_kibble_please",
                englishText = "Bark! (Translation: Kibble por favor!)",
                spanishText = "¡Guau! (Traducción: ¡Kibble por favor!)",
                japaneseText = "ワン！(翻訳: キブルお願い！)",
                quipType = QuipType.NibbleBark,
                isNeonThemed = true
            }
        };

        [Header("UI Elements")]
        [SerializeField] private LocalizedQuip[] uiElements = {
            new LocalizedQuip {
                key = "ui_high_score",
                englishText = "High Score",
                spanishText = "Puntuación Máxima",
                japaneseText = "ハイスコア",
                quipType = QuipType.UIElement,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "ui_currency",
                englishText = "KibbleCoins",
                spanishText = "Monedas Kibble",
                japaneseText = "キブルコイン",
                quipType = QuipType.UIElement,
                isNeonThemed = true
            },
            new LocalizedQuip {
                key = "ui_settings",
                englishText = "Settings",
                spanishText = "Configuración",
                japaneseText = "設定",
                quipType = QuipType.UIElement,
                isNeonThemed = false
            },
            new LocalizedQuip {
                key = "ui_language",
                englishText = "Language",
                spanishText = "Idioma",
                japaneseText = "言語",
                quipType = QuipType.UIElement,
                isNeonThemed = false
            }
        };

        [Header("Systems")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private UIManager uiManager;

        private Dictionary<string, LocalizedQuip> _quipDatabase;
        private string _currentLanguage;
        private bool _isInitialized;

        // Events
        public System.Action<string> OnLanguageChanged;
        public System.Action<LocalizedQuip> OnQuipRequested;

        private void Awake()
        {
            _quipDatabase = new Dictionary<string, LocalizedQuip>();
            _currentLanguage = defaultLanguage;
        }

        private void Start()
        {
            if (enableLocalization)
            {
                InitializeLocalization();
            }
        }

        /// <summary>
        /// Initializes the localization system.
        /// Riley: "Initialize the localization system!"
        /// </summary>
        private void InitializeLocalization()
        {
            try
            {
                // Build quip database
                BuildQuipDatabase();
                
                // Load saved language preference
                LoadLanguagePreference();
                
                // Set up Unity Localization
                SetupUnityLocalization();
                
                _isInitialized = true;
                
                Debug.Log("Riley: Localization system initialized!");
                Debug.Log("Nibble: *bark* (Translation: Localization ready!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to initialize localization: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds the quip database from all localized quips.
        /// Nibble: "Bark! (Translation: Build the quip database!)"
        /// </summary>
        private void BuildQuipDatabase()
        {
            _quipDatabase.Clear();
            
            // Add Riley's quips
            foreach (var quip in rileyQuips)
            {
                _quipDatabase[quip.key] = quip;
            }
            
            // Add Nibble's barks
            foreach (var bark in nibbleBarks)
            {
                _quipDatabase[bark.key] = bark;
            }
            
            // Add UI elements
            foreach (var uiElement in uiElements)
            {
                _quipDatabase[uiElement.key] = uiElement;
            }
            
            Debug.Log($"Riley: Quip database built with {_quipDatabase.Count} entries!");
        }

        /// <summary>
        /// Sets up Unity Localization system.
        /// Riley: "Set up Unity Localization!"
        /// </summary>
        private void SetupUnityLocalization()
        {
            // Initialize Unity Localization
            if (LocalizationSettings.SelectedLocale == null)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            }
            
            // Set up locale change handler
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        /// <summary>
        /// Loads language preference from save data.
        /// Nibble: "Bark! (Translation: Load language preference!)"
        /// </summary>
        private void LoadLanguagePreference()
        {
            if (saveManager != null)
            {
                // Check if language preference is stored in settings
                // For now, we'll use the default language
                _currentLanguage = defaultLanguage;
                
                Debug.Log($"Riley: Language preference loaded: {_currentLanguage}");
            }
        }

        /// <summary>
        /// Gets a localized quip by key.
        /// Riley: "Get a localized quip!"
        /// </summary>
        public string GetLocalizedQuip(string key)
        {
            if (!_isInitialized || !_quipDatabase.ContainsKey(key))
            {
                Debug.LogWarning($"Riley: Quip key '{key}' not found in database!");
                return key; // Return key as fallback
            }

            var quip = _quipDatabase[key];
            var localizedText = GetLocalizedText(quip);
            
            OnQuipRequested?.Invoke(quip);
            
            return localizedText;
        }

        /// <summary>
        /// Gets localized text for a quip based on current language.
        /// Nibble: "Bark! (Translation: Get localized text!)"
        /// </summary>
        private string GetLocalizedText(LocalizedQuip quip)
        {
            switch (_currentLanguage)
            {
                case "es":
                    return quip.spanishText;
                case "ja":
                    return quip.japaneseText;
                case "en":
                default:
                    return quip.englishText;
            }
        }

        /// <summary>
        /// Changes the current language.
        /// Riley: "Change the language!"
        /// </summary>
        public void ChangeLanguage(string languageCode)
        {
            if (!IsLanguageSupported(languageCode))
            {
                Debug.LogWarning($"Riley: Language '{languageCode}' is not supported!");
                return;
            }

            _currentLanguage = languageCode;
            
            // Update Unity Localization
            var locale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
            }
            
            // Save language preference
            SaveLanguagePreference();
            
            OnLanguageChanged?.Invoke(languageCode);
            
            Debug.Log($"Riley: Language changed to {languageCode}!");
            Debug.Log($"Nibble: *bark* (Translation: Language changed!)");
        }

        /// <summary>
        /// Checks if a language is supported.
        /// Riley: "Check if language is supported!"
        /// </summary>
        public bool IsLanguageSupported(string languageCode)
        {
            foreach (var language in supportedLanguages)
            {
                if (language.languageCode == languageCode)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all supported languages.
        /// Nibble: "Bark! (Translation: Get supported languages!)"
        /// </summary>
        public LanguageOption[] GetSupportedLanguages()
        {
            return supportedLanguages;
        }

        /// <summary>
        /// Gets the current language.
        /// Riley: "Get current language!"
        /// </summary>
        public string GetCurrentLanguage()
        {
            return _currentLanguage;
        }

        /// <summary>
        /// Saves language preference to save data.
        /// Riley: "Save language preference!"
        /// </summary>
        private void SaveLanguagePreference()
        {
            if (saveManager != null)
            {
                // Store language preference in settings
                // You would need to add a LanguagePreference property to PlayerSettingsData
                Debug.Log($"Riley: Language preference saved: {_currentLanguage}");
            }
        }

        /// <summary>
        /// Handles Unity Localization locale changes.
        /// Nibble: "Bark! (Translation: Handle locale changes!)"
        /// </summary>
        private void OnLocaleChanged(Locale newLocale)
        {
            if (newLocale != null)
            {
                _currentLanguage = newLocale.Identifier.Code;
                OnLanguageChanged?.Invoke(_currentLanguage);
                
                Debug.Log($"Riley: Unity Localization locale changed to {_currentLanguage}!");
            }
        }

        /// <summary>
        /// Gets a random Riley quip in the current language.
        /// Riley: "Get a random quip!"
        /// </summary>
        public string GetRandomRileyQuip()
        {
            var rileyQuipKeys = new List<string>();
            foreach (var quip in rileyQuips)
            {
                rileyQuipKeys.Add(quip.key);
            }
            
            if (rileyQuipKeys.Count > 0)
            {
                var randomKey = rileyQuipKeys[UnityEngine.Random.Range(0, rileyQuipKeys.Count)];
                return GetLocalizedQuip(randomKey);
            }
            
            return "Riley: Default quip!";
        }

        /// <summary>
        /// Gets a random Nibble bark in the current language.
        /// Nibble: "Bark! (Translation: Get a random bark!)"
        /// </summary>
        public string GetRandomNibbleBark()
        {
            var nibbleBarkKeys = new List<string>();
            foreach (var bark in nibbleBarks)
            {
                nibbleBarkKeys.Add(bark.key);
            }
            
            if (nibbleBarkKeys.Count > 0)
            {
                var randomKey = nibbleBarkKeys[UnityEngine.Random.Range(0, nibbleBarkKeys.Count)];
                return GetLocalizedQuip(randomKey);
            }
            
            return "Nibble: *bark* (Translation: Default bark!)";
        }

        /// <summary>
        /// Gets a UI element text in the current language.
        /// Riley: "Get UI element text!"
        /// </summary>
        public string GetUIElementText(string key)
        {
            return GetLocalizedQuip(key);
        }

        /// <summary>
        /// Checks if a quip is neon-themed.
        /// Nibble: "Bark! (Translation: Check if quip is neon-themed!)"
        /// </summary>
        public bool IsQuipNeonThemed(string key)
        {
            if (_quipDatabase.ContainsKey(key))
            {
                return _quipDatabase[key].isNeonThemed;
            }
            return false;
        }

        /// <summary>
        /// Gets quip type for styling purposes.
        /// Riley: "Get quip type for styling!"
        /// </summary>
        public QuipType GetQuipType(string key)
        {
            if (_quipDatabase.ContainsKey(key))
            {
                return _quipDatabase[key].quipType;
            }
            return QuipType.RileyQuip;
        }

        /// <summary>
        /// Formats text for neon display with proper RTL support.
        /// Riley: "Format text for neon display!"
        /// </summary>
        public string FormatTextForDisplay(string text, bool isNeonThemed = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Get current language RTL setting
            var currentLanguage = GetCurrentLanguage();
            var isRTL = false;
            
            foreach (var language in supportedLanguages)
            {
                if (language.languageCode == currentLanguage)
                {
                    isRTL = language.isRTL;
                    break;
                }
            }

            // Apply RTL formatting if needed
            if (isRTL)
            {
                // In a real implementation, you'd apply RTL text formatting here
                text = $"<RTL>{text}</RTL>";
            }

            // Apply neon theming if needed
            if (isNeonThemed)
            {
                text = $"<NEON>{text}</NEON>";
            }

            return text;
        }

        /// <summary>
        /// Gets localization statistics.
        /// Riley: "Get localization stats!"
        /// </summary>
        public string GetLocalizationStats()
        {
            var stats = $"Localization Statistics:\n";
            stats += $"Current Language: {_currentLanguage}\n";
            stats += $"Supported Languages: {supportedLanguages.Length}\n";
            stats += $"Total Quips: {_quipDatabase.Count}\n";
            stats += $"Riley Quips: {rileyQuips.Length}\n";
            stats += $"Nibble Barks: {nibbleBarks.Length}\n";
            stats += $"UI Elements: {uiElements.Length}\n";
            stats += $"Initialized: {_isInitialized}\n";
            
            return stats;
        }

        private void OnDestroy()
        {
            if (LocalizationSettings.SelectedLocaleChanged != null)
            {
                LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            }
        }
    }
}