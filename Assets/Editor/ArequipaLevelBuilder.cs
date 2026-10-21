using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

// Construye las escenas de nivel a partir de un layout en celdas (calles, río, campiña,
// manzanas) usando el tileset Pico-8 y los tiles de Arequipa. Las escenas resultantes
// se pueden seguir editando a mano con la Tile Palette.
public static class ArequipaLevelBuilder
{
    const string Gen = "Assets/Art/Generated/";
    const string Prefabs = "Assets/Prefabs/";

    [MenuItem("Arequipa/Construir Nivel 2 (Yanahuara)")]
    public static void BuildNivel2()
    {
        var m = new Map(62, 48, 2027);

        // Calles: avenida principal + callejones de 2 de ancho
        m.Mark('R', 0, 55, 10, 11);   // Camino a la campiña
        m.Mark('R', 0, 61, 22, 24);   // Av. Ejército → Puente Grau
        m.Mark('R', 0, 55, 36, 37);   // Calle Jerusalén
        m.Mark('R', 12, 13, 10, 47);
        m.Mark('R', 27, 29, 10, 47);  // Calle Misti
        m.Mark('R', 42, 43, 10, 47);
        m.Mark('A', 0, 55, 0, 9);     // Campiña con andenes
        m.Mark('W', 56, 61, 0, 47);   // Río Chili
        m.Mark('P', 56, 61, 22, 24);  // Puente Grau
        m.Begin("Nivel2");
        m.PaintBase();

        // Campiña: andenes con una casa de chacra solo alcanzable lanzando el pedido
        for (int c = 0; c <= 55; c++)
            for (int r = 0; r <= 9; r++)
                if (!(c >= 17 && c <= 23 && r >= 2 && r <= 6))
                    m.bldAqp.SetTile(m.Cell(c, r), m.G("tile_anden"));
        for (int c = 17; c <= 23; c++) for (int r = 2; r <= 6; r++) m.suelo.SetTile(m.Cell(c, r), m.P8(0));
        m.Building(18, 22, 3, 5, Map.RoofSets[1]);
        foreach (var p in new[] { (4, 3), (10, 6), (31, 2), (38, 7), (47, 4), (52, 8), (17, 2), (23, 6) }) m.decor.SetTile(m.Cell(p.Item1, p.Item2), m.P8(358));

        foreach (var b in m.Blocks)
        {
            if (b.Is(30, 12)) // Iglesia San Juan Bautista con atrio de sillar
            {
                m.FillInterior(b, m.sueloAqp, m.G("tile_sillar_piso"));
                m.Landmark("Iglesia_Yanahuara", "iglesia_yanahuara", m.P(35.5f, 16.5f), new Vector2(5.8f, 4.4f), Vector2.zero);
            }
            else if (b.Is(30, 25)) // Plaza de Yanahuara + mirador de arcos de sillar
            {
                m.FillInterior(b, m.sueloAqp, m.G("tile_adoquin"));
                for (int c = 32; c <= 39; c++)
                {
                    m.bldAqp.SetTile(m.Cell(c, 27), m.G("tile_techo_sillar"));
                    m.bldAqp.SetTile(m.Cell(c, 28), m.G("tile_portal_arco"));
                }
                foreach (var p in new[] { (31, 26), (40, 26), (31, 34), (40, 34), (33, 32), (38, 32) }) m.decor.SetTile(m.Cell(p.Item1, p.Item2), m.G("tile_palmera"));
                foreach (int c in new[] { 32, 33, 38, 39 }) m.bldAqp.SetTile(m.Cell(c, 33), m.G("tile_jardin_plaza"));
                var fu = m.Landmark("Pileta_Yanahuara", "fuente_plaza_grande", m.P(35.5f, 31f), Vector2.zero, Vector2.zero);
                fu.transform.localScale = Vector3.one * 0.7f;
                fu.AddComponent<CircleCollider2D>().radius = 1.4f;
            }
            else m.GenericBlock(b);
        }
        m.FinishTilemaps();

        var gLand = m.Ent("Landmarks");
        m.Combis(new[] { (m.P(8, 21.9f), 0f), (m.P(50, 24.7f), 180f), (m.P(22, 24.7f), 0f), (m.P(48, 21.9f), 180f) });
        m.Signs(new[] {
            ("Av. Ejército", m.P(20, 21)), ("Puente Grau", m.P(58.5f, 20.4f)), ("Calle Jerusalén", m.P(20, 38.4f)),
            ("Calle Misti", m.P(28, 44)), ("Mirador de Yanahuara", m.P(35.5f, 29.7f)), ("Campiña de Yanahuara", m.P(36, 4.5f)) });

        m.Gameplay(
            spawn: m.P(50, 36.5f),
            pedestrians: new[] { m.P(3, 12), m.P(20, 25), m.P(31, 30), m.P(44, 19), m.P(26, 40), m.P(50, 38), m.P(3, 35), m.P(41, 47) },
            dogs: new[] { m.P(28, 15), m.P(12.5f, 40), m.P(42.5f, 30), m.P(5, 23) },
            points: new[] {
                ("Punto_Picanteria", "Picantería La Lucila", "zona_picanteria", m.P(26, 30)),
                ("Punto_Mirador", "Mirador de Yanahuara", "zona_casa", m.P(35.5f, 25)),
                ("Punto_CasaParroquial", "Casa Parroquial", "zona_tienda", m.P(44, 16)),
                ("Punto_TiendaCampina", "Tienda de la Campiña", "zona_tienda", m.P(6, 12)) },
            route: ("Ruta_01_Ciclica", WaypointRouteGizmo.RouteMode.Ciclica, new[] {
                m.P(28, 23), m.P(35.5f, 23), m.P(42.5f, 23), m.P(42.5f, 36.5f), m.P(35.5f, 36.5f), m.P(28, 36.5f) }),
            launch: (m.P(20, 10.5f), m.P(20, 6), "Lanzamiento_01_Campina", "Objetivo_01_CasaChacra"));

        m.Finish(levelName: "Nivel 2 · Yanahuara", deliveries: 4, timeLimit: 35f, nextScene: "Nivel3");
    }

    [MenuItem("Arequipa/Construir Nivel 3 (San Camilo)")]
    public static void BuildNivel3()
    {
        var m = new Map(62, 48, 2028);

        m.Mark('R', 0, 61, 22, 24);   // Calle San Juan de Dios (avenida)
        m.Mark('R', 0, 61, 9, 10);    // Calle Zela
        m.Mark('R', 0, 46, 35, 36);   // Calle Piérola (termina en el mercado)
        foreach (int c in new[] { 10, 21, 33, 45 }) m.Mark('R', c, c + 1, 0, 47);
        m.Begin("Nivel3");
        m.PaintBase();

        foreach (var b in m.Blocks)
        {
            if (b.Is(23, 0)) // Iglesia y Plaza San Francisco
            {
                m.FillInterior(b, m.sueloAqp, m.G("tile_adoquin"));
                m.Landmark("Iglesia_SanFrancisco", "iglesia_san_francisco", m.P(27.5f, 2.8f), new Vector2(4.8f, 3.6f), Vector2.zero);
                foreach (var p in new[] { (24, 7), (31, 7) }) m.decor.SetTile(m.Cell(p.Item1, p.Item2), m.G("tile_palmera"));
            }
            else if (b.Is(35, 0)) // Fundo del Fierro: patio de artesanos
            {
                m.FillInterior(b, m.sueloAqp, m.G("tile_sillar_piso"));
                m.Puestos(new[] { m.P(37.5f, 2.5f), m.P(41.5f, 2.5f), m.P(37.5f, 6), m.P(41.5f, 6) });
            }
            else if (b.Is(47, 25)) // Mercado San Camilo con su explanada de puestos
            {
                m.FillInterior(b, m.sueloAqp, m.G("tile_sillar_piso"));
                var me = m.Landmark("Mercado_SanCamilo", "mercado_san_camilo", m.P(54, 30.5f), new Vector2(8f, 4.6f), new Vector2(0, -0.2f));
                me.transform.localScale = Vector3.one * 1.4f;
                var stalls = new List<Vector3>();
                for (int i = 0; i < 4; i++) { stalls.Add(m.P(49.5f + i * 3.2f, 38.5f)); stalls.Add(m.P(49.5f + i * 3.2f, 45)); }
                stalls.Add(m.P(49.5f, 41.8f)); stalls.Add(m.P(59.1f, 41.8f));
                m.Puestos(stalls.ToArray());
            }
            else m.GenericBlock(b);
        }
        // Puestos ambulantes en la avenida: dejan solo dos carriles libres
        m.Puestos(new[] { m.P(6, 21.9f), m.P(28, 24.7f), m.P(40, 21.9f), m.P(52, 24.7f) });
        m.FinishTilemaps();

        m.Combis(new[] { (m.P(15, 21.9f), 0f), (m.P(30, 21.9f), 180f), (m.P(48, 21.9f), 0f), (m.P(20, 24.7f), 0f), (m.P(36, 24.7f), 180f), (m.P(58, 24.7f), 0f) });
        m.Signs(new[] {
            ("Calle San Juan de Dios", m.P(25, 21)), ("Calle Zela", m.P(5, 11.6f)), ("Calle Piérola", m.P(28, 37.6f)),
            ("Calle Perú", m.P(46.5f, 42)), ("Plaza San Francisco", m.P(27.5f, 8)), ("Fundo del Fierro", m.P(39.5f, 4.3f)),
            ("Mercado San Camilo", m.P(56.5f, 25)) });

        m.Gameplay(
            spawn: m.P(2, 23),
            pedestrians: new[] { m.P(5, 11), m.P(16, 25), m.P(27.5f, 6), m.P(39, 11), m.P(48, 21), m.P(51, 36), m.P(5, 37), m.P(27, 47), m.P(39, 25), m.P(60, 11) },
            dogs: new[] { m.P(10.5f, 15), m.P(21.5f, 40), m.P(33.5f, 30), m.P(45.5f, 5), m.P(26, 35.5f), m.P(56, 23) },
            points: new[] {
                ("Punto_Mercado", "Mercado San Camilo", "zona_tienda", m.P(51, 25)),
                ("Punto_Picanteria", "Picantería Sol de Mayo", "zona_picanteria", m.P(20, 30)),
                ("Punto_FundoFierro", "Fundo del Fierro", "zona_tienda", m.P(39.5f, 8)),
                ("Punto_Botica", "Botica San Juan de Dios", "zona_hospital", m.P(9, 16)),
                ("Punto_Casona", "Casona Calle Piérola", "zona_casa", m.P(39.5f, 37)) },
            route: ("Ruta_01_IdaVuelta", WaypointRouteGizmo.RouteMode.IdaVuelta, new[] {
                m.P(3, 23), m.P(16, 23), m.P(27.5f, 23), m.P(39.5f, 23), m.P(51, 23), m.P(60, 23) }),
            launch: (m.P(45.5f, 41), m.P(54.4f, 41.8f), "Lanzamiento_01_CallePeru", "Objetivo_01_PuestosSanCamilo"));

        m.Finish(levelName: "Nivel 3 · Mercado San Camilo", deliveries: 5, timeLimit: 30f, nextScene: "");
    }

    // ─────────────────────────────────────────────────────────────────────

    class Block
    {
        public int c0, r0, c1, r1;
        public bool Is(int c, int r) => c0 == c && r0 == r;
    }

    class Map
    {
        // Techos y fachadas del tileset: TL,T1,T2,TR, L,C1,C2,R, BL,B1,B2,BR | fachada izq, puerta, medio, der
        public static readonly int[][] RoofSets = {
            new[]{106,107,108,109,130,131,132,133,154,155,156,157, 255,256,257,258},
            new[]{96,97,98,99,120,121,122,123,144,145,146,147, 240,241,242,243},
            new[]{101,102,103,104,125,126,127,128,149,150,151,152, 245,246,247,248},
        };
        static readonly int[] Trees = { 310, 358, 334, 359 };

        readonly int W, H;
        readonly char[,] T;
        readonly System.Random rng;
        readonly Dictionary<string, TileBase> cache = new Dictionary<string, TileBase>();
        public readonly List<Block> Blocks = new List<Block>();

        public Tilemap suelo, sueloAqp, rio, bld, bldAqp, decor;
        GameObject gMan, gEnt, gPlay, gCam, gUI;
        UnityEngine.SceneManagement.Scene scene;
        string sceneName;
        int obst, mini;
        GameObject player, managers;
        readonly List<Transform> points = new List<Transform>();

        public Map(int w, int h, int seed)
        {
            W = w; H = h; T = new char[w, h]; rng = new System.Random(seed);
            for (int c = 0; c < W; c++) for (int r = 0; r < H; r++) T[c, r] = 'B';
        }

        public TileBase P8(int i) => Load("Assets/Art/pico8_city_assets/tilemap_" + i + ".asset");
        public TileBase G(string n) => Load(Gen + n + ".asset");
        TileBase Load(string path)
        {
            if (!cache.TryGetValue(path, out var t)) cache[path] = t = AssetDatabase.LoadAssetAtPath<TileBase>(path);
            return t;
        }
        static Sprite Spr(string n) => AssetDatabase.LoadAssetAtPath<Sprite>(Gen + n + ".png");

        public Vector3Int Cell(int c, int r) => new Vector3Int(c - W / 2, H / 2 - 1 - r, 0);
        public Vector3 P(float c, float r) => new Vector3(c - W / 2 + 0.5f, H / 2 - 1 - r + 0.5f, 0);
        bool In(int c, int r) => c >= 0 && c < W && r >= 0 && r < H;
        bool Road(int c, int r) => !In(c, r) || T[c, r] == 'R' || T[c, r] == 'P';
        bool Water(int c, int r) => !In(c, r) || T[c, r] == 'W';

        public void Mark(char t, int c0, int c1, int r0, int r1)
        {
            for (int c = c0; c <= c1; c++) for (int r = r0; r <= r1; r++) T[c, r] = t;
        }

        public void Begin(string name)
        {
            sceneName = name;
            obst = LayerMask.NameToLayer("Obstaculo");
            mini = LayerMask.NameToLayer("Minimapa");
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            gMan = new GameObject("--- Managers ---");
            gEnt = new GameObject("--- Entorno ---");
            gPlay = new GameObject("--- Gameplay ---");
            gCam = new GameObject("--- Camara ---");
            gUI = new GameObject("--- UI ---");

            var grid = Child("Grid", gEnt); grid.AddComponent<Grid>();
            suelo = MakeMap(grid, "Suelo", 0, new Color(0.87f, 0.78f, 0.6f), false);
            sueloAqp = MakeMap(grid, "Suelo_Arequipa", 1, Color.white, false);
            rio = MakeMap(grid, "Rio_Chili", 1, new Color(0.92f, 0.96f, 1f), true);
            bld = MakeMap(grid, "Edificios", 2, new Color(1f, 0.96f, 0.88f), true);
            bldAqp = MakeMap(grid, "Edificios_Arequipa", 2, Color.white, true);
            decor = MakeMap(grid, "Decoracion", 3, Color.white, false);
        }

        static GameObject Child(string name, GameObject parent)
        {
            var g = new GameObject(name); g.transform.SetParent(parent.transform, false); return g;
        }
        public GameObject Ent(string name) => Child(name, gEnt);

        Tilemap MakeMap(GameObject grid, string name, int order, Color tint, bool collider)
        {
            var go = Child(name, grid);
            if (collider) go.layer = obst;
            var tm = go.AddComponent<Tilemap>(); tm.color = tint;
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
            if (collider)
            {
                go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                go.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
                go.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;
            }
            return tm;
        }

        // Calles (autotile 3x3 + esquinas interiores), pasos de cebra, río, puentes y manzanas.
        public void PaintBase()
        {
            var ids = new int[W, H];
            for (int c = 0; c < W; c++)
                for (int r = 0; r < H; r++)
                {
                    if (T[c, r] != 'R') continue;
                    bool u = Road(c, r - 1), d = Road(c, r + 1), l = Road(c - 1, r), rt = Road(c + 1, r);
                    int id;
                    if (!u && !l) id = 264; else if (!u && !rt) id = 266; else if (!d && !l) id = 312; else if (!d && !rt) id = 314;
                    else if (!u) id = 265; else if (!d) id = 313; else if (!l) id = 288; else if (!rt) id = 290;
                    else if (!Road(c - 1, r - 1)) id = 292; else if (!Road(c + 1, r - 1)) id = 291;
                    else if (!Road(c - 1, r + 1)) id = 268; else if (!Road(c + 1, r + 1)) id = 267;
                    else id = 289;
                    ids[c, r] = id;
                }
            for (int c = 0; c < W; c++)
                for (int r = 0; r < H; r++)
                {
                    int id = ids[c, r];
                    if (id == 0) continue;
                    // Paso de cebra en el tramo vertical que toca un cruce
                    if (id >= 288 && id <= 290 && (Junction(ids, c, r - 1) || Junction(ids, c, r + 1))) id += 48;
                    suelo.SetTile(Cell(c, r), P8(id));
                }

            for (int c = 0; c < W; c++)
                for (int r = 0; r < H; r++)
                {
                    if (T[c, r] == 'P')
                    {
                        sueloAqp.SetTile(Cell(c, r), G("tile_puente_sillar"));
                        if (Water(c, r - 1) && In(c, r - 1)) decor.SetTile(Cell(c, r), G("tile_puente_baranda"));
                        if (Water(c, r + 1) && In(c, r + 1))
                        {
                            decor.SetTile(Cell(c, r), G("tile_puente_baranda"));
                            decor.SetTransformMatrix(Cell(c, r), Matrix4x4.Scale(new Vector3(1, -1, 1)));
                        }
                    }
                    if (T[c, r] != 'W') continue;
                    bool top = !Water(c, r - 1), bot = !Water(c, r + 1), left = !Water(c - 1, r), right = !Water(c + 1, r);
                    int id;
                    if (top) id = left ? 14 : right ? 16 : 15;
                    else if (bot) id = left ? 62 : right ? 64 : 63;
                    else if (left) id = 38; else if (right) id = 40;
                    else id = rng.Next(6) == 0 ? 25 : 1;
                    rio.SetTile(Cell(c, r), P8(id));
                }

            FindBlocks();
            foreach (var b in Blocks)
                for (int c = b.c0; c <= b.c1; c++)
                    for (int r = b.r0; r <= b.r1; r++)
                    {
                        int id;
                        if (r == b.r0) id = c == b.c0 ? 19 : c == b.c1 ? 21 : 20;
                        else if (r == b.r1) id = c == b.c0 ? 67 : c == b.c1 ? 69 : 68;
                        else if (c == b.c0) id = 43; else if (c == b.c1) id = 45;
                        else { int p = rng.Next(20); id = p < 14 ? 0 : p < 19 ? 24 : 48; }
                        suelo.SetTile(Cell(c, r), P8(id));
                    }
        }

        bool Junction(int[,] ids, int c, int r)
        {
            if (!In(c, r) || ids[c, r] == 0) return false;
            int id = ids[c, r];
            return !(id >= 288 && id <= 290);
        }

        // Manzanas = rectángulos conexos de celdas 'B'.
        void FindBlocks()
        {
            var seen = new bool[W, H];
            for (int c = 0; c < W; c++)
                for (int r = 0; r < H; r++)
                {
                    if (seen[c, r] || T[c, r] != 'B') continue;
                    var b = new Block { c0 = c, r0 = r, c1 = c, r1 = r };
                    var q = new Queue<Vector2Int>(); q.Enqueue(new Vector2Int(c, r)); seen[c, r] = true;
                    while (q.Count > 0)
                    {
                        var v = q.Dequeue();
                        b.c0 = Mathf.Min(b.c0, v.x); b.c1 = Mathf.Max(b.c1, v.x); b.r0 = Mathf.Min(b.r0, v.y); b.r1 = Mathf.Max(b.r1, v.y);
                        foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                        {
                            var n = v + d;
                            if (In(n.x, n.y) && !seen[n.x, n.y] && T[n.x, n.y] == 'B') { seen[n.x, n.y] = true; q.Enqueue(n); }
                        }
                    }
                    Blocks.Add(b);
                }
        }

        public void FillInterior(Block b, Tilemap tm, TileBase t)
        {
            for (int c = b.c0 + 1; c < b.c1; c++) for (int r = b.r0 + 1; r < b.r1; r++) tm.SetTile(Cell(c, r), t);
        }

        public void Building(int x0, int x1, int rTop, int rBot, int[] s)
        {
            for (int r = rTop; r < rBot; r++)
                for (int c = x0; c <= x1; c++)
                {
                    int row = r == rTop ? 0 : (r == rBot - 1 ? 2 : 1);
                    int col = c == x0 ? 0 : (c == x1 ? 3 : 1 + ((c - x0) % 2));
                    bld.SetTile(Cell(c, r), P8(s[row * 4 + col]));
                }
            int door = (x0 + x1) / 2;
            for (int c = x0; c <= x1; c++)
                bld.SetTile(Cell(c, rBot), P8(c == door ? s[13] : c == x0 ? s[12] : c == x1 ? s[15] : s[14]));
        }

        void Tier(int x0, int x1, int rTop, int rBot)
        {
            int c = x0;
            while (c <= x1)
            {
                int left = x1 - c + 1;
                int w = left <= 5 ? left : rng.Next(3, 6);
                if (left - w > 0 && left - w < 3) w = left - 3;
                int p = rng.Next(10);
                Building(c, c + w - 1, rTop, rBot, RoofSets[p < 6 ? 0 : p < 8 ? 2 : 1]);
                c += w;
            }
        }

        void GardenRow(int x0, int x1, int r)
        {
            for (int c = x0; c <= x1; c++)
            {
                int p = rng.Next(100);
                if (p < 30) decor.SetTile(Cell(c, r), P8(Trees[rng.Next(Trees.Length)]));
                else if (p < 40) decor.SetTile(Cell(c, r), P8(263));
            }
        }

        // Casas de sillar en una o dos hileras con un patio arbolado al medio.
        public void GenericBlock(Block b)
        {
            int x0 = b.c0 + 1, x1 = b.c1 - 1, y0 = b.r0 + 1, y1 = b.r1 - 1, h = y1 - y0 + 1;
            if (x1 - x0 + 1 < 3 || h < 3) { for (int r = y0; r <= y1; r++) GardenRow(x0, x1, r); return; }
            if (h < 7) { Tier(x0, x1, y0, y1); return; }
            int t1 = (h - 1) / 2;
            Tier(x0, x1, y0, y0 + t1 - 1);
            GardenRow(x0, x1, y0 + t1);
            Tier(x0, x1, y0 + t1 + 1, y1);
        }

        public void FinishTilemaps()
        {
            foreach (var tm in new[] { suelo, sueloAqp, rio, bld, bldAqp, decor }) tm.CompressBounds();
        }

        GameObject SpriteObj(string name, GameObject parent, string sprite, Vector3 pos, int order, int layer)
        {
            var go = Child(name, parent); go.transform.position = pos; go.layer = layer;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr(sprite); sr.sortingOrder = order;
            return go;
        }

        GameObject landmarks;
        public GameObject Landmark(string name, string sprite, Vector3 pos, Vector2 box, Vector2 offset)
        {
            if (landmarks == null) landmarks = Ent("Landmarks");
            var go = SpriteObj(name, landmarks, sprite, pos, 3, obst);
            if (box != Vector2.zero) { var bc = go.AddComponent<BoxCollider2D>(); bc.size = box; bc.offset = offset; }
            return go;
        }

        GameObject stallsGroup;
        int stallCount;
        public void Puestos(Vector3[] positions)
        {
            if (stallsGroup == null) stallsGroup = Ent("Puestos");
            foreach (var p in positions)
            {
                var go = SpriteObj("Puesto_" + ++stallCount, stallsGroup, stallCount % 2 == 0 ? "puesto_mercado_azul" : "puesto_mercado_rojo", p, 3, obst);
                var bc = go.AddComponent<BoxCollider2D>(); bc.size = new Vector2(1.9f, 1.3f); bc.offset = new Vector2(0, -0.05f);
            }
        }

        public void Combis((Vector3, float)[] combis)
        {
            var g = Ent("Combis_Estacionadas");
            int i = 1;
            foreach (var (pos, rot) in combis)
            {
                var go = SpriteObj("Combi_" + i++, g, "combi", pos, 4, obst);
                go.transform.rotation = Quaternion.Euler(0, 0, rot);
                go.AddComponent<BoxCollider2D>().size = new Vector2(1.9f, 0.85f);
            }
        }

        public void Signs((string, Vector3)[] signs)
        {
            var g = Ent("Letreros");
            foreach (var (text, pos) in signs)
            {
                float w = text.Length * 0.3f + 0.8f;
                var go = Child("Letrero_" + text.Replace(" ", "").Replace(".", ""), g); go.transform.position = pos;
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("letrero_calle"); sr.drawMode = SpriteDrawMode.Sliced; sr.size = new Vector2(w, 0.8f); sr.sortingOrder = 6;
                var t = Child("Texto", go).AddComponent<TextMeshPro>();
                t.text = text; t.fontSize = 4.2f; t.alignment = TextAlignmentOptions.Center; t.color = Color.white; t.fontStyle = FontStyles.Bold;
                t.textWrappingMode = TextWrappingModes.NoWrap; t.rectTransform.sizeDelta = new Vector2(w, 0.8f); t.sortingOrder = 7;
            }
        }

        GameObject Spawn(string prefab, GameObject parent, string name, Vector3 pos)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab"));
            go.name = name; go.transform.SetParent(parent.transform); go.transform.position = pos;
            return go;
        }

        public void Gameplay(Vector3 spawn, Vector3[] pedestrians, Vector3[] dogs,
            (string name, string label, string icon, Vector3 pos)[] points,
            (string name, WaypointRouteGizmo.RouteMode mode, Vector3[] wps) route,
            (Vector3 origin, Vector3 target, string originName, string targetName) launch)
        {
            player = Spawn("Player_Car", gPlay, "Player_Car", spawn);

            var people = new Dictionary<string, Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/tilemap.png")) if (o is Sprite s) people[s.name] = s;
            string[] ids = { "tilemap_283", "tilemap_307", "tilemap_331", "tilemap_355", "tilemap_284", "tilemap_332", "tilemap_308", "tilemap_356" };
            var gPed = Child("Peatones", gPlay);
            for (int i = 0; i < pedestrians.Length; i++)
                Spawn("Peaton", gPed, "Peaton_" + (i + 1), pedestrians[i]).GetComponent<SpriteRenderer>().sprite = people[ids[i % ids.Length]];
            var gDog = Child("PerrosChuscos", gPlay);
            for (int i = 0; i < dogs.Length; i++) Spawn("PerroChusco", gDog, "PerroChusco_" + (i + 1), dogs[i]);

            var gPts = Child("PuntosEntrega", gPlay);
            foreach (var (name, label, icon, pos) in points)
            {
                var pt = Spawn("PuntoEntrega", gPts, name, pos);
                pt.GetComponent<SpriteRenderer>().sprite = Spr(icon);
                var mk = pt.GetComponent<DeliveryZoneMarker>(); mk.zoneName = label;
                var so = new SerializedObject(mk); so.FindProperty("icon").objectReferenceValue = Spr(icon); so.ApplyModifiedPropertiesWithoutUndo();
                this.points.Add(pt.transform);
            }

            var gRoute = Child(route.name, Child("Rutas", gPlay));
            var rg = gRoute.AddComponent<WaypointRouteGizmo>();
            var rso = new SerializedObject(rg); rso.FindProperty("mode").enumValueIndex = (int)route.mode; rso.ApplyModifiedPropertiesWithoutUndo();
            for (int i = 0; i < route.wps.Length; i++) Child("WP_" + (i + 1).ToString("00"), gRoute).transform.position = route.wps[i];

            var gLz = Child("ZonasLanzamiento", gPlay);
            var lz = Child(launch.originName, gLz); lz.transform.position = launch.origin;
            var target = Child(launch.targetName, gLz); target.transform.position = launch.target;
            var lso = new SerializedObject(lz.AddComponent<LaunchZoneGizmo>());
            lso.FindProperty("target").objectReferenceValue = target.transform; lso.ApplyModifiedPropertiesWithoutUndo();
        }

        public void Finish(string levelName, int deliveries, float timeLimit, string nextScene)
        {
            var gLim = Ent("Limites");
            float hw = W / 2f, hh = H / 2f;
            foreach (var (pos, size) in new[] { (new Vector2(-hw - 1, 0), new Vector2(2, H + 4)), (new Vector2(hw + 1, 0), new Vector2(2, H + 4)), (new Vector2(0, hh + 1), new Vector2(W + 4, 2)), (new Vector2(0, -hh - 1), new Vector2(W + 4, 2)) })
            {
                var l = Child("Limite", gLim); l.layer = obst; l.transform.position = pos;
                l.AddComponent<BoxCollider2D>().size = size;
            }

            managers = Spawn("Managers", gMan, "Managers", Vector3.zero);
            var om = managers.GetComponent<OrderManager>();
            var oso = new SerializedObject(om);
            var arr = oso.FindProperty("possibleDestinations"); arr.arraySize = points.Count;
            for (int i = 0; i < points.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            oso.FindProperty("player").objectReferenceValue = player.transform;
            oso.FindProperty("orderTimeLimit").floatValue = timeLimit;
            oso.ApplyModifiedPropertiesWithoutUndo();
            var lso = new SerializedObject(managers.GetComponent<LevelManager>());
            lso.FindProperty("levelDisplayName").stringValue = levelName;
            lso.FindProperty("deliveriesToComplete").intValue = deliveries;
            lso.FindProperty("nextLevelScene").stringValue = nextScene;
            lso.ApplyModifiedPropertiesWithoutUndo();

            var camGo = Child("Main Camera", gCam); camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 6; cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.345f, 0.627f, 0.847f); cam.cullingMask = ~(1 << mini);
            camGo.AddComponent<AudioListener>();
            var fso = new SerializedObject(camGo.AddComponent<CameraFollow2D>());
            fso.FindProperty("target").objectReferenceValue = player.transform;
            fso.FindProperty("useBounds").boolValue = true;
            fso.FindProperty("minBounds").vector2Value = new Vector2(-hw + 10.7f, -hh + 6f);
            fso.FindProperty("maxBounds").vector2Value = new Vector2(hw - 10.7f, hh - 6f);
            fso.ApplyModifiedPropertiesWithoutUndo();

            var lgso = new SerializedObject(Child("Global Light 2D", gCam).AddComponent<Light2D>());
            lgso.FindProperty("m_LightType").intValue = (int)Light2D.LightType.Global; lgso.ApplyModifiedPropertiesWithoutUndo();

            var gMini = Child("Minimapa", gCam);
            var mcGo = Child("MinimapCamera", gMini); mcGo.transform.position = new Vector3(0, 0, -10);
            var mc = mcGo.AddComponent<Camera>();
            mc.orthographic = true; mc.orthographicSize = hh; mc.clearFlags = CameraClearFlags.SolidColor;
            mc.backgroundColor = new Color(0.1f, 0.12f, 0.18f); mc.depth = -2;
            mc.targetTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/Art/UI/Minimap_RT.renderTexture");
            var blip = SpriteObj("DestinoBlip", gMini, "ui_blip_destino", Vector3.zero, 20, mini);
            blip.transform.localScale = Vector3.one * 1.6f;
            var mso = new SerializedObject(gMini.AddComponent<MinimapController>());
            mso.FindProperty("orderManager").objectReferenceValue = om;
            mso.FindProperty("destinationBlip").objectReferenceValue = blip.transform;
            mso.ApplyModifiedPropertiesWithoutUndo();

            var es = Child("EventSystem", gUI); es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
            var hud = Spawn("HUD_Canvas", gUI, "HUD_Canvas", Vector3.zero);
            var hso = new SerializedObject(hud.GetComponent<HUDController>());
            hso.FindProperty("orderManager").objectReferenceValue = om;
            hso.FindProperty("levelManager").objectReferenceValue = managers.GetComponent<LevelManager>();
            hso.ApplyModifiedPropertiesWithoutUndo();
            var aso = new SerializedObject(hud.GetComponentInChildren<DestinationArrow>(true));
            aso.FindProperty("orderManager").objectReferenceValue = om;
            aso.FindProperty("player").objectReferenceValue = player.transform;
            aso.FindProperty("worldCamera").objectReferenceValue = cam;
            aso.ApplyModifiedPropertiesWithoutUndo();

            // Las colisiones de los tilemaps se generan al guardar para que queden serializadas.
            foreach (var tc in Object.FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None))
            {
                tc.enabled = false; tc.enabled = true;
                tc.ProcessTilemapChanges();
                tc.GetComponent<CompositeCollider2D>().GenerateGeometry();
            }

            string path = "Assets/Scenes/" + sceneName + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            AddToBuild(path);
            Debug.Log("[Arequipa] " + sceneName + " construido en " + path);
        }

        static void AddToBuild(string path)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (list.Exists(s => s.path == path)) return;
            list.Add(new EditorBuildSettingsScene(path, true));
            list.Sort((a, b) => string.CompareOrdinal(a.path.Contains("MainMenu") ? "0" : a.path, b.path.Contains("MainMenu") ? "0" : b.path));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
