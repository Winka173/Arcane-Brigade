using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ArcadeMusic : MonoBehaviour
    {
        private AudioSource source;
        private AudioClip combat;
        private AudioClip shop;
        private float target = 0.13f;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0f;
            combat = Resources.Load<AudioClip>("Audio/CombatStudy");
            shop = Resources.Load<AudioClip>("Audio/ShopStudy");
            source.clip = shop;
            if (source.clip != null) source.Play();
        }

        public void SetPhase(SessionPhase phase, bool paused, bool boss)
        {
            AudioClip desired = phase == SessionPhase.Combat ? combat : shop;
            if (source.clip != desired) { source.clip = desired; if (desired != null) source.Play(); }
            target = paused ? 0.035f : phase == SessionPhase.Combat ? (boss ? 0.16f : 0.13f) : 0.11f;
        }

        private void Update() { source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * 0.2f); }
    }
}
