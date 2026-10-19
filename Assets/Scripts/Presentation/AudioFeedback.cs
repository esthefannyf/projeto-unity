using CrystalRuins.Core;
using UnityEngine;

namespace CrystalRuins.Presentation
{
    // Observer: turns session events into sound effects.
    public sealed class AudioFeedback : MonoBehaviour
    {
        [SerializeField] AudioSource effects;
        [SerializeField] AudioClip step;
        [SerializeField] AudioClip push;
        [SerializeField] AudioClip diamond;
        [SerializeField] AudioClip key;
        [SerializeField] AudioClip door;
        [SerializeField] AudioClip clock;
        [SerializeField] AudioClip spikes;
        [SerializeField] AudioClip hurt;
        [SerializeField] AudioClip blocked;
        [SerializeField] AudioClip undo;
        [SerializeField] AudioClip win;
        [SerializeField] AudioClip lose;

        GameSession session;

        public void Bind(GameSession gameSession)
        {
            session = gameSession;
            session.TurnPlayed += OnTurn;
            session.Undone += () => Play(undo);
            session.LevelCompleted += () => Play(win);
            session.Finished += won => Play(won ? win : lose);
        }

        void OnTurn(TurnEvents e)
        {
            if (Has(e, TurnEvents.SpikesHit)) Play(spikes);
            else if (Has(e, TurnEvents.PlayerHit)) Play(hurt);
            else if (Has(e, TurnEvents.DiamondTaken)) Play(diamond);
            else if (Has(e, TurnEvents.KeyTaken)) Play(key);
            else if (Has(e, TurnEvents.ClockTaken)) Play(clock);
            else if (Has(e, TurnEvents.DoorOpened)) Play(door);
            else if (Has(e, TurnEvents.BoulderPushed)) Play(push);
            else if (Has(e, TurnEvents.Blocked)) Play(blocked);
            else if (Has(e, TurnEvents.Moved)) Play(step);
        }

        static bool Has(TurnEvents value, TurnEvents flag) => (value & flag) != 0;

        void Play(AudioClip clip)
        {
            if (clip != null) effects.PlayOneShot(clip);
        }
    }
}
