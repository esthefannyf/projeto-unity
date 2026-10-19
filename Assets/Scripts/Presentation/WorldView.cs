using System.Collections.Generic;
using CrystalRuins.Core;
using UnityEngine;

namespace CrystalRuins.Presentation
{
    public sealed class WorldView : MonoBehaviour
    {
        [Header("TileKind")]
        [SerializeField] Sprite floor;
        [SerializeField] Sprite wall;
        [SerializeField] Sprite spikes;
        [SerializeField] Sprite door;
        [SerializeField] Sprite exitClosed;
        [SerializeField] Sprite exitOpen;

        [Header("Entities")]
        [SerializeField] Sprite player;
        [SerializeField] Sprite diamond;
        [SerializeField] Sprite key;
        [SerializeField] Sprite clock;
        [SerializeField] Sprite boulder;
        [SerializeField] Sprite patroller;
        [SerializeField] Sprite chaser;

        [SerializeField] Camera cam;

        const float HudMargin = 1.6f;

        World world;
        SpriteRenderer[] tiles;
        SpriteRenderer playerRenderer;
        EntityView playerView;
        readonly Dictionary<int, EntityView> views = new Dictionary<int, EntityView>();
        readonly List<int> stale = new List<int>();

        public void Build(World newWorld)
        {
            world = newWorld;
            foreach (Transform child in transform) Destroy(child.gameObject);
            views.Clear();

            tiles = new SpriteRenderer[world.Width * world.Height];
            for (int y = 0; y < world.Height; y++)
                for (int x = 0; x < world.Width; x++)
                    tiles[y * world.Width + x] = CreateRenderer("Tile", new Vector2Int(x, y), 0);

            var go = CreateRenderer("Player", world.Player, 3);
            go.sprite = player;
            playerRenderer = go;
            playerView = go.gameObject.AddComponent<EntityView>();
            playerView.SnapTo(world.Player);

            FitCamera();
        }

        void Update()
        {
            if (world == null) return;

            for (int y = 0; y < world.Height; y++)
                for (int x = 0; x < world.Width; x++)
                    tiles[y * world.Width + x].sprite = SpriteFor(world.TerrainAt(new Vector2Int(x, y)));

            playerView.MoveTo(world.Player);
            SyncEntities();
        }

        void SyncEntities()
        {
            stale.Clear();
            foreach (var id in views.Keys) stale.Add(id);

            foreach (var e in world.Entities)
            {
                stale.Remove(e.Id);
                if (!views.TryGetValue(e.Id, out var view))
                {
                    var sr = CreateRenderer(e.Kind.ToString(), e.Pos, e.Kind == EntityKind.Boulder || e.IsEnemy ? 2 : 1);
                    sr.sprite = SpriteFor(e.Kind);
                    view = sr.gameObject.AddComponent<EntityView>();
                    view.SnapTo(e.Pos);
                    views.Add(e.Id, view);
                }
                view.MoveTo(e.Pos);
            }

            foreach (var id in stale)
            {
                Destroy(views[id].gameObject);
                views.Remove(id);
            }
        }

        SpriteRenderer CreateRenderer(string objectName, Vector2Int cell, int order)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(cell.x, cell.y, 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return sr;
        }

        Sprite SpriteFor(TileKind t)
        {
            switch (t)
            {
                case TileKind.Wall: return wall;
                case TileKind.Spikes: return spikes;
                case TileKind.Door: return door;
                case TileKind.Exit: return world.ExitOpen ? exitOpen : exitClosed;
                default: return floor;
            }
        }

        Sprite SpriteFor(EntityKind kind)
        {
            switch (kind)
            {
                case EntityKind.Diamond: return diamond;
                case EntityKind.Key: return key;
                case EntityKind.Clock: return clock;
                case EntityKind.Boulder: return boulder;
                case EntityKind.Patroller: return patroller;
                default: return chaser;
            }
        }

        void FitCamera()
        {
            float aspect = (float)Screen.width / Screen.height;
            float byHeight = (world.Height + HudMargin) / 2f;
            float byWidth = (world.Width + 1f) / (2f * aspect);
            cam.orthographicSize = Mathf.Max(byHeight, byWidth);
            cam.transform.position = new Vector3(
                (world.Width - 1) / 2f,
                (world.Height - 1) / 2f + HudMargin / 4f,
                -10f);
        }
    }
}
