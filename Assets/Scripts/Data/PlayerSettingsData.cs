using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngryDogs.Data
{
    /// <summary>
    /// Stores player configurable options such as key bindings and audio levels.
    /// Designed to be lightweight for JSON serialization.
    /// </summary>
    [Serializable]
    public sealed class PlayerSettingsData
    {
        [Serializable]
        public struct KeyBinding
        {
            public string actionId;
            public KeyCode key;
        }

        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.75f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.85f;
        [SerializeField] private bool hapticsEnabled = true;
        [SerializeField] private bool leftHandedUi;
        [SerializeField] private List<KeyBinding> keyBindings = new();

        private Dictionary<string, KeyCode> _bindingLookup;

        public float MusicVolume
        {
            get => musicVolume;
            set => musicVolume = Mathf.Clamp01(value);
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set => sfxVolume = Mathf.Clamp01(value);
        }

        public bool HapticsEnabled
        {
            get => hapticsEnabled;
            set => hapticsEnabled = value;
        }

        public bool LeftHandedUi
        {
            get => leftHandedUi;
            set => leftHandedUi = value;
        }

        public IReadOnlyList<KeyBinding> Bindings => keyBindings;

        public static PlayerSettingsData CreateDefault()
        {
            return new PlayerSettingsData
            {
                musicVolume = 0.75f,
                sfxVolume = 0.85f,
                hapticsEnabled = true,
                leftHandedUi = false,
                keyBindings = new List<KeyBinding>()
            };
        }

        public void SetBinding(string actionId, KeyCode key)
        {
            if (string.IsNullOrEmpty(actionId))
            {
                return;
            }

            EnsureLookup();
            _bindingLookup[actionId] = key;

            var found = false;
            for (var i = 0; i < keyBindings.Count; i++)
            {
                if (keyBindings[i].actionId == actionId)
                {
                    keyBindings[i] = new KeyBinding { actionId = actionId, key = key };
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                keyBindings.Add(new KeyBinding { actionId = actionId, key = key });
            }
        }

        public KeyCode GetBinding(string actionId, KeyCode fallback)
        {
            if (string.IsNullOrEmpty(actionId))
            {
                return fallback;
            }

            EnsureLookup();
            return _bindingLookup.TryGetValue(actionId, out var key) ? key : fallback;
        }

        private void EnsureLookup()
        {
            _bindingLookup ??= new Dictionary<string, KeyCode>(StringComparer.Ordinal);
<<<<<<< HEAD

=======
>>>>>>> origin/codex/refactor-prototype-code-for-maintainability-78tl00
            if (_bindingLookup.Count == keyBindings.Count)
            {
                return;
            }

            _bindingLookup.Clear();
            foreach (var binding in keyBindings)
            {
                if (!string.IsNullOrEmpty(binding.actionId) && !_bindingLookup.ContainsKey(binding.actionId))
                {
                    _bindingLookup.Add(binding.actionId, binding.key);
                }
            }
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/codex/refactor-prototype-code-for-maintainability-78tl00
