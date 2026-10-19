using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalRuins.Core
{
    // Rules of a run: levels, lives, countdown, score and undo history.
    public sealed class GameSession
    {
        public const int StartingLives = 3;
        public const float ClockBonus = 10f;
        const float EnemyInterval = 0.3f;

        readonly IReadOnlyList<Level> levels;
        readonly Stack<WorldSnapshot> history = new Stack<WorldSnapshot>();
        WorldSnapshot levelStart;
        int levelStartScore;
        float enemyTimer;

        public World World { get; private set; }
        public int LevelIndex { get; private set; }
        public int Lives { get; private set; }
        public int Score { get; private set; }
        public float TimeLeft { get; private set; }
        public float TotalTime { get; private set; }
        public bool Running { get; private set; }

        public Level Level => levels[LevelIndex];
        public int LevelCount => levels.Count;
        public bool IsLastLevel => LevelIndex == levels.Count - 1;

        public event Action LevelStarted;
        public event Action<TurnEvents> TurnPlayed;
        public event Action Undone;
        public event Action LevelCompleted;
        public event Action<bool> Finished;

        public GameSession(IReadOnlyList<Level> levels)
        {
            this.levels = levels;
        }

        public void Begin()
        {
            Lives = StartingLives;
            Score = 0;
            TotalTime = 0f;
            StartLevel(0);
        }

        public void NextLevel() => StartLevel(LevelIndex + 1);

        public void Tick(float dt)
        {
            if (!Running) return;

            TimeLeft -= dt;
            TotalTime += dt;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                LoseLife();
                return;
            }

            enemyTimer += dt;
            while (enemyTimer >= EnemyInterval)
            {
                enemyTimer -= EnemyInterval;
                var events = World.AdvanceEnemies();
                if ((events & TurnEvents.PlayerHit) != 0)
                {
                    TurnPlayed?.Invoke(events);
                    LoseLife();
                    return;
                }
            }
        }

        public void Move(Vector2Int dir)
        {
            if (!Running || dir == Vector2Int.zero) return;

            var before = World.Capture();
            var events = World.Step(dir);

            if ((events & TurnEvents.Blocked) == 0)
                history.Push(before);

            if ((events & TurnEvents.DiamondTaken) != 0) Score += 100;
            if ((events & TurnEvents.ClockTaken) != 0) TimeLeft += ClockBonus;

            TurnPlayed?.Invoke(events);

            if ((events & TurnEvents.PlayerHit) != 0) LoseLife();
            else if ((events & TurnEvents.ExitReached) != 0) Complete();
        }

        public void Undo()
        {
            if (!Running || history.Count == 0) return;

            World.Restore(history.Pop());
            Undone?.Invoke();
        }

        // Always available, so a stuck boulder can never end the run.
        public void RestartLevel()
        {
            if (!Running) return;
            ResetLevel();
        }

        void StartLevel(int index)
        {
            LevelIndex = index;
            World = new World(Level);
            levelStart = World.Capture();
            levelStartScore = Score;
            history.Clear();
            enemyTimer = 0f;
            TimeLeft = Level.TimeLimit;
            Running = true;
            LevelStarted?.Invoke();
        }

        void ResetLevel()
        {
            World.Restore(levelStart);
            Score = levelStartScore;
            history.Clear();
            enemyTimer = 0f;
            TimeLeft = Level.TimeLimit;
        }

        void LoseLife()
        {
            Lives--;
            if (Lives <= 0)
            {
                Running = false;
                Finished?.Invoke(false);
                return;
            }
            ResetLevel();
        }

        void Complete()
        {
            Running = false;
            Score += 500 + Mathf.CeilToInt(TimeLeft) * 5;

            if (IsLastLevel) Finished?.Invoke(true);
            else LevelCompleted?.Invoke();
        }
    }
}
