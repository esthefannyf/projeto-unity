namespace CrystalRuins.Presentation
{
    // State pattern: what the controller does each frame depends on the current state.
    public interface IGameState
    {
        void Enter();
        void Tick();
        void Exit();
    }

    public sealed class PlayingState : IGameState
    {
        readonly GameController game;

        public PlayingState(GameController game) => this.game = game;

        public void Enter() => game.HideOverlay();

        public void Tick()
        {
            var session = game.Session;
            var input = game.Input;

            if (input.Pause())
            {
                game.SetState(new PausedState(game));
                return;
            }

            session.Tick(UnityEngine.Time.deltaTime);

            if (input.Undo()) session.Undo();
            else if (input.Restart()) session.RestartLevel();
            else
            {
                var dir = input.PollMove();
                if (dir != UnityEngine.Vector2Int.zero) session.Move(dir);
            }
        }

        public void Exit() { }
    }

    public sealed class PausedState : IGameState
    {
        readonly GameController game;

        public PausedState(GameController game) => this.game = game;

        public void Enter() => game.ShowPause();

        public void Tick()
        {
            if (game.Input.Pause()) game.SetState(new PlayingState(game));
        }

        public void Exit() { }
    }

    // Level-complete / game-over screens: the overlay buttons drive what happens next.
    public sealed class MessageState : IGameState
    {
        public MessageState(GameController game) { }

        public void Enter() { }
        public void Tick() { }
        public void Exit() { }
    }
}
