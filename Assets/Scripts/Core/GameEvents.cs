using System;

namespace AngryDogs.Core
{
    /// <summary>
    /// Central hub for gameplay events. Keeps MonoBehaviours loosely coupled.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<int> ScoreChanged;
        public static event Action<float> RileyHealthChanged;
        public static event Action<float> NibbleHealthChanged;
        public static event Action<int> HoundPackCountChanged;
        public static event Action GamePaused;
        public static event Action GameResumed;
        public static event Action GameOver;

        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
        public static void RaiseRileyHealthChanged(float value) => RileyHealthChanged?.Invoke(value);
        public static void RaiseNibbleHealthChanged(float value) => NibbleHealthChanged?.Invoke(value);
        public static void RaiseHoundPackCountChanged(int count) => HoundPackCountChanged?.Invoke(count);
        public static void RaiseGamePaused() => GamePaused?.Invoke();
        public static void RaiseGameResumed() => GameResumed?.Invoke();
        public static void RaiseGameOver() => GameOver?.Invoke();
    }
}
