// CityMapExpander.cs
// Editor script - run via menu: Tools > City > Expand City Map
// Expands the Bridge city map to the right in Final Fantasy 5/6 style.
// IMPORTANT: Open the City/Bridge scene before running.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class CityMapExpander
{
    // =========================================================
    //  TILE ASSET PATHS  (already used in Bridge.unity)
    // =========================================================

    // --- Ground tiles (Doma Castle Exterior tileset) ---
    const string PATH_GROUND_FILL   = "Assets/Palettes/CastleExterior/OutsideCastle/SNES - Final Fantasy VI - World of Balance Maps - Doma Castle (Exterior)_3633.asset"; // idx 20
    const string PATH_GROUND_BOT    = "Assets/Palettes/CastleExterior/OutsideCastle/SNES - Final Fantasy VI - World of Balance Maps - Doma Castle (Exterior)_3829.asset"; // idx 29 (very common)
    const string PATH_GROUND_ROAD   = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_402.asset";                                                          // idx 5
    const string PATH_GROUND_STONE  = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_424.asset";                                                          // idx 3
    const string PATH_GROUND_EDGE_L = "Assets/Palettes/CastleExterior/OutsideCastle/SNES - Final Fantasy VI - World of Balance Maps - Doma Castle (Exterior)_3744.asset"; // idx 12
    const string PATH_GROUND_EDGE_C = "Assets/Palettes/CastleExterior/OutsideCastle/SNES - Final Fantasy VI - World of Balance Maps - Doma Castle (Exterior)_3745.asset"; // idx 14
    const string PATH_GROUND_EDGE_R = "Assets/Palettes/CastleExterior/OutsideCastle/SNES - Final Fantasy VI - World of Balance Maps - Doma Castle (Exterior)_3746.asset"; // idx 13
    const string PATH_GROUND_ROW3_L = "Assets/Palettes/CastleExterior/OutsideCastle/SNES - Final Fantasy VI - World of Balance Maps - Doma Castle (Exterior)_3548.asset"; // idx 15
    const string PATH_GROUND_ROW3_C = "Assets/Palettes/CastleExterior/OutsideCastle/SNES - Final Fantasy VI - World of Balance Maps - Doma Castle (Exterior)_3549.asset"; // idx 17
    const string PATH_GROUND_ROW3_R = "Assets/Palettes/CastleExterior/OutsideCastle/SNES - Final Fantasy VI - World of Balance Maps - Doma Castle (Exterior)_3550.asset"; // idx 16

    // --- Wall (city-wall/base row at y=-1) ---
    const string PATH_WALL_BASE     = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_391.asset"; // idx 243 in wall layer

    // --- Building tiles (FF5 Town tileset) ---
    // Roof
    const string PATH_ROOF_CHIMNEY  = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_276.asset"; // chimney/top accent  (idx 261)
    const string PATH_ROOF_L        = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_291.asset"; // roof left           (idx 259)
    const string PATH_ROOF_C        = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_273.asset"; // roof centre         (idx 257)
    const string PATH_ROOF_R        = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_293.asset"; // roof right          (idx 260)
    const string PATH_ROOF2_L       = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_272.asset"; // second roof row L   (idx 256)
    const string PATH_ROOF2_R       = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_274.asset"; // second roof row R   (idx 258)
    // Upper wall
    const string PATH_WALL_UP_L     = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_288.asset"; // upper wall L        (idx 253)
    const string PATH_WALL_UP_C     = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_289.asset"; // upper wall C        (idx 254)
    const string PATH_WALL_UP_R     = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_290.asset"; // upper wall R        (idx 255)
    // Mid wall
    const string PATH_WALL_MID_L    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_304.asset"; // mid wall L          (idx 249)
    const string PATH_WALL_MID_C    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_305.asset"; // mid wall C          (idx 252)
    const string PATH_WALL_MID_R    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_306.asset"; // mid wall R          (idx 251)
    const string PATH_WALL_MID_CD   = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_356.asset"; // mid wall C (door)   (idx 250)
    // Base
    const string PATH_WALL_BOT_L    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_371.asset"; // base L              (idx 245)
    const string PATH_WALL_BOT_C    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_292.asset"; // base C              (idx 246)
    const string PATH_WALL_BOT_R    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_373.asset"; // base R              (idx 247)
    const string PATH_WALL_BOT_CD   = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_372.asset"; // base C (door/arch)  (idx 248)
    // Wide-building accent tiles
    const string PATH_WIDE_MID_L    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_339.asset"; // wide mid L         (idx 270)
    const string PATH_WIDE_MID_C    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_341.asset"; // wide mid C         (idx 269)
    const string PATH_WIDE_BOT_L    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_307.asset"; // wide base L        (idx 268)
    const string PATH_WIDE_BOT_C    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_308.asset"; // wide base C        (idx 267)
    // Tree tiles
    const string PATH_TREE_TOP_L    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_138.asset"; // tree top L         (idx 264)
    const string PATH_TREE_TOP_C    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_139.asset"; // tree top R/C       (idx 265)
    const string PATH_TREE_BOT_L    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_153.asset"; // tree bottom L      (idx 266)
    const string PATH_TREE_BOT_C    = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_154.asset"; // tree bottom R/C    (idx 262)
    const string PATH_TREE_SINGLE   = "Assets/Palettes/SNES - Final Fantasy 5 (JPN) - Tilesets - Town_50.asset";  // single tree        (idx 263)

    // =========================================================
    [MenuItem("Tools/City/Expand City Map")]
    public static void ExpandCityMap()
    {
        // --- find tilemaps by name ---
        Tilemap wallTm   = FindTilemap("Wall");
        Tilemap groundTm = FindTilemap("Ground");

        if (wallTm == null || groundTm == null)
        {
            Debug.LogError("[CityMapExpander] Could not find 'Wall' and/or 'Ground' tilemaps. Open the Bridge scene first.");
            return;
        }

        // --- load tile assets ---
        var gFill   = Load(PATH_GROUND_FILL);
        var gBot    = Load(PATH_GROUND_BOT);
        var gRoad   = Load(PATH_GROUND_ROAD);
        var gStone  = Load(PATH_GROUND_STONE);
        var gEdgeL  = Load(PATH_GROUND_EDGE_L);
        var gEdgeC  = Load(PATH_GROUND_EDGE_C);
        var gEdgeR  = Load(PATH_GROUND_EDGE_R);
        var gRow3L  = Load(PATH_GROUND_ROW3_L);
        var gRow3C  = Load(PATH_GROUND_ROW3_C);
        var gRow3R  = Load(PATH_GROUND_ROW3_R);

        var wBase   = Load(PATH_WALL_BASE);

        var rChimney = Load(PATH_ROOF_CHIMNEY);
        var rL       = Load(PATH_ROOF_L);
        var rC       = Load(PATH_ROOF_C);
        var rR       = Load(PATH_ROOF_R);
        var r2L      = Load(PATH_ROOF2_L);
        var r2R      = Load(PATH_ROOF2_R);

        var wUpL  = Load(PATH_WALL_UP_L);
        var wUpC  = Load(PATH_WALL_UP_C);
        var wUpR  = Load(PATH_WALL_UP_R);
        var wMidL = Load(PATH_WALL_MID_L);
        var wMidC = Load(PATH_WALL_MID_C);
        var wMidR = Load(PATH_WALL_MID_R);
        var wMidD = Load(PATH_WALL_MID_CD);
        var wBotL = Load(PATH_WALL_BOT_L);
        var wBotC = Load(PATH_WALL_BOT_C);
        var wBotR = Load(PATH_WALL_BOT_R);
        var wBotD = Load(PATH_WALL_BOT_CD);

        var wmL   = Load(PATH_WIDE_MID_L);
        var wmC   = Load(PATH_WIDE_MID_C);
        var wbL   = Load(PATH_WIDE_BOT_L);
        var wbC   = Load(PATH_WIDE_BOT_C);

        var tTL   = Load(PATH_TREE_TOP_L);
        var tTC   = Load(PATH_TREE_TOP_C);
        var tBL   = Load(PATH_TREE_BOT_L);
        var tBC   = Load(PATH_TREE_BOT_C);
        var tS    = Load(PATH_TREE_SINGLE);

        // ======================================================
        //  GROUND EXTENSION  (x = 68..104, y = -6..-2)
        // ======================================================
        // Replicate the existing Ground pattern observed in the scene:
        //   y=-2  : gEdgeL / gEdgeC (fill) / gEdgeR
        //   y=-3  : gRow3L / gRow3C (fill) / gRow3R
        //   y=-4  : gStone (fill)
        //   y=-5  : gRoad  (fill – the main road row)
        //   y=-6  : gBot   (fill – bottom border)

        int gx0 = 68, gx1 = 104;

        for (int x = gx0; x <= gx1; x++)
        {
            SetG(groundTm, x, -6, gBot);
            SetG(groundTm, x, -5, gRoad);
            SetG(groundTm, x, -4, gStone);

            // y=-3: left/right edges + fill
            TileBase row3 = (x == gx0) ? gRow3L : (x == gx1) ? gRow3R : gRow3C;
            SetG(groundTm, x, -3, row3);

            // y=-2: left/right edges + fill
            TileBase row2 = (x == gx0) ? gEdgeL : (x == gx1) ? gEdgeR : gEdgeC;
            SetG(groundTm, x, -2, row2);
        }

        // ======================================================
        //  WALL-BASE ROW at y=-1  (continues the existing wall)
        // ======================================================
        for (int x = gx0; x <= gx1; x++)
            SetG(wallTm, x, -1, wBase);

        // ======================================================
        //  CITY BUILDINGS
        //  Each building is placed relative to a left-edge X.
        //  Ground floor = y=3, roof peak = y=8 or 9.
        // ======================================================

        // -- Building A  (narrow, 3 wide, door, x=68-70) ------
        PlaceNarrowBuilding(wallTm, 68, hasChimney: false, hasDoor: true,
            rL, rC, rR, r2L, rC, r2R, wUpL, wUpC, wUpR,
            wMidL, wMidC, wMidR, wMidD, wBotL, wBotC, wBotR, wBotD);

        // gap x=71 (open / tree)
        SetG(wallTm, 71, 5, tTL); SetG(wallTm, 71, 4, tBL);

        // -- Building B (narrow, 3 wide, x=72-74) --------------
        PlaceNarrowBuilding(wallTm, 72, hasChimney: true, hasDoor: false,
            rL, rC, rR, r2L, rC, r2R, wUpL, wUpC, wUpR,
            wMidL, wMidC, wMidR, wMidD, wBotL, wBotC, wBotR, wBotD,
            rChimney);

        // gap x=75-76 (trees)
        SetG(wallTm, 75, 5, tTL); SetG(wallTm, 76, 5, tTC);
        SetG(wallTm, 75, 4, tBL); SetG(wallTm, 76, 4, tBC);

        // -- Building C (narrow, 3 wide, door, x=77-79) --------
        PlaceNarrowBuilding(wallTm, 77, hasChimney: false, hasDoor: true,
            rL, rC, rR, r2L, rC, r2R, wUpL, wUpC, wUpR,
            wMidL, wMidC, wMidR, wMidD, wBotL, wBotC, wBotR, wBotD);

        // gap x=80 (tree)
        SetG(wallTm, 80, 5, tTC); SetG(wallTm, 80, 4, tBC);

        // -- Building D (wide, 5 tiles, x=81-85) ---------------
        PlaceWideBuilding(wallTm, 81, 5,
            rL, rC, rR, r2L, rC, r2R, wUpL, wUpC, wUpR,
            wmL, wmC, wbL, wbC, wBotL, wBotR);

        // gap x=86-87 (trees)
        SetG(wallTm, 86, 5, tTL); SetG(wallTm, 87, 5, tTC);
        SetG(wallTm, 86, 4, tBL); SetG(wallTm, 87, 4, tBC);
        SetG(wallTm, 86, 3, tS);

        // -- Building E (narrow, 3 wide, door, x=88-90) --------
        PlaceNarrowBuilding(wallTm, 88, hasChimney: true, hasDoor: true,
            rL, rC, rR, r2L, rC, r2R, wUpL, wUpC, wUpR,
            wMidL, wMidC, wMidR, wMidD, wBotL, wBotC, wBotR, wBotD,
            rChimney);

        // gap x=91
        SetG(wallTm, 91, 5, tTL); SetG(wallTm, 91, 4, tBL);

        // -- Building F (narrow, 3 wide, x=92-94) ---------------
        PlaceNarrowBuilding(wallTm, 92, hasChimney: false, hasDoor: false,
            rL, rC, rR, r2L, rC, r2R, wUpL, wUpC, wUpR,
            wMidL, wMidC, wMidR, wMidD, wBotL, wBotC, wBotR, wBotD);

        // gap x=95-96 (trees)
        SetG(wallTm, 95, 5, tTL); SetG(wallTm, 96, 5, tTC);
        SetG(wallTm, 95, 4, tBL); SetG(wallTm, 96, 4, tBC);

        // -- Building G (wide, 6 tiles, x=97-102) ---------------
        PlaceWideBuilding(wallTm, 97, 6,
            rL, rC, rR, r2L, rC, r2R, wUpL, wUpC, wUpR,
            wmL, wmC, wbL, wbC, wBotL, wBotR);

        // end tree cluster (x=103-104)
        SetG(wallTm, 103, 5, tTL); SetG(wallTm, 104, 5, tTC);
        SetG(wallTm, 103, 4, tBL); SetG(wallTm, 104, 4, tBC);

        // ======================================================
        //  MARK DIRTY + SAVE
        // ======================================================
        EditorUtility.SetDirty(wallTm);
        EditorUtility.SetDirty(groundTm);
        EditorSceneManager.MarkSceneDirty(wallTm.gameObject.scene);
        EditorSceneManager.SaveScene(wallTm.gameObject.scene);

        Debug.Log("[CityMapExpander] City expanded successfully! Ground x=68-104, 8 new buildings added.");
    }

    // =========================================================
    //  HELPERS
    // =========================================================

    static void SetG(Tilemap tm, int x, int y, TileBase tile)
    {
        if (tile == null) return;
        tm.SetTile(new Vector3Int(x, y, 0), tile);
    }

    /// <summary>
    /// Place a standard narrow building (3 tiles wide).
    /// leftX = left column. Building occupies leftX, leftX+1, leftX+2.
    /// Ground floor y=3, roof peak y=8 (y=9 if chimney).
    /// </summary>
    static void PlaceNarrowBuilding(Tilemap tm, int leftX,
        bool hasChimney, bool hasDoor,
        TileBase rL, TileBase rC, TileBase rR,
        TileBase r2L, TileBase r2C, TileBase r2R,
        TileBase wUpL, TileBase wUpC, TileBase wUpR,
        TileBase wMidL, TileBase wMidC, TileBase wMidR, TileBase wMidD,
        TileBase wBotL, TileBase wBotC, TileBase wBotR, TileBase wBotD,
        TileBase chimney = null)
    {
        int x0 = leftX, x1 = leftX + 1, x2 = leftX + 2;

        if (hasChimney && chimney != null)
            SetG(tm, x1, 9, chimney);

        // Roof top row (y=8)
        SetG(tm, x0, 8, rL);
        SetG(tm, x1, 8, rC);
        SetG(tm, x2, 8, rR);

        // Roof lower row (y=7)
        SetG(tm, x0, 7, r2L);
        SetG(tm, x1, 7, r2C);
        SetG(tm, x2, 7, r2R);

        // Upper wall (y=6)
        SetG(tm, x0, 6, wUpL);
        SetG(tm, x1, 6, wUpC);
        SetG(tm, x2, 6, wUpR);

        // Mid wall upper (y=5)
        SetG(tm, x0, 5, wMidL);
        SetG(tm, x1, 5, wMidC);
        SetG(tm, x2, 5, wMidR);

        // Mid wall lower (y=4) - use door tile in centre if hasDoor
        SetG(tm, x0, 4, wMidL);
        SetG(tm, x1, 4, hasDoor ? wMidD : wMidC);
        SetG(tm, x2, 4, wMidR);

        // Base (y=3) - use door arch in centre if hasDoor
        SetG(tm, x0, 3, wBotL);
        SetG(tm, x1, 3, hasDoor ? wBotD : wBotC);
        SetG(tm, x2, 3, wBotR);
    }

    /// <summary>
    /// Place a wide building. width >= 3.
    /// Uses different tile set for inner columns (wmL/wmC for mid, wbL/wbC for base).
    /// </summary>
    static void PlaceWideBuilding(Tilemap tm, int leftX, int width,
        TileBase rL, TileBase rC, TileBase rR,
        TileBase r2L, TileBase r2C, TileBase r2R,
        TileBase wUpL, TileBase wUpC, TileBase wUpR,
        TileBase wmL, TileBase wmC,   // wide mid tiles (inner)
        TileBase wbL, TileBase wbC,   // wide base inner tiles
        TileBase edgeL, TileBase edgeR)
    {
        int xLast = leftX + width - 1;

        // Roof (y=8)
        SetG(tm, leftX, 8, rL);
        for (int x = leftX + 1; x < xLast; x++) SetG(tm, x, 8, rC);
        SetG(tm, xLast, 8, rR);

        // Roof lower (y=7)
        SetG(tm, leftX, 7, r2L);
        for (int x = leftX + 1; x < xLast; x++) SetG(tm, x, 7, r2C);
        SetG(tm, xLast, 7, r2R);

        // Upper wall (y=6)
        SetG(tm, leftX, 6, wUpL);
        for (int x = leftX + 1; x < xLast; x++) SetG(tm, x, 6, wUpC);
        SetG(tm, xLast, 6, wUpR);

        // Mid wall upper (y=5)
        SetG(tm, leftX, 5, wmL);
        for (int x = leftX + 1; x < xLast; x++) SetG(tm, x, 5, wmC);
        SetG(tm, xLast, 5, wmL);

        // Mid wall lower (y=4) – wide building tiles
        SetG(tm, leftX, 4, wmL);
        for (int x = leftX + 1; x < xLast; x++) SetG(tm, x, 4, wmC);
        SetG(tm, xLast, 4, wmL);

        // Base (y=3) – wide building base
        SetG(tm, leftX, 3, wbL);
        for (int x = leftX + 1; x < xLast; x++) SetG(tm, x, 3, wbC);
        SetG(tm, xLast, 3, wbL);
    }

    static TileBase Load(string assetPath)
    {
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        if (tile == null)
            Debug.LogWarning($"[CityMapExpander] Could not load tile at: {assetPath}");
        return tile;
    }

    static Tilemap FindTilemap(string name)
    {
        foreach (var tm in Object.FindObjectsOfType<Tilemap>())
            if (tm.name == name)
                return tm;
        return null;
    }
}
#endif
