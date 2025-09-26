using UnityEngine;
using UnityEngine.UI;
using AngryDogs.Core;

namespace AngryDogs.UI
{
    /// <summary>
    /// Listens to gameplay events and updates HUD widgets.
    /// </summary>
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Slider rileyHealthBar;
        [SerializeField] private Slider nibbleHealthBar;
        [SerializeField] private Text houndCountText;

        private void OnEnable()
        {
            GameEvents.ScoreChanged += HandleScoreChanged;
            GameEvents.RileyHealthChanged += HandleRileyHealthChanged;
            GameEvents.NibbleHealthChanged += HandleNibbleHealthChanged;
            GameEvents.HoundPackCountChanged += HandleHoundCountChanged;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= HandleScoreChanged;
            GameEvents.RileyHealthChanged -= HandleRileyHealthChanged;
            GameEvents.NibbleHealthChanged -= HandleNibbleHealthChanged;
            GameEvents.HoundPackCountChanged -= HandleHoundCountChanged;
        }

        private void HandleScoreChanged(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString("N0");
            }
        }

        private void HandleRileyHealthChanged(float value)
        {
            if (rileyHealthBar != null)
            {
                rileyHealthBar.value = value;
            }
        }

        private void HandleNibbleHealthChanged(float value)
        {
            if (nibbleHealthBar != null)
            {
                nibbleHealthBar.value = value;
            }
        }

        private void HandleHoundCountChanged(int count)
        {
            if (houndCountText != null)
            {
                houndCountText.text = $"Hounds: {count}";
            }
        }
    }
}
