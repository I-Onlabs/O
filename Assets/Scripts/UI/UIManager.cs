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
    /// Uses CanvasGroup fading to keep draw calls friendly for mobile builds.
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

        [Header("HUD Widgets")]
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Slider rileyHealthBar;
        [SerializeField] private Slider nibbleHealthBar;
        [SerializeField] private Text quipLabel;

        [Header("Systems")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private AudioSource uiAudioSource;

        private Coroutine _fadeRoutine;
        private ScreenConfig _currentScreen;
        private int _highScore;
        private bool _isPaused;

        private void Awake()
        {
            if (saveManager != null)
            {
                saveManager.SaveLoaded += OnSaveLoaded;
            }

            GameEvents.GamePaused += HandleGamePaused;
            GameEvents.GameResumed += HandleGameResumed;
            GameEvents.GameOver += HandleGameOver;

            ShowScreen(mainMenu);
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
    }
}
