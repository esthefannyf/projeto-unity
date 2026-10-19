using UnityEngine;

namespace CrystalRuins.Core
{
    // Strategy: each enemy kind decides its next step; World applies it.
    public interface IEnemyBehavior
    {
        Vector2Int NextStep(World world, Entity enemy);
    }

    public sealed class PatrolBehavior : IEnemyBehavior
    {
        public Vector2Int NextStep(World world, Entity enemy)
        {
            Vector2Int dir = enemy.Dir == Vector2Int.zero ? Vector2Int.right : enemy.Dir;

            if (world.CanEnemyEnter(enemy.Pos + dir)) return dir;
            if (world.CanEnemyEnter(enemy.Pos - dir)) return -dir;
            return Vector2Int.zero;
        }
    }

    public sealed class ChaseBehavior : IEnemyBehavior
    {
        const int SightRange = 6;

        static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        public Vector2Int NextStep(World world, Entity enemy)
        {
            // Slower than the player: acts every other turn.
            if (world.Turn % 2 != 0) return Vector2Int.zero;

            int current = Distance(enemy.Pos, world.Player);
            if (current > SightRange) return Vector2Int.zero;

            Vector2Int best = Vector2Int.zero;
            int bestDistance = current;
            foreach (var dir in Directions)
            {
                var next = enemy.Pos + dir;
                if (!world.CanEnemyEnter(next)) continue;

                int d = Distance(next, world.Player);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = dir;
                }
            }
            return best;
        }

        static int Distance(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public static class EnemyBehaviors
    {
        static readonly IEnemyBehavior Patrol = new PatrolBehavior();
        static readonly IEnemyBehavior Chase = new ChaseBehavior();

        public static IEnemyBehavior For(EntityKind kind) =>
            kind == EntityKind.Chaser ? Chase : Patrol;
    }
}

