using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ArcadeMusic : MonoBehaviour
    {
        private AudioSource source;
        private AudioClip combat;
        private AudioClip shop;
        private float target = 0.13f;
        private AudioSource ensemble;
        private AudioSource bossLayer;
        private AudioSource shopLayer;
        private float ensembleTarget;
        private float bossTarget;
        private float shopTarget = 0.13f;
        private bool licensed;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0f;
            combat = Resources.Load<AudioClip>("Audio/NightShift_Base");
            AudioClip recorded = Resources.Load<AudioClip>("Audio/Licensed/FusionJazz");
            licensed = recorded != null;
            if (licensed) combat = recorded;
            shop = Resources.Load<AudioClip>("Audio/ShopAfterHours");
            source.clip = combat;
            ensemble = Layer("Audio/NightShift_Ensemble");
            bossLayer = Layer("Audio/NightShift_Boss");
            shopLayer = Layer("Audio/ShopAfterHours");
            if (licensed) { shopLayer.clip = recorded; shopLayer.pitch = 0.9f; }
            double start = AudioSettings.dspTime + 0.1;
            foreach (AudioSource layer in new[] { source, ensemble, bossLayer, shopLayer }) if (layer.clip != null) layer.PlayScheduled(start);
            target = 0;
        }

        private AudioSource Layer(string resource)
        {
            AudioSource layer = gameObject.AddComponent<AudioSource>(); layer.playOnAwake = false;
            layer.loop = true; layer.volume = 0; layer.priority = 210; layer.clip = Resources.Load<AudioClip>(resource);
            return layer;
        }

        public void SetPhase(SessionPhase phase, bool paused, bool boss)
        {
            bool battle = phase == SessionPhase.Combat;
            float pauseGain = paused ? 0.28f : 1;
            target = battle ? (licensed ? 0.16f : 0.22f) * pauseGain : 0;
            ensembleTarget = !licensed && battle ? 0.1f * pauseGain : 0;
            bossTarget = !licensed && battle && boss ? 0.15f * pauseGain : 0;
            source.pitch = licensed && boss ? 1.035f : 1;
            shopTarget = battle ? 0 : 0.18f * pauseGain;
        }

        private void Update()
        {
            float gain = PresentationSettings.MusicVolume * PresentationSettings.MusicDuck;
            source.volume = Mathf.MoveTowards(source.volume, target * gain, Time.unscaledDeltaTime * 0.5f);
            ensemble.volume = Mathf.MoveTowards(ensemble.volume, ensembleTarget * gain, Time.unscaledDeltaTime * 0.4f);
            bossLayer.volume = Mathf.MoveTowards(bossLayer.volume, bossTarget * gain, Time.unscaledDeltaTime * 0.4f);
            shopLayer.volume = Mathf.MoveTowards(shopLayer.volume, shopTarget * gain, Time.unscaledDeltaTime * 0.5f);
        }
    }
}
