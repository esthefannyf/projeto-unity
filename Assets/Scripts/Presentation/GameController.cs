using System.Collections.Generic;
using CrystalRuins.Core;
using CrystalRuins.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalRuins.Presentation
{
    public sealed class GameController : MonoBehaviour
    {
        public const string BestScoreKey = "best_score";

        [SerializeField] TextAsset[] levelFiles;
        [SerializeField] WorldView view;
        [SerializeField] Hud hud;
        [SerializeField] OverlayPanel overlay;
        [SerializeField] AudioFeedback audioFeedback;
        [SerializeField] string menuScene = "Menu";

        public GameSession Session { get; private set; }
        public GridInput Input { get; } = new GridInput();

        IGameState state;

        void Start()
        {
            var levels = new List<Level>();
            foreach (var file in levelFiles) levels.Add(Level.Parse(file.text));

            Session = new GameSession(levels);
            Session.LevelStarted += () => view.Build(Session.World);
            Session.LevelCompleted += OnLevelCompleted;
            Session.Finished += OnFinished;

            hud.Bind(Session);
            audioFeedback.Bind(Session);

            Session.Begin();
            SetState(new PlayingState(this));
        }

        void Update() => state?.Tick();

        public void SetState(IGameState next)
        {
            state?.Exit();
            state = next;
            state.Enter();
        }

        public void ShowPause()
        {
            overlay.Show("Pausado", "Z desfaz a ultima jogada, R reinicia a fase.",
                "Continuar", () => SetState(new PlayingState(this)),
                "Menu", GoToMenu);
        }

        public void HideOverlay() => overlay.Hide();

        public void GoToMenu() => SceneManager.LoadScene(menuScene);

        void OnLevelCompleted()
        {
            SetState(new MessageState(this));
            overlay.Show("Fase completa!",
                $"Tempo restante: {Hud.FormatTime(Session.TimeLeft)}\nPontos: {Session.Score}",
                "Proxima fase", () =>
                {
                    Session.NextLevel();
                    SetState(new PlayingState(this));
                });
        }

        void OnFinished(bool won)
        {
            SetState(new MessageState(this));

            if (won)
            {
                int best = PlayerPrefs.GetInt(BestScoreKey, 0);
                string record = Session.Score > best ? "\nNovo recorde!" : $"\nRecorde: {best}";
                if (Session.Score > best) PlayerPrefs.SetInt(BestScoreKey, Session.Score);

                overlay.Show("Voce escapou!",
                    $"Tempo total: {Hud.FormatTime(Session.TotalTime)}\nPontos: {Session.Score}{record}",
                    "Jogar de novo", Restart, "Menu", GoToMenu);
            }
            else
            {
                overlay.Show("Fim de jogo",
                    $"Tempo total: {Hud.FormatTime(Session.TotalTime)}\nPontos: {Session.Score}",
                    "Tentar de novo", Restart, "Menu", GoToMenu);
            }
        }

        void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
