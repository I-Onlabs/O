using System;
using System.Collections.Generic;

namespace AngryDogs.Core
{
    public enum GameState
    {
        Boot,
        MainMenu,
        Gameplay,
        Paused,
        GameOver
    }

    /// <summary>
    /// Lightweight finite state machine for top-level game flow.
    /// </summary>
    public sealed class GameStateMachine
    {
        private readonly Dictionary<GameState, Action> _enterHandlers = new();
        private readonly Dictionary<GameState, Action> _exitHandlers = new();

        public GameState CurrentState { get; private set; } = GameState.Boot;

        public void Register(GameState state, Action onEnter, Action onExit)
        {
            _enterHandlers[state] = onEnter;
            _exitHandlers[state] = onExit;
        }

        public void ChangeState(GameState nextState)
        {
            if (nextState == CurrentState)
            {
                return;
            }

            if (_exitHandlers.TryGetValue(CurrentState, out var exit))
            {
                exit?.Invoke();
            }

            CurrentState = nextState;

            if (_enterHandlers.TryGetValue(CurrentState, out var enter))
            {
                enter?.Invoke();
            }
        }
    }
}
