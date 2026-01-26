namespace ArcaneBrigade
{
    /// <summary>Presentation-only edge observer. Never mutates simulation state or its RNG.</summary>
    public sealed class ToyboxAudioObserver
    {
        private CombatSimulation observed;
        private MidwaveSpectacle spectacle;
        private bool started, ended, ready;
        private int rewards;

        public static float ChargeRequired(HeroMechanic hero) => hero switch
        {
            HeroMechanic.LanternStride => 12,
            HeroMechanic.ParcelExpress or HeroMechanic.GlassGambler => 6,
            HeroMechanic.DreamBreath or HeroMechanic.MirrorGuard => 2,
            HeroMechanic.TideCollector => 8,
            HeroMechanic.ClockRunner or HeroMechanic.TreasureTax => 10,
            _ => float.PositiveInfinity
        };

        public void Observe(CombatSimulation simulation)
        {
            if (simulation == null || simulation.Phase != RunPhase.Combat) return;
            if (observed != simulation)
            {
                observed = simulation; spectacle = null; started = ended = ready = false; rewards = 0;
            }
            bool charged = simulation.Champion.Alive && simulation.HeroCooldownRemaining <= 0 &&
                simulation.HeroCharge >= ChargeRequired(simulation.Champion.Definition.HeroMechanic);
            if (charged && !ready) CheckpointAudio.PlayInterfaceId("hero.charge.ready");
            ready = charged;
            MidwaveSpectacle next = simulation.Spectacle;
            if (next == null) return;
            if (spectacle != next)
            {
                spectacle = next; started = ended = false; rewards = next.RewardDrops;
                CheckpointAudio.PlayWorldId("event.warn", next.Position);
            }
            float age = next.Age(simulation.Elapsed);
            if (!started && age >= 2)
            {
                started = true;
                CheckpointAudio.PlayWorldId(next.Kind == SpectacleKind.PaperParade ? "event.parade.start" :
                    next.Kind == SpectacleKind.GildedTrap ? "event.trap.start" : "event.eclipse.start", next.Position);
            }
            if (next.RewardDrops > rewards) CheckpointAudio.PlayWorldId("event.reward", next.Position);
            rewards = next.RewardDrops;
            if (!ended && age >= MidwaveSpectacle.Duration)
            {
                ended = true; CheckpointAudio.PlayWorldId("event.end", next.Position);
            }
        }
    }
}
