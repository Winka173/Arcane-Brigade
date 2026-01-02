namespace ArcaneBrigade.Core
{
    /// <summary>Top-level state machine for a run.</summary>
    public enum GameState
    {
        Booting = 0,
        WaveActive = 1,
        WaveCleared = 2,   // shop phase
        BossWave = 3,
        Victory = 4,
        GameOver = 5,
    }
}
