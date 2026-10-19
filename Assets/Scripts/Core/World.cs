using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalRuins.Core
{
    // Pure grid logic (no MonoBehaviour): one Step() per player action.
    public sealed class World
    {
        readonly TileKind[] terrain;
        readonly List<Entity> entities = new List<Entity>();
        int nextId;

        public int Width { get; }
        public int Height { get; }
        public int DiamondsTotal { get; }
        public Vector2Int Player { get; private set; }
        public int Keys { get; private set; }
        public int DiamondsTaken { get; private set; }
        public int Turn { get; private set; }

        public IReadOnlyList<Entity> Entities => entities;
        public bool ExitOpen => DiamondsTaken >= DiamondsTotal;

        public World(Level level)
        {
            Height = level.Rows.Length;
            Width = level.Rows[0].Length;
            terrain = new TileKind[Width * Height];

            int diamonds = 0;
            for (int row = 0; row < Height; row++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var p = new Vector2Int(x, Height - 1 - row);
                    char c = level.Rows[row][x];
                    terrain[Index(p)] = TerrainFor(c);

                    switch (c)
                    {
                        case 'P': Player = p; break;
                        case 'D': Add(EntityKind.Diamond, p); diamonds++; break;
                        case 'K': Add(EntityKind.Key, p); break;
                        case 'T': Add(EntityKind.Clock, p); break;
                        case 'B': Add(EntityKind.Boulder, p); break;
                        case 'M': Add(EntityKind.Patroller, p, Vector2Int.right); break;
                        case 'V': Add(EntityKind.Patroller, p, Vector2Int.down); break;
                        case 'C': Add(EntityKind.Chaser, p); break;
                    }
                }
            }
            DiamondsTotal = diamonds;
        }

        static TileKind TerrainFor(char c)
        {
            switch (c)
            {
                case '#': return TileKind.Wall;
                case 'S': return TileKind.Spikes;
                case 'L': return TileKind.Door;
                case 'E': return TileKind.Exit;
                default: return TileKind.Floor;
            }
        }

        public TileKind TerrainAt(Vector2Int p) => InBounds(p) ? terrain[Index(p)] : TileKind.Wall;

        public Entity EntityAt(Vector2Int p)
        {
            foreach (var e in entities)
                if (e.Pos == p) return e;
            return null;
        }

        public bool CanEnemyEnter(Vector2Int p) =>
            TerrainAt(p) == TileKind.Floor && EntityAt(p) == null;

        public TurnEvents Step(Vector2Int dir)
        {
            var events = TurnEvents.None;

            if (dir != Vector2Int.zero && !TryMovePlayer(dir, ref events))
                return events | TurnEvents.Blocked;

            return events;
        }

        // Enemies act on their own clock, whether or not the player moves.
        public TurnEvents AdvanceEnemies()
        {
            var events = TurnEvents.None;
            Turn++;
            MoveEnemies(ref events);
            return events;
        }

        public WorldSnapshot Capture() =>
            new WorldSnapshot(terrain, entities, Player, Keys, DiamondsTaken, Turn);

        public void Restore(WorldSnapshot s)
        {
            Array.Copy(s.Tiles, terrain, terrain.Length);
            entities.Clear();
            foreach (var e in s.Entities) entities.Add(e.Clone());
            Player = s.Player;
            Keys = s.Keys;
            DiamondsTaken = s.DiamondsTaken;
            Turn = s.Turn;
        }

        bool TryMovePlayer(Vector2Int dir, ref TurnEvents events)
        {
            var target = Player + dir;

            switch (TerrainAt(target))
            {
                case TileKind.Wall:
                    return false;
                case TileKind.Door:
                    if (Keys == 0) { events |= TurnEvents.DoorLocked; return false; }
                    Keys--;
                    terrain[Index(target)] = TileKind.Floor;
                    events |= TurnEvents.DoorOpened;
                    break;
                case TileKind.Exit:
                    if (!ExitOpen) { events |= TurnEvents.ExitLocked; return false; }
                    break;
            }

            var occupant = EntityAt(target);
            if (occupant != null)
            {
                switch (occupant.Kind)
                {
                    case EntityKind.Boulder:
                        if (!TryPush(occupant, dir, ref events)) return false;
                        break;
                    case EntityKind.Diamond:
                        DiamondsTaken++;
                        entities.Remove(occupant);
                        events |= TurnEvents.DiamondTaken;
                        break;
                    case EntityKind.Key:
                        Keys++;
                        entities.Remove(occupant);
                        events |= TurnEvents.KeyTaken;
                        break;
                    case EntityKind.Clock:
                        entities.Remove(occupant);
                        events |= TurnEvents.ClockTaken;
                        break;
                    default:
                        events |= TurnEvents.PlayerHit;
                        break;
                }
            }

            Player = target;
            events |= TurnEvents.Moved;

            var landed = TerrainAt(target);
            if (landed == TileKind.Spikes) events |= TurnEvents.PlayerHit | TurnEvents.SpikesHit;
            if (landed == TileKind.Exit) events |= TurnEvents.ExitReached;
            return true;
        }

        // A boulder rolls onto floor or fills a spike pit (and disappears).
        bool TryPush(Entity boulder, Vector2Int dir, ref TurnEvents events)
        {
            var dest = boulder.Pos + dir;
            var t = TerrainAt(dest);
            if (t != TileKind.Floor && t != TileKind.Spikes) return false;
            if (EntityAt(dest) != null) return false;

            if (t == TileKind.Spikes)
            {
                terrain[Index(dest)] = TileKind.Floor;
                entities.Remove(boulder);
                events |= TurnEvents.SpikesFilled;
            }
            else
            {
                boulder.Pos = dest;
            }
            events |= TurnEvents.BoulderPushed;
            return true;
        }

        void MoveEnemies(ref TurnEvents events)
        {
            foreach (var enemy in entities)
            {
                if (!enemy.IsEnemy) continue;

                var step = EnemyBehaviors.For(enemy.Kind).NextStep(this, enemy);
                if (step == Vector2Int.zero) continue;

                var dest = enemy.Pos + step;
                if (dest == Player)
                {
                    events |= TurnEvents.PlayerHit;
                    continue;
                }
                if (!CanEnemyEnter(dest)) continue;

                enemy.Pos = dest;
                enemy.Dir = step;
            }
        }

        void Add(EntityKind kind, Vector2Int pos, Vector2Int dir = default) =>
            entities.Add(new Entity(nextId++, kind, pos, dir));

        bool InBounds(Vector2Int p) => p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;

        int Index(Vector2Int p) => p.y * Width + p.x;
    }
}
