using UnityEngine;
using UnityEditor;
using System.IO;

public static partial class ArequipaAssetGenerator
{
    // === Paleta Arequipa ===
    static readonly Color32 CLEAR     = new Color32(0,   0,   0,   0  );
    static readonly Color32 NEGRO     = new Color32(20,  10,  0,   255);
    static readonly Color32 BLANCO    = new Color32(245, 242, 235, 255);
    static readonly Color32 SILLAR    = new Color32(242, 230, 208, 255);
    static readonly Color32 SILLAR_S  = new Color32(180, 160, 130, 255);
    static readonly Color32 ROJO      = new Color32(204, 40,  40,  255);
    static readonly Color32 NARANJA   = new Color32(200, 80,  20,  255);
    static readonly Color32 NARANJA_O = new Color32(140, 50,  8,   255);
    static readonly Color32 OCRE      = new Color32(212, 168, 64,  255);
    static readonly Color32 OCRE_O    = new Color32(148, 112, 32,  255);
    static readonly Color32 VERDE_O   = new Color32(35,  85,  18,  255);
    static readonly Color32 AZUL      = new Color32(30,  86,  180, 255);
    static readonly Color32 AZUL_C    = new Color32(80,  140, 220, 255);

    // Colores del perro
    static readonly Color32 P_TAN  = new Color32(200, 150, 75,  255);
    static readonly Color32 P_MED  = new Color32(145, 95,  38,  255);
    static readonly Color32 P_DARK = new Color32(85,  45,  12,  255);
    static readonly Color32 P_NOSE = new Color32(45,  20,  5,   255);
    static readonly Color32 P_PINK = new Color32(185, 75,  55,  255);

    [MenuItem("Arequipa/Generar Sprites de Assets")]
    public static void GenerateAllSprites()
    {
        string folder = "Assets/Art/Generated";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Art", "Generated");

        string fullPath = Application.dataPath + "/Art/Generated/";

        GeneratePerroChusco(fullPath);
        GeneratePicanteria(fullPath);
        GenerateHospital(fullPath);
        GenerateTienda(fullPath);
        GenerateCasa(fullPath);

        AssetDatabase.Refresh();
        Debug.Log("[Arequipa] 5 sprites generados en " + folder);
    }

    // ── helpers ──────────────────────────────────────────────────────────

    // h < 0 => textura cuadrada (h = w); los sprites del Avance 1 son cuadrados.
    static void FillCircle(Color32[] px, int cx, int cy, int r, Color32 c, int w, int h = -1)
    {
        if (h < 0) h = w;
        for (int y = Mathf.Max(0, cy - r); y <= Mathf.Min(h - 1, cy + r); y++)
        for (int x = Mathf.Max(0, cx - r); x <= Mathf.Min(w - 1, cx + r); x++)
            if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                px[y * w + x] = c;
    }

    static void FillEllipse(Color32[] px, int cx, int cy, int rx, int ry, Color32 c, int w, int h = -1)
    {
        if (h < 0) h = w;
        for (int y = Mathf.Max(0, cy - ry); y <= Mathf.Min(h - 1, cy + ry); y++)
        for (int x = Mathf.Max(0, cx - rx); x <= Mathf.Min(w - 1, cx + rx); x++)
        {
            float dx = (float)(x - cx) / rx, dy = (float)(y - cy) / ry;
            if (dx * dx + dy * dy <= 1f)
                px[y * w + x] = c;
        }
    }

    static void FillRect(Color32[] px, int x0, int y0, int x1, int y1, Color32 c, int w, int h = -1)
    {
        if (h < 0) h = w;
        for (int y = Mathf.Max(0, y0); y <= Mathf.Min(h - 1, y1); y++)
        for (int x = Mathf.Max(0, x0); x <= Mathf.Min(w - 1, x1); x++)
            px[y * w + x] = c;
    }

    static void DrawRing(Color32[] px, int cx, int cy, int r, int thick, Color32 c, int w, int h = -1)
    {
        if (h < 0) h = w;
        int outer = r * r, inner = (r - thick) * (r - thick);
        for (int y = Mathf.Max(0, cy - r); y <= Mathf.Min(h - 1, cy + r); y++)
        for (int x = Mathf.Max(0, cx - r); x <= Mathf.Min(w - 1, cx + r); x++)
        {
            int d2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
            if (d2 <= outer && d2 >= inner)
                px[y * w + x] = c;
        }
    }

    static void DrawCross(Color32[] px, int cx, int cy, int arm, int thick, Color32 c, int w)
    {
        FillRect(px, cx - arm, cy - thick, cx + arm, cy + thick, c, w);
        FillRect(px, cx - thick, cy - arm, cx + thick, cy + arm, c, w);
    }

    static void Px(Color32[] px, int x, int y, Color32 c, int w, int h = -1)
    {
        if (h < 0) h = w;
        if (x >= 0 && x < w && y >= 0 && y < h) px[y * w + x] = c;
    }

    // Adds 1px dark outline around all opaque pixels
    static void Outline(Color32[] px, Color32 outC, int w, int h = -1)
    {
        if (h < 0) h = w;
        var copy = new Color32[px.Length];
        px.CopyTo(copy, 0);
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            if (copy[y * w + x].a > 0) continue;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d], ny = y + dy[d];
                if (nx >= 0 && nx < w && ny >= 0 && ny < h && copy[ny * w + nx].a > 0)
                { px[y * w + x] = outC; break; }
            }
        }
    }

    static Texture2D NewTex(int w, int h = -1)
    {
        if (h < 0) h = w;
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color32[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = CLEAR;
        t.SetPixels32(px);
        return t;
    }

    static void Save(Texture2D tex, string fullPath)
    {
        tex.Apply();
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    // ── PERRO CHUSCO — 16x16 top-down ────────────────────────────────────
    static void GeneratePerroChusco(string path)
    {
        int S = 16;
        var tex = NewTex(S);
        var px = tex.GetPixels32();

        // Cuerpo: óvalo elongado, centro (7,7), rx=4, ry=6
        FillEllipse(px, 7, 7, 4, 6, P_TAN, S);

        // Manchas de perro chusco
        FillCircle(px, 5, 9, 1, P_MED, S);
        FillCircle(px, 9, 6, 1, P_MED, S);
        FillCircle(px, 6, 4, 1, P_MED, S);
        FillCircle(px, 9, 10, 1, P_MED, S);

        // Cabeza (hacia arriba, y=11-13)
        FillEllipse(px, 7, 12, 3, 2, P_MED, S);

        // Orejas
        Px(px, 4, 13, P_DARK, S); Px(px, 5, 14, P_DARK, S);
        Px(px, 10, 13, P_DARK, S); Px(px, 9, 14, P_DARK, S);

        // Ojos
        Px(px, 5, 12, P_NOSE, S);
        Px(px, 9, 12, P_NOSE, S);

        // Nariz
        Px(px, 7, 11, P_PINK, S);
        Px(px, 7, 10, P_NOSE, S);

        // Cola (abajo, y=0-1)
        Px(px, 6, 1, P_MED, S); Px(px, 7, 1, P_MED, S);
        Px(px, 8, 1, P_MED, S); Px(px, 9, 1, P_MED, S);
        Px(px, 5, 0, P_MED, S); Px(px, 10, 0, P_MED, S);

        Outline(px, P_DARK, S);
        tex.SetPixels32(px);
        Save(tex, path + "perro_chusco.png");
        Debug.Log("[Arequipa] perro_chusco.png generado");
    }

    // ── PICANTERÍA — 32x32 ───────────────────────────────────────────────
    static void GeneratePicanteria(string path)
    {
        int S = 32;
        var tex = NewTex(S);
        var px = tex.GetPixels32();

        // Fondo naranja/terracota
        FillCircle(px, 15, 15, 14, NARANJA, S);
        DrawRing(px, 15, 15, 14, 2, NARANJA_O, S);

        // Olla: cuerpo
        FillRect(px, 8, 9, 22, 18, SILLAR, S);
        FillEllipse(px, 15, 9, 7, 2, SILLAR, S);   // base redondeada
        FillEllipse(px, 15, 18, 7, 2, SILLAR_S, S); // sombra fondo

        // Manijas
        FillRect(px, 5, 12, 8, 15, SILLAR_S, S);
        FillRect(px, 22, 12, 25, 15, SILLAR_S, S);

        // Tapa
        FillRect(px, 9, 18, 21, 20, SILLAR_S, S);
        FillCircle(px, 15, 21, 2, SILLAR_S, S); // perilla

        // Vapor (3 líneas onduladas)
        int[] vx = { 11, 15, 19 };
        for (int i = 0; i < 3; i++)
        {
            Px(px, vx[i],     22, BLANCO, S);
            Px(px, vx[i] + 1, 23, BLANCO, S);
            Px(px, vx[i],     24, BLANCO, S);
            Px(px, vx[i] + 1, 25, BLANCO, S);
        }

        Outline(px, NEGRO, S);
        tex.SetPixels32(px);
        Save(tex, path + "zona_picanteria.png");
        Debug.Log("[Arequipa] zona_picanteria.png generado");
    }

    // ── HOSPITAL — 32x32 ─────────────────────────────────────────────────
    static void GenerateHospital(string path)
    {
        int S = 32;
        var tex = NewTex(S);
        var px = tex.GetPixels32();

        FillCircle(px, 15, 15, 14, BLANCO, S);
        DrawRing(px, 15, 15, 14, 2, VERDE_O, S);
        DrawCross(px, 15, 15, 8, 3, ROJO, S);

        Outline(px, NEGRO, S);
        tex.SetPixels32(px);
        Save(tex, path + "zona_hospital.png");
        Debug.Log("[Arequipa] zona_hospital.png generado");
    }

    // ── TIENDA COMERCIAL — 32x32 ─────────────────────────────────────────
    static void GenerateTienda(string path)
    {
        int S = 32;
        var tex = NewTex(S);
        var px = tex.GetPixels32();

        FillCircle(px, 15, 15, 14, OCRE, S);
        DrawRing(px, 15, 15, 14, 2, OCRE_O, S);

        // Bolsa de compras (icono frontal)
        // Cuerpo de la bolsa
        FillRect(px, 9, 8, 21, 19, SILLAR, S);
        // Tope de la bolsa (borde más oscuro)
        FillRect(px, 9, 18, 21, 19, SILLAR_S, S);
        // Asas
        FillRect(px, 11, 19, 13, 23, SILLAR_S, S);
        FillRect(px, 17, 19, 19, 23, SILLAR_S, S);
        // Decoración: estrella en la bolsa
        Px(px, 15, 14, OCRE_O, S);
        Px(px, 14, 13, OCRE_O, S); Px(px, 16, 13, OCRE_O, S);
        Px(px, 13, 14, OCRE_O, S); Px(px, 17, 14, OCRE_O, S);
        Px(px, 14, 15, OCRE_O, S); Px(px, 16, 15, OCRE_O, S);

        Outline(px, NEGRO, S);
        tex.SetPixels32(px);
        Save(tex, path + "zona_tienda.png");
        Debug.Log("[Arequipa] zona_tienda.png generado");
    }

    // ── CASA RESIDENCIAL — 32x32 ─────────────────────────────────────────
    static void GenerateCasa(string path)
    {
        int S = 32;
        var tex = NewTex(S);
        var px = tex.GetPixels32();

        FillCircle(px, 15, 15, 14, SILLAR, S);
        DrawRing(px, 15, 15, 14, 2, SILLAR_S, S);

        // Fachada de sillar (cuerpo casa, estilo colonial Arequipa)
        FillRect(px, 7, 7, 23, 18, SILLAR_S, S);

        // Arcos coloniales (3 arcos pequeños en la fachada)
        for (int i = 0; i < 3; i++)
        {
            int ax = 9 + i * 5;
            FillCircle(px, ax, 12, 2, SILLAR, S);
            FillRect(px, ax - 2, 7, ax + 2, 12, SILLAR, S);
        }

        // Techo terracota (triángulo, y crece hacia arriba)
        for (int row = 0; row <= 7; row++)
        {
            int hw = 9 - row;
            FillRect(px, 15 - hw, 18 + row, 15 + hw, 18 + row, ROJO, S);
        }

        // Ventana balcón (detalle colonial)
        FillRect(px, 13, 14, 17, 17, AZUL_C, S);
        FillRect(px, 14, 17, 16, 18, SILLAR_S, S); // repisa balcón

        Outline(px, NEGRO, S);
        tex.SetPixels32(px);
        Save(tex, path + "zona_casa.png");
        Debug.Log("[Arequipa] zona_casa.png generado");
    }
}
