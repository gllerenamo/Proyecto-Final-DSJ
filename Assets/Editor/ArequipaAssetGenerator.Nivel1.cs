using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.Tilemaps;

// Sprites del Nivel 1 (Centro Histórico) y de la UI: Catedral, Santa Catalina,
// Puente Bolognesi, San Camilo, combis, letreros y el fondo del menú con el Misti.
public static partial class ArequipaAssetGenerator
{
    // === Paleta extendida ===
    static readonly Color32 AZUL_SC     = new Color32(38,  84,  160, 255); // muros de Santa Catalina
    static readonly Color32 AZUL_SC_O   = new Color32(26,  58,  118, 255);
    static readonly Color32 AZUL_SC_C   = new Color32(70,  118, 196, 255);
    static readonly Color32 TERRACOTA   = new Color32(178, 66,  38,  255);
    static readonly Color32 TERRACOTA_O = new Color32(128, 42,  22,  255);
    static readonly Color32 TERRACOTA_C = new Color32(208, 98,  58,  255);
    static readonly Color32 CREMA       = new Color32(238, 214, 160, 255);
    static readonly Color32 GRIS        = new Color32(150, 152, 160, 255);
    static readonly Color32 GRIS_O      = new Color32(98,  100, 112, 255);
    static readonly Color32 GRIS_C      = new Color32(196, 198, 206, 255);
    static readonly Color32 VIDRIO      = new Color32(140, 196, 226, 255);
    static readonly Color32 MADERA      = new Color32(110, 62,  28,  255);
    static readonly Color32 BRONCE      = new Color32(156, 108, 48,  255);
    static readonly Color32 BRONCE_C    = new Color32(206, 160, 82,  255);
    static readonly Color32 AGUA        = new Color32(56,  146, 216, 255);
    static readonly Color32 AGUA_C      = new Color32(130, 196, 240, 255);
    static readonly Color32 CIELO_ALTO  = new Color32(58,  118, 204, 255);
    static readonly Color32 CIELO_BAJO  = new Color32(170, 212, 242, 255);
    static readonly Color32 MISTI       = new Color32(124, 98,  112, 255);
    static readonly Color32 MISTI_S     = new Color32(92,  70,  88,  255);
    static readonly Color32 CHACHANI    = new Color32(146, 126, 136, 255);
    static readonly Color32 NIEVE       = new Color32(250, 250, 252, 255);
    static readonly Color32 VERDE       = new Color32(72,  146, 52,  255);
    static readonly Color32 VERDE_C     = new Color32(110, 178, 70,  255);
    static readonly Color32 VERDE_NEON  = new Color32(60,  230, 100, 255);
    static readonly Color32 PLACA       = new Color32(28,  52,  112, 255);

    const string GenFolder = "Assets/Art/Generated";
    const string PalettePath = "Assets/Art/Arequipa_Palette.prefab";

    [MenuItem("Arequipa/Generar Sprites Nivel 1 (Centro Histórico)")]
    public static void GenerateLevel1Sprites()
    {
        string full = Application.dataPath + "/Art/Generated/";

        GenerateMuro(full + "tile_muro_azul.png", AZUL_SC, AZUL_SC_O, AZUL_SC_C);
        GenerateMuro(full + "tile_muro_ocre.png", TERRACOTA, TERRACOTA_O, TERRACOTA_C);
        GenerateTechoSillar(full + "tile_techo_sillar.png");
        GeneratePuente(full + "tile_puente_sillar.png");
        GenerateBaranda(full + "tile_puente_baranda.png");
        GenerateCatedral(full + "catedral.png");
        GenerateMercado(full + "mercado_san_camilo.png");
        GenerateFuenteGrande(full + "fuente_plaza_grande.png");
        GenerateCombi(full + "combi.png");
        GenerateLetrero(full + "letrero_calle.png");
        GenerateFondoMisti(full + "fondo_menu_misti.png");
        GeneratePanelSillar(full + "ui_panel_sillar.png");
        GenerateFlecha(full + "ui_flecha.png");
        GenerateBlip(full + "ui_blip_jugador.png", ROJO);
        GenerateBlipDestino(full + "ui_blip_destino.png");

        AssetDatabase.Refresh();

        string[] tiles16 = { "tile_muro_azul", "tile_muro_ocre", "tile_techo_sillar", "tile_puente_sillar", "tile_puente_baranda" };
        foreach (var n in tiles16) ConfigureSprite(n, 16, Vector4.zero);
        ConfigureSprite("catedral", 16, Vector4.zero);
        ConfigureSprite("mercado_san_camilo", 16, Vector4.zero);
        ConfigureSprite("fuente_plaza_grande", 16, Vector4.zero);
        ConfigureSprite("combi", 16, Vector4.zero);
        ConfigureSprite("letrero_calle", 16, new Vector4(3, 3, 3, 3));
        ConfigureSprite("fondo_menu_misti", 16, Vector4.zero);
        ConfigureSprite("ui_panel_sillar", 16, new Vector4(6, 6, 6, 6));
        ConfigureSprite("ui_flecha", 16, Vector4.zero);
        ConfigureSprite("ui_blip_jugador", 4, Vector4.zero);
        ConfigureSprite("ui_blip_destino", 4, Vector4.zero);

        CreateTile("tile_muro_azul", Tile.ColliderType.Grid);
        CreateTile("tile_muro_ocre", Tile.ColliderType.Grid);
        CreateTile("tile_techo_sillar", Tile.ColliderType.Grid);
        CreateTile("tile_puente_sillar", Tile.ColliderType.None);
        CreateTile("tile_puente_baranda", Tile.ColliderType.None);

        BuildPalette();
        AssetDatabase.SaveAssets();
        Debug.Log("[Arequipa] Sprites del Nivel 1 generados en " + GenFolder);
    }

    // ── import / assets ──────────────────────────────────────────────────

    static void ConfigureSprite(string name, int ppu, Vector4 border)
    {
        string path = $"{GenFolder}/{name}.png";
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = ppu;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.spriteBorder = border;
        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(settings);
        imp.SaveAndReimport();
    }

    static void CreateTile(string name, Tile.ColliderType collider)
    {
        string path = $"{GenFolder}/{name}.asset";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{GenFolder}/{name}.png");
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
        }
        tile.sprite = sprite;
        tile.colliderType = collider;
        EditorUtility.SetDirty(tile);
    }

    // Paleta con todos los tiles hechos a mano para Arequipa, para seguir pintando niveles.
    static void BuildPalette()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) == null)
        {
            GridPaletteUtility.CreateNewPalette("Assets/Art", "Arequipa_Palette", GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Manual, Vector3.one, GridLayout.CellSwizzle.XYZ);
        }

        string[] names =
        {
            "tile_adoquin", "tile_sillar_piso", "tile_jardin_plaza", "tile_palmera", "tile_portal_arco",
            "tile_techo_sillar", "tile_muro_azul", "tile_muro_ocre", "tile_puente_sillar", "tile_puente_baranda"
        };

        var root = PrefabUtility.LoadPrefabContents(PalettePath);
        var tm = root.GetComponentInChildren<Tilemap>();
        tm.ClearAllTiles();
        for (int i = 0; i < names.Length; i++)
        {
            var t = AssetDatabase.LoadAssetAtPath<TileBase>($"{GenFolder}/{names[i]}.asset");
            if (t != null) tm.SetTile(new Vector3Int(i % 5, -(i / 5), 0), t);
        }
        PrefabUtility.SaveAsPrefabAsset(root, PalettePath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static Color32 Lerp(Color32 a, Color32 b, float t) => Color32.Lerp(a, b, Mathf.Clamp01(t));

    // ── TILES 16x16 ──────────────────────────────────────────────────────

    // Muro estucado de Santa Catalina: bloques grandes con juntas tenues.
    static void GenerateMuro(string path, Color32 baseC, Color32 dark, Color32 light)
    {
        int S = 16;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        FillRect(px, 0, 0, 15, 15, baseC, S);
        FillRect(px, 0, 0, 15, 0, dark, S);
        FillRect(px, 0, 8, 15, 8, dark, S);
        FillRect(px, 0, 1, 0, 7, dark, S);
        FillRect(px, 8, 9, 8, 15, dark, S);
        for (int y = 0; y < S; y++)
        for (int x = 0; x < S; x++)
            if ((x * 7 + y * 13) % 17 == 0 && px[y * S + x].Equals(baseC)) px[y * S + x] = light;
        tex.SetPixels32(px);
        Save(tex, path);
    }

    static void GenerateTechoSillar(string path)
    {
        int S = 16;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        FillRect(px, 0, 0, 15, 15, SILLAR, S);
        for (int row = 0; row < 4; row++)
        {
            int y = row * 4;
            FillRect(px, 0, y, 15, y, SILLAR_S, S);
            int off = (row % 2) * 4;
            for (int x = off; x < S; x += 8) FillRect(px, x, y + 1, x, y + 3, SILLAR_S, S);
        }
        // Porosidad típica del sillar volcánico.
        for (int y = 0; y < S; y++)
        for (int x = 0; x < S; x++)
            if ((x * 5 + y * 11) % 23 == 0 && px[y * S + x].Equals(SILLAR)) px[y * S + x] = BLANCO;
        tex.SetPixels32(px);
        Save(tex, path);
    }

    static void GeneratePuente(string path)
    {
        int S = 16;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        FillRect(px, 0, 0, 15, 15, SILLAR_S, S);
        for (int y = 0; y < S; y += 4)
        {
            int off = (y / 4 % 2) * 3;
            for (int x = off; x < S; x += 6)
                FillRect(px, x, y, x + 4, y + 2, SILLAR, S);
        }
        tex.SetPixels32(px);
        Save(tex, path);
    }

    // Baranda de sillar en la parte superior del tile; abajo se usa espejada.
    static void GenerateBaranda(string path)
    {
        int S = 16;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        FillRect(px, 0, 13, 15, 15, BLANCO, S);
        FillRect(px, 0, 13, 15, 13, SILLAR_S, S);
        for (int x = 1; x < S; x += 4) FillRect(px, x, 10, x + 1, 12, SILLAR, S);
        FillRect(px, 0, 9, 15, 9, SILLAR_S, S);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    // ── LANDMARKS ────────────────────────────────────────────────────────

    // Catedral de Arequipa: fachada larga de sillar con pilastras, tres portadas y dos torres.
    static void GenerateCatedral(string path)
    {
        int W = 176, H = 80;
        var tex = NewTex(W, H);
        var px = tex.GetPixels32();

        FillRect(px, 0, 0, W - 1, 52, SILLAR, W, H);
        FillRect(px, 0, 50, W - 1, 54, SILLAR_S, W, H);
        FillRect(px, 0, 0, W - 1, 1, SILLAR_S, W, H);

        // Pilastras con capitel a lo largo de la fachada
        for (int x = 36; x <= 140; x += 8)
        {
            FillRect(px, x, 3, x + 1, 46, SILLAR_S, W, H);
            FillRect(px, x - 1, 46, x + 2, 48, SILLAR_S, W, H);
        }

        // Ventanas altas en arco
        for (int x = 39; x <= 135; x += 16)
        {
            if (x > 70 && x < 104) continue;
            FillRect(px, x, 30, x + 3, 38, NEGRO, W, H);
            FillCircle(px, x + 1, 38, 2, NEGRO, W, H);
        }

        // Portadas: central mayor y dos laterales
        DrawPortada(px, W, H, 88, 9, 26);
        DrawPortada(px, W, H, 58, 6, 20);
        DrawPortada(px, W, H, 118, 6, 20);

        // Frontón central con óculo
        for (int row = 0; row <= 12; row++)
        {
            int hw = 26 - row * 2;
            FillRect(px, 88 - hw, 55 + row, 88 + hw, 55 + row, SILLAR, W, H);
        }
        DrawRing(px, 88, 59, 3, 1, SILLAR_S, W, H);
        Px(px, 88, 59, NEGRO, W, H);

        DrawTorre(px, W, H, 4);
        DrawTorre(px, W, H, W - 32);

        Outline(px, NEGRO, W, H);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    static void DrawPortada(Color32[] px, int W, int H, int cx, int hw, int top)
    {
        FillRect(px, cx - hw - 3, 2, cx + hw + 3, top + 4, BLANCO, W, H);
        FillRect(px, cx - hw, 2, cx + hw, top - hw, MADERA, W, H);
        FillCircle(px, cx, top - hw, hw, MADERA, W, H);
        FillRect(px, cx, 2, cx, top - 2, NARANJA_O, W, H);
    }

    static void DrawTorre(Color32[] px, int W, int H, int x0)
    {
        FillRect(px, x0, 0, x0 + 27, 58, BLANCO, W, H);
        FillRect(px, x0, 56, x0 + 27, 58, SILLAR_S, W, H);
        // Campanario: dos vanos en arco
        FillRect(px, x0 + 2, 59, x0 + 25, 70, SILLAR, W, H);
        for (int i = 0; i < 2; i++)
        {
            int vx = x0 + 6 + i * 12;
            FillRect(px, vx, 60, vx + 4, 66, NEGRO, W, H);
            FillCircle(px, vx + 2, 66, 2, NEGRO, W, H);
        }
        FillRect(px, x0 + 1, 70, x0 + 26, 71, SILLAR_S, W, H);
        // Cúpula y cruz
        FillEllipse(px, x0 + 13, 72, 9, 4, SILLAR, W, H);
        FillRect(px, x0 + 13, 75, x0 + 13, 79, NEGRO, W, H);
        FillRect(px, x0 + 11, 77, x0 + 15, 77, NEGRO, W, H);
        // Ventanas de la torre
        FillRect(px, x0 + 11, 20, x0 + 15, 28, NEGRO, W, H);
        FillCircle(px, x0 + 13, 28, 2, NEGRO, W, H);
    }

    // Mercado San Camilo: edificio crema con gran techo metálico a dos aguas.
    static void GenerateMercado(string path)
    {
        int W = 128, H = 80;
        var tex = NewTex(W, H);
        var px = tex.GetPixels32();

        FillRect(px, 0, 0, W - 1, 40, CREMA, W, H);
        FillRect(px, 0, 0, W - 1, 3, SILLAR_S, W, H);
        FillRect(px, 0, 32, W - 1, 38, ROJO, W, H);
        for (int x = 6; x < W - 6; x += 4) FillRect(px, x, 34, x + 1, 36, BLANCO, W, H);

        for (int x = 8; x < W - 8; x += 12)
        {
            if (x > 44 && x < 80) continue;
            FillRect(px, x, 12, x + 5, 24, GRIS_O, W, H);
            FillCircle(px, x + 2, 24, 3, GRIS_O, W, H);
            FillRect(px, x + 1, 13, x + 4, 22, VIDRIO, W, H);
        }
        FillRect(px, 52, 4, 75, 22, NEGRO, W, H);
        FillCircle(px, 63, 22, 11, NEGRO, W, H);
        FillRect(px, 54, 4, 73, 20, MADERA, W, H);

        // Techo metálico con nervaduras
        for (int y = 41; y < H; y++)
        {
            float t = (y - 41f) / (H - 42f);
            int hw = Mathf.RoundToInt(Mathf.Lerp(63, 30, t));
            FillRect(px, 64 - hw, y, 63 + hw, y, GRIS, W, H);
            for (int x = 64 - hw; x <= 63 + hw; x++)
                if ((x % 6) == 0) Px(px, x, y, GRIS_O, W, H);
        }
        FillRect(px, 34, H - 2, 93, H - 1, GRIS_C, W, H);
        FillRect(px, 50, 62, 77, 74, VIDRIO, W, H);
        for (int x = 52; x < 77; x += 5) FillRect(px, x, 62, x, 74, GRIS_O, W, H);

        Outline(px, NEGRO, W, H);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    // Pileta de la Plaza de Armas vista desde arriba, con el "Tuturutu" de bronce al centro.
    static void GenerateFuenteGrande(string path)
    {
        int S = 48;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        FillCircle(px, 23, 23, 22, SILLAR, S);
        DrawRing(px, 23, 23, 22, 2, SILLAR_S, S);
        FillCircle(px, 23, 23, 18, AGUA, S);
        DrawRing(px, 23, 23, 14, 1, AGUA_C, S);
        DrawRing(px, 23, 23, 17, 1, AGUA_C, S);
        FillCircle(px, 23, 23, 9, SILLAR, S);
        DrawRing(px, 23, 23, 9, 2, SILLAR_S, S);
        FillCircle(px, 23, 23, 6, AGUA, S);
        FillCircle(px, 23, 23, 4, BRONCE, S);
        FillCircle(px, 22, 24, 2, BRONCE_C, S);
        Outline(px, NEGRO, S);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    // Combi arequipeña vista desde arriba (mirando a la derecha).
    static void GenerateCombi(string path)
    {
        int W = 32, H = 16;
        var tex = NewTex(W, H);
        var px = tex.GetPixels32();
        FillRect(px, 5, 1, 9, 14, NEGRO, W, H);
        FillRect(px, 22, 1, 26, 14, NEGRO, W, H);
        FillRect(px, 1, 2, 30, 13, BLANCO, W, H);
        FillRect(px, 1, 7, 26, 8, ROJO, W, H);
        FillRect(px, 1, 5, 26, 5, AZUL, W, H);
        FillRect(px, 1, 10, 26, 10, AZUL, W, H);
        FillRect(px, 25, 3, 28, 12, VIDRIO, W, H);
        FillRect(px, 3, 11, 22, 12, GRIS_C, W, H);
        Outline(px, NEGRO, W, H);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    static void GenerateLetrero(string path)
    {
        int W = 48, H = 12;
        var tex = NewTex(W, H);
        var px = tex.GetPixels32();
        FillRect(px, 0, 0, W - 1, H - 1, BLANCO, W, H);
        FillRect(px, 1, 1, W - 2, H - 2, PLACA, W, H);
        Px(px, 0, 0, CLEAR, W, H); Px(px, W - 1, 0, CLEAR, W, H);
        Px(px, 0, H - 1, CLEAR, W, H); Px(px, W - 1, H - 1, CLEAR, W, H);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    // Fondo del menú: Chachani, Misti y Pichu Pichu sobre la campiña y la ciudad de sillar.
    static void GenerateFondoMisti(string path)
    {
        int W = 320, H = 180;
        var tex = NewTex(W, H);
        var px = tex.GetPixels32();

        for (int y = 0; y < H; y++)
            FillRect(px, 0, y, W - 1, y, Lerp(CIELO_BAJO, CIELO_ALTO, (y - 50f) / 130f), W, H);

        FillEllipse(px, 60, 160, 22, 5, BLANCO, W, H);
        FillEllipse(px, 80, 163, 14, 4, BLANCO, W, H);
        FillEllipse(px, 260, 150, 18, 4, BLANCO, W, H);

        // Chachani (macizo con varios picos)
        int[] peaksX = { 30, 72, 112 };
        int[] peaksY = { 118, 128, 112 };
        DrawMountain(px, W, H, peaksX, peaksY, 0.75f, CHACHANI, 112);
        // Pichu Pichu
        DrawMountain(px, W, H, new[] { 292 }, new[] { 98 }, 0.5f, CHACHANI, 200);
        // Misti: cono casi perfecto
        for (int x = 0; x < W; x++)
        {
            int top = 150 - Mathf.RoundToInt(Mathf.Abs(x - 205) * 0.82f);
            for (int y = 40; y <= top; y++)
            {
                Color32 c = x > 205 ? MISTI_S : MISTI;
                int snow = 126 + ((x * 7) % 5) + (Mathf.Abs(x - 205) / 6);
                if (y > snow) c = NIEVE;
                px[y * W + x] = c;
            }
        }

        // Campiña con andenes
        for (int y = 0; y < 52; y++)
        {
            Color32 c = (y / 5) % 2 == 0 ? VERDE : VERDE_C;
            FillRect(px, 0, y, W - 1, y, c, W, H);
            if (y % 5 == 0) FillRect(px, 0, y, W - 1, y, Lerp(VERDE, NEGRO, 0.25f), W, H);
        }

        // Ciudad de sillar con la Catedral al centro
        int[] bx = { 0, 22, 40, 64, 96, 186, 214, 236, 262, 290 };
        int[] bw = { 20, 16, 22, 28, 22, 26, 20, 24, 26, 30 };
        int[] bh = { 14, 20, 12, 18, 16, 18, 12, 20, 14, 16 };
        for (int i = 0; i < bx.Length; i++)
        {
            FillRect(px, bx[i], 40, bx[i] + bw[i], 40 + bh[i], i % 2 == 0 ? SILLAR : BLANCO, W, H);
            FillRect(px, bx[i], 40 + bh[i], bx[i] + bw[i], 40 + bh[i], SILLAR_S, W, H);
            for (int wx = bx[i] + 3; wx < bx[i] + bw[i] - 2; wx += 6)
                FillRect(px, wx, 44, wx + 2, 48, SILLAR_S, W, H);
        }
        FillRect(px, 120, 40, 184, 66, BLANCO, W, H);
        FillRect(px, 120, 64, 184, 66, SILLAR_S, W, H);
        for (int x = 126; x < 182; x += 6) FillRect(px, x, 42, x, 62, SILLAR_S, W, H);
        foreach (int tx in new[] { 112, 182 })
        {
            FillRect(px, tx, 40, tx + 12, 82, BLANCO, W, H);
            FillRect(px, tx + 3, 70, tx + 5, 76, NEGRO, W, H);
            FillRect(px, tx + 7, 70, tx + 9, 76, NEGRO, W, H);
            FillEllipse(px, tx + 6, 83, 6, 3, SILLAR, W, H);
            FillRect(px, tx + 6, 86, tx + 6, 90, NEGRO, W, H);
            FillRect(px, tx + 4, 88, tx + 8, 88, NEGRO, W, H);
        }
        FillRect(px, 146, 40, 158, 56, MADERA, W, H);
        FillCircle(px, 152, 56, 6, MADERA, W, H);

        // Palmeras de la plaza
        foreach (int ptx in new[] { 100, 106, 200, 206 })
        {
            FillRect(px, ptx, 40, ptx + 1, 60, MADERA, W, H);
            FillEllipse(px, ptx, 61, 6, 2, VERDE, W, H);
            FillEllipse(px, ptx + 1, 63, 3, 2, VERDE_C, W, H);
        }

        tex.SetPixels32(px);
        Save(tex, path);
    }

    static void DrawMountain(Color32[] px, int W, int H, int[] peaksX, int[] peaksY, float slope, Color32 c, int snowLine)
    {
        for (int x = 0; x < W; x++)
        {
            int top = 0;
            for (int i = 0; i < peaksX.Length; i++)
                top = Mathf.Max(top, peaksY[i] - Mathf.RoundToInt(Mathf.Abs(x - peaksX[i]) * slope));
            for (int y = 40; y <= top; y++)
                px[y * W + x] = y > snowLine + ((x * 3) % 4) ? NIEVE : c;
        }
    }

    // ── UI ───────────────────────────────────────────────────────────────

    static void GeneratePanelSillar(string path)
    {
        int S = 32;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        FillRect(px, 0, 0, 31, 31, NEGRO, S);
        FillRect(px, 1, 1, 30, 30, TERRACOTA, S);
        FillRect(px, 3, 3, 28, 28, SILLAR, S);
        FillRect(px, 3, 3, 28, 3, SILLAR_S, S);
        Px(px, 0, 0, CLEAR, S); Px(px, 31, 0, CLEAR, S);
        Px(px, 0, 31, CLEAR, S); Px(px, 31, 31, CLEAR, S);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    // Flecha apuntando a la derecha (0°), se rota desde código.
    static void GenerateFlecha(string path)
    {
        int S = 32;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        FillRect(px, 3, 12, 18, 19, OCRE, S);
        for (int x = 17; x <= 29; x++)
        {
            int hw = Mathf.RoundToInt((29 - x) * 0.95f);
            FillRect(px, x, 15 - hw, x, 16 + hw, OCRE, S);
        }
        FillRect(px, 3, 17, 18, 18, BLANCO, S);
        Outline(px, NEGRO, S);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    static void GenerateBlip(string path, Color32 c)
    {
        int S = 8;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        FillCircle(px, 3, 3, 3, BLANCO, S);
        FillCircle(px, 3, 3, 2, c, S);
        tex.SetPixels32(px);
        Save(tex, path);
    }

    static void GenerateBlipDestino(string path)
    {
        int S = 8;
        var tex = NewTex(S);
        var px = tex.GetPixels32();
        for (int y = 0; y < S; y++)
        for (int x = 0; x < S; x++)
        {
            int d = Mathf.Abs(x - 3) + Mathf.Abs(y - 3);
            if (d <= 3) px[y * S + x] = d == 3 ? BLANCO : VERDE_NEON;
        }
        tex.SetPixels32(px);
        Save(tex, path);
    }
}
