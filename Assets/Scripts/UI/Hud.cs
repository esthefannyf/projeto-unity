using CrystalRuins.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalRuins.UI
{
    public sealed class Hud : MonoBehaviour
    {
        [SerializeField] Text levelName;
        [SerializeField] Text diamonds;
        [SerializeField] Text keys;
        [SerializeField] Text timer;
        [SerializeField] Text score;
        [SerializeField] Image[] hearts;

        GameSession session;

        public void Bind(GameSession gameSession) => session = gameSession;

        void Update()
        {
            if (session?.World == null) return;

            var w = session.World;
            levelName.text = $"{session.LevelIndex + 1}. {session.Level.Name}";
            diamonds.text = $"Diamantes {w.DiamondsTaken}/{w.DiamondsTotal}";
            keys.text = $"Chaves {w.Keys}";
            score.text = $"Pontos {session.Score}";

            timer.text = FormatTime(session.TimeLeft);
            timer.color = session.TimeLeft <= 10f ? new Color(1f, 0.4f, 0.4f) : Color.white;

            for (int i = 0; i < hearts.Length; i++)
                hearts[i].enabled = i < session.Lives;
        }

        public static string FormatTime(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
