using System.Collections.Generic;
using UnityEngine;

namespace CrystalRuins.Core
{
    // Memento: everything needed to put a World back exactly as it was.
    public sealed class WorldSnapshot
    {
        internal readonly TileKind[] Tiles;
        internal readonly List<Entity> Entities;
        internal readonly Vector2Int Player;
        internal readonly int Keys;
        internal readonly int DiamondsTaken;
        internal readonly int Turn;

        internal WorldSnapshot(TileKind[] terrain, List<Entity> entities, Vector2Int player,
                               int keys, int diamondsTaken, int turn)
        {
            Tiles = (TileKind[])terrain.Clone();
            Entities = new List<Entity>(entities.Count);
            foreach (var e in entities) Entities.Add(e.Clone());
            Player = player;
            Keys = keys;
            DiamondsTaken = diamondsTaken;
            Turn = turn;
        }
    }
}
