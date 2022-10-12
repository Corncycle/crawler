using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Text;

public class DungeonGeneration : MonoBehaviour
{
    public static int mapHeight = 40;
    public static int mapWidth = 40;
    public TileBase floorTile;
    public TileBase wallTile;

    public TileBase topWall;
    public TileBase blackWall;
    public TileBase frontWall;

    public Tilemap decorativeWallTilemap;
    public Tilemap groundTilemap;
    public Tilemap wallTilemap;
    public Tilemap itemTilemap;

    // what proportion of the original tiles should be walls. 0 <= inDe <= 1. default 0.45
    static float initialDensity = 0.45f;
    // how often to override with wall anyway. keep this low
    //static float chanceToOverrideWithWall = 0.02f;

    int[,] tiles;
    static Dictionary<int, string> consoleTiles = new Dictionary<int, string>()
    {
        {0, "."},
        {1, "#"}
    };

    void Awake() {
        tiles = GenerateDungeon();
        
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tiles[x, y] == 0) {
                    groundTilemap.SetTile(pos, floorTile);
                } else {
                    wallTilemap.SetTile(pos, wallTile);
                }
            }
        }

        for (int y = 1; y < mapHeight - 1; y++) {
            for (int x = 0; x < mapWidth; x++) {
                if (tiles[x, y] == 1) {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    wallTilemap.SetTile(pos, blackWall);
                    if (tiles[x, y + 1] == 0) {
                        decorativeWallTilemap.SetTile(pos + new Vector3Int(0, 1, 0), topWall);
                    } else if (tiles[x, y - 1] == 0) {
                        wallTilemap.SetTile(pos, frontWall);
                        groundTilemap.SetTile(pos, floorTile);
                    }
                }
            }
        }
    }

    public bool HasSolidTileAt((int, int) tup) {
        return (tiles[tup.Item1, tup.Item2] == 1);
    }

    bool HasSolidTileAt(int x, int y) {
        return (tiles[x, y] == 1);
    }

    int[,] GenerateDungeon() {
        var output = FillWithNoise(mapWidth, mapHeight);
        //Print2DArray(output);
        for (int _ = 0; _ < 4; _++) {
            output = DoCellularIteration(output, true, false);
            //Print2DArray(output);
        }
        for (int _ = 0; _ < 3; _++) {
            output = DoCellularIteration(output, false, false);
            //Print2DArray(output);
        }

        return output;
    }

    int[,] FillWithNoise(int width, int height) {
        int[,] output = new int[width, height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (Random.value < initialDensity) {
                    output[x, y] = 1;
                }
            }
        }
        return output;
    }

    // if extraCondition is true we allow less large open areas
    int[,] DoCellularIteration(int[,] old, bool extraCondition, bool allowWallOverride) {
        int width = old.GetLength(0);
        int height = old.GetLength(1);
        int[,] output = new int[width, height];
        // non-edges
        for (int y = 1; y < height - 1; y++) {
            int lSlice;
            int mSlice = countSlice(old, 0, y);
            int rSlice = countSlice(old, 1, y);
            for (int x = 1; x < width - 1; x++) {
                lSlice = mSlice;
                mSlice = rSlice;
                rSlice = countSlice(old, x + 1, y);
                int count = lSlice + mSlice + rSlice;
                if (extraCondition) {
                    if (count <= 1) {
                        if (check2Range(old, x, y, height, width)) {
                            output[x, y] = 1;
                            continue;
                        }
                    }
                }
                if (count >= 5) {
                    output[x, y] = 1;
                } else {
                    output[x, y] = 0;
                }
            }
        }
        // top and bottom (no corners)
        for (int x = 1; x < width - 1; x++) {
            int count = old[x-1,0] + old[x,0] + old[x+1,0] + old[x-1,1] + old[x,1] + old[x+1,1];
            if (count >= 2) {
                output[x, 0] = 1;
            }
            int h = height;
            count = old[x-1,h-1] + old[x,h-1] + old[x+1,h-1] + old[x-1,h-2] + old[x,h-2] + old[x+1,h-2];
            if (count >= 2) {
                output[x, h-1] = 1;
            }
        }
        // left and right (no corners)
        for (int y = 1; y < height - 1; y++) {
            int count = old[0,y-1] + old[0,y] + old[0,y+1] + old[1,y-1] + old[1,y] + old[1,y+1];
            if (count >= 2) {
                output[1, y] = 1;
            }
            int w = width;
            count = old[w-1,y-1] + old[w-1,y] + old[w-1,y+1] + old[w-2,y-1] + old[w-2,y] + old[w-2,y+1];
            if (count >= 2) {
                output[w-1, y] = 1;
            } 
        }
        output[0, 0] = 1;
        output[width - 1, 0] = 1;
        output[0, height - 1] = 1;
        output[width - 1, height - 1] = 1;

        return output;
    }

    bool check2Range(int[,] arr, int x, int y, int height, int width) {
        if (x < 2 || y < 2 || x > width - 3|| y > height - 3) {
            return false;
        }
        int count = 0;
        for (int yy = -2; yy < 3; yy++) {
            for (int xx = -2; xx < 3; xx++) {
                if (arr[x + xx, y + yy] != 0) {
                    count += 1;
                }
            }
        }
        return count < 1;
    }

    int countSlice(int[,] arr, int x, int y) {
        int count = 0;
        for (int yy = -1; yy < 2; yy++) {
            count += arr[x, y + yy];
        }
        return count;
    }

    public void PrintMap() {
        Print2DArray(tiles);
    }

    void Print2DArray(int[,] arr) {
        int height = arr.GetLength(0);
        int width = arr.GetLength(1);
        StringBuilder sb = new StringBuilder("", (height + 1) * width);
        for (int y = height - 1; y >= 0; y--) {
            for (int x = 0; x < width; x++) {
                sb.Append(GetConsoleChar(arr[x,y]));
            }
            sb.Append("\n");
        }
        print(sb.ToString());
    }

    string GetConsoleChar(int n) {
        if (consoleTiles.ContainsKey(n)) {
            return consoleTiles[n];
        } else {
            return "?";
        }
    }

    public Vector2 FindRandomOpenTile() {
        if (tiles is null) {
            throw (new System.Exception("Cannot find open tile in non-initialized tiles"));
        }
        while (true) {
            int x = Random.Range(0, mapWidth);
            int y = Random.Range(0, mapHeight);
            if (tiles[x, y] == 0) {
                return new Vector2(x, y);
            }
        }
    }

    public (int, int) FindRandomOpenTileTuple() {
        Vector2 spot = FindRandomOpenTile();
        return (System.Convert.ToInt32(spot.x), System.Convert.ToInt32(spot.y));
    }
}
