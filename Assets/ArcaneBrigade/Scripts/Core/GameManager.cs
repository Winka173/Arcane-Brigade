using System;
using UnityEngine;
using ArcaneBrigade.Data;

namespace ArcaneBrigade.Core
{
    /// <summary>
    /// Persistent singleton holding the active <see cref="RunContext"/> and current
    /// <see cref="GameState"/>. Bootstraps on first scene load.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Boot")]
        [Tooltip("Default champion used when no run has started yet (MVP: Aldric).")]
        [SerializeField] private ChampionDefinition defaultChampion;

        public RunContext Run { get; } = new RunContext();
        public GameState State { get; private set; } = GameState.Booting;

        /// <summary>Fired whenever the game state transitions.</summary>
        public event Action<GameState, GameState> StateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (defaultChampion != null)
            {
                Run.Reset(defaultChampion);
            }
        }

        public void SetState(GameState next)
        {
            if (next == State) return;
            var previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
        }
    }
}
