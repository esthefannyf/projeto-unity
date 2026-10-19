using System;
using UnityEngine;

namespace CrystalRuins.Core
{
    public enum TileKind
    {
        Floor,
        Wall,
        Spikes,
        Door,
        Exit
    }

    public enum EntityKind
    {
        Diamond,
        Key,
        Clock,
        Boulder,
        Patroller,
        Chaser
    }

    [Flags]
    public enum TurnEvents
    {
        None = 0,
        Moved = 1 << 0,
        Blocked = 1 << 1,
        DiamondTaken = 1 << 2,
        KeyTaken = 1 << 3,
        ClockTaken = 1 << 4,
        BoulderPushed = 1 << 5,
        SpikesFilled = 1 << 6,
        DoorOpened = 1 << 7,
        DoorLocked = 1 << 8,
        ExitLocked = 1 << 9,
        PlayerHit = 1 << 10,
        SpikesHit = 1 << 11,
        ExitReached = 1 << 12
    }

    public sealed class Entity
    {
        public readonly int Id;
        public EntityKind Kind;
        public Vector2Int Pos;
        public Vector2Int Dir;

        public Entity(int id, EntityKind kind, Vector2Int pos, Vector2Int dir = default)
        {
            Id = id;
            Kind = kind;
            Pos = pos;
            Dir = dir;
        }

        public bool IsEnemy => Kind == EntityKind.Patroller || Kind == EntityKind.Chaser;

        public Entity Clone() => (Entity)MemberwiseClone();
    }
}

