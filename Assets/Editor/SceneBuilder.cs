using CrystalRuins.Presentation;
using CrystalRuins.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace CrystalRuins.EditorTools
{
    // Menu: Tools > Build Scenes. Recreates the Menu and Game scenes from the assets.
    public static class SceneBuilder
    {
        const string Art = "Assets/Art/";
        const string Audio = "Assets/Audio/";
        const string MenuPath = "Assets/Scenes/Menu.unity";
        const string GamePath = "Assets/Scenes/Game.unity";

        static readonly Color Ink = new Color(0.07f, 0.07f, 0.12f, 1f);
        static readonly Color Gold = new Color(0.95f, 0.76f, 0.3f);
        static readonly Color ButtonColor = new Color(0.2f, 0.2f, 0.34f, 1f);

        [MenuItem("Tools/Build Scenes")]
        public static void Build()
        {
            ConfigureSprites();
            BuildGame();
            BuildMenu();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuPath, true),
                new EditorBuildSettingsScene(GamePath, true),
            };
            PlayerSettings.productName = "Diamond Rush";
            EditorSceneManager.OpenScene(MenuPath);
        }

        static void ConfigureSprites()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" }))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // ------------------------------------------------------------ game scene

        static void BuildGame()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = CreateCamera(5f);
            CreateGlobalLight();
            CreateEventSystem();

            var audio = new GameObject("Audio");
            AddMusic(audio);
            var effects = audio.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            var feedback = audio.AddComponent<AudioFeedback>();
            Assign(feedback, ("effects", effects),
                ("step", Clip("passo")), ("push", Clip("empurrar")), ("diamond", Clip("diamante")),
                ("key", Clip("chave")), ("door", Clip("porta")), ("clock", Clip("relogio")),
                ("spikes", Clip("espinhos")), ("hurt", Clip("dano")), ("blocked", Clip("clique")),
                ("undo", Clip("desfazer")), ("win", Clip("vitoria")), ("lose", Clip("derrota")));

            var world = new GameObject("World").AddComponent<WorldView>();
            Assign(world, ("cam", cam),
                ("floor", Sprite("chao")), ("wall", Sprite("parede")), ("spikes", Sprite("espinhos")),
                ("door", Sprite("porta")), ("exitClosed", Sprite("saida_fechada")), ("exitOpen", Sprite("saida_aberta")),
                ("player", Sprite("explorador")), ("diamond", Sprite("diamante")), ("key", Sprite("chave")),
                ("clock", Sprite("relogio")), ("boulder", Sprite("pedra")),
                ("patroller", Sprite("cobra")), ("chaser", Sprite("mumia")));

            var canvas = CreateCanvas();
            var hud = BuildHud(canvas);
            var overlay = BuildOverlay(canvas);

            var controller = new GameObject("GameController").AddComponent<GameController>();
            var levels = new[]
            {
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Levels/level1.txt"),
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Levels/level2.txt"),
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Levels/level3.txt"),
            };
            var so = new SerializedObject(controller);
            var list = so.FindProperty("levelFiles");
            list.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            Assign(controller, ("view", world), ("hud", hud), ("overlay", overlay), ("audioFeedback", feedback));

            overlay.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), GamePath);
        }

        static Hud BuildHud(Transform canvas)
        {
            var bar = Panel(canvas, "Hud", new Color(0, 0, 0, 0.6f));
            Anchor(bar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 90));
            var layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 0, 0);
            layout.spacing = 40;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;

            var level = HudText(bar, "Fase", 420, Color.white);
            var diamonds = HudText(bar, "Diamantes", 330, new Color(0.45f, 0.85f, 1f));
            var keys = HudText(bar, "Chaves", 230, Gold);
            var timer = HudText(bar, "Tempo", 150, Color.white);
            var score = HudText(bar, "Pontos", 260, Color.white);

            var hearts = new Image[GameSessionLives];
            for (int i = 0; i < hearts.Length; i++)
            {
                var img = Panel(bar, "Vida " + (i + 1), Color.white).GetComponent<Image>();
                img.sprite = Sprite("coracao");
                img.preserveAspect = true;
                var le = img.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 56;
                le.preferredHeight = 56;
                hearts[i] = img;
            }

            var hud = bar.gameObject.AddComponent<Hud>();
            var so = new SerializedObject(hud);
            so.FindProperty("levelName").objectReferenceValue = level;
            so.FindProperty("diamonds").objectReferenceValue = diamonds;
            so.FindProperty("keys").objectReferenceValue = keys;
            so.FindProperty("timer").objectReferenceValue = timer;
            so.FindProperty("score").objectReferenceValue = score;
            var arr = so.FindProperty("hearts");
            arr.arraySize = hearts.Length;
            for (int i = 0; i < hearts.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        const int GameSessionLives = 3;

        static Text HudText(Transform parent, string name, float width, Color color)
        {
            var t = Label(parent, name, name, 34, TextAnchor.MiddleLeft, color);
            var le = t.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            return t;
        }

        static OverlayPanel BuildOverlay(Transform canvas)
        {
            var panel = Panel(canvas, "Overlay", new Color(0, 0, 0, 0.82f));
            Stretch(panel);

            var title = Label(panel, "Titulo", "", 84, TextAnchor.MiddleCenter, Gold);
            Anchor(title.rectTransform, Center, Center, Center, new Vector2(0, 300), new Vector2(1500, 130));
            var body = Label(panel, "Texto", "", 38, TextAnchor.MiddleCenter, Color.white);
            Anchor(body.rectTransform, Center, Center, Center, new Vector2(0, 70), new Vector2(1500, 380));
            var primary = Button(panel, "Principal", new Vector2(0, -190));
            var secondary = Button(panel, "Secundario", new Vector2(0, -310));

            var overlay = panel.gameObject.AddComponent<OverlayPanel>();
            Assign(overlay, ("title", title), ("body", body), ("primary", primary), ("secondary", secondary));
            return overlay;
        }

        // ------------------------------------------------------------ menu scene

        static void BuildMenu()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(5.6f);
            CreateGlobalLight();
            CreateEventSystem();
            AddMusic(new GameObject("Audio"));

            // Decorative tile backdrop.
            for (int y = -6; y <= 6; y++)
                for (int x = -11; x <= 11; x++)
                    Decor("Tile", Sprite(y == 6 || y == -6 ? "parede" : "chao"), new Vector3(x, y, 0), 1f, -10);
            Decor("Explorador", Sprite("explorador"), new Vector3(0, 1.3f, 0), 4f, 0);
            Decor("Diamante", Sprite("diamante"), new Vector3(-5.5f, 1.2f, 0), 2.5f, 0);
            Decor("Diamante", Sprite("diamante"), new Vector3(6f, -0.4f, 0), 2f, 0);
            Decor("Pedra", Sprite("pedra"), new Vector3(5.5f, 2.6f, 0), 3f, 0);
            Decor("Mumia", Sprite("mumia"), new Vector3(-6f, -1.8f, 0), 3f, 0);

            var canvas = CreateCanvas();
            var title = Label(canvas, "Titulo", "DIAMOND RUSH", 110, TextAnchor.MiddleCenter, Gold);
            Anchor(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(1700, 160));
            title.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(5, -5);

            var best = Label(canvas, "Recorde", "", 38, TextAnchor.MiddleCenter, Color.white);
            Anchor(best.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -210), new Vector2(900, 60));

            var play = Button(canvas, "Jogar", new Vector2(0, -200));
            var help = Button(canvas, "Como jogar", new Vector2(0, -315));
            var quit = Button(canvas, "Sair", new Vector2(0, -430));
            foreach (var b in new[] { play, help, quit })
                b.GetComponentInChildren<Text>().text = b.name;

            var overlay = BuildOverlay(canvas);
            overlay.gameObject.SetActive(false);

            var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
            Assign(menu, ("playButton", play), ("helpButton", help), ("quitButton", quit),
                ("bestScore", best), ("overlay", overlay));

            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), MenuPath);
        }

        // ------------------------------------------------------------ helpers

        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        static Camera CreateCamera(float size)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0, 0, -10);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = size;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Ink;
            go.AddComponent<AudioListener>();
            return cam;
        }

        static void CreateGlobalLight()
        {
            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
        }

        static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        static void AddMusic(GameObject host)
        {
            var music = host.AddComponent<AudioSource>();
            music.clip = Clip("musica");
            music.loop = true;
            music.volume = 0.4f;
            music.playOnAwake = true;
        }

        static Transform CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go.transform;
        }

        static void Decor(string name, Sprite sprite, Vector3 pos, float scale, int order)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            if (order < 0) sr.color = new Color(0.55f, 0.55f, 0.65f);
        }

        static RectTransform Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        static Text Label(Transform parent, string name, string text, int size, TextAnchor align, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.fontStyle = FontStyle.Bold;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static Button Button(Transform parent, string name, Vector2 pos)
        {
            var rt = Panel(parent, name, Color.white);
            Anchor(rt, Center, Center, Center, pos, new Vector2(520, 95));
            var button = rt.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
            colors.selectedColor = new Color(0.55f, 0.42f, 0.12f);
            colors.pressedColor = Gold;
            button.colors = colors;

            var label = Label(rt, "Texto", name, 44, TextAnchor.MiddleCenter, Color.white);
            Stretch(label.rectTransform);
            return button;
        }

        static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");

        static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + name + ".wav");

        static void Assign(Object target, params (string field, Object value)[] values)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in values)
            {
                var prop = so.FindProperty(field);
                if (prop == null) Debug.LogError($"Missing field '{field}' on {target.GetType().Name}");
                else prop.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
