using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Text;

public class DungeonGeneration : MonoBehaviour
{
    static int mapHeight = 40;
    static int mapWidth = 80;
    public TileBase floorTile;
    public TileBase wallTile;

    public Tilemap groundTilemap;
    public Tilemap wallTilemap;

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
        
        for (int i = 0; i < mapHeight; i++) {
            for (int j = 0; j < mapWidth; j++) {
                Vector3Int pos = new Vector3Int(j, i, 0);
                if (tiles[i, j] == 0) {
                    groundTilemap.SetTile(pos, floorTile);
                } else {
                    wallTilemap.SetTile(pos, wallTile);
                }
            }
        }
    }

    void Start()
    {
    }

    void Update()
    {
        
    }

    public bool HasSolidTileAt((int, int) tup) {
        return (tiles[tup.Item2, tup.Item1] == 1);
    }

    bool HasSolidTileAt(int x, int y) {
        return (tiles[y, x] == 1);
    }

    int[,] GenerateDungeon() {
        var output = FillWithNoise(mapHeight, mapWidth);
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

    int[,] FillWithNoise(int height, int width) {
        int[,] output = new int[height, width];
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (Random.value < initialDensity) {
                    output[i, j] = 1;
                }
            }
        }
        return output;
    }

    // if extraCondition is true we allow less large open areas
    int[,] DoCellularIteration(int[,] old, bool extraCondition, bool allowWallOverride) {
        int height = old.GetLength(0);
        int width = old.GetLength(1);
        int[,] output = new int[height, width];
        // non-edges
        for (int i = 1; i < height - 1; i++) {
            int lSlice;
            int mSlice = countSlice(old, i, 0);
            int rSlice = countSlice(old, i, 1);
            for (int j = 1; j < width - 1; j++) {
                lSlice = mSlice;
                mSlice = rSlice;
                rSlice = countSlice(old, i, j + 1);
                int count = lSlice + mSlice + rSlice;
                if (extraCondition) {
                    if (count <= 1) {
                        if (check2Range(old, i, j, height, width)) {
                            output[i, j] = 1;
                            continue;
                        }
                    }
                }
                if (count >= 5) {
                    output[i, j] = 1;
                } else {
                    output[i, j] = 0;
                }
            }
        }
        // top and bottom (no corners)
        for (int j = 1; j < width - 1; j++) {
            int count = old[0,j-1] + old[0,j] + old[0,j+1] + old[1,j-1] + old[1,j] + old[1,j+1];
            if (count >= 2) {
                output[0, j] = 1;
            }
            int h = height;
            count = old[h-1,j-1] + old[h-1,j] + old[h-1,j+1] + old[h-2,j-1] + old[h-2,j] + old[h-2,j+1];
            if (count >= 2) {
                output[h-1, j] = 1;
            }
        }
        // left and right (no corners)
        for (int i = 1; i < height - 1; i++) {
            int count = old[i-1,0] + old[i,0] + old[i+1,0] + old[i-1,1] + old[i,1] + old[i+1,1];
            if (count >= 2) {
                output[i, 0] = 1;
            }
            int w = width;
            count = old[i-1,w-1] + old[i,w-1] + old[i+1,w-1] + old[i-1,w-2] + old[i,w-2] + old[i+1,w-2];
            if (count >= 2) {
                output[i, w-1] = 1;
            } 
        }
        output[0, 0] = 1;
        output[0, width - 1] = 1;
        output[height - 1, 0] = 1;
        output[height - 1, width - 1] = 1;

        return output;
    }

    bool check2Range(int[,] arr, int i, int j, int height, int width) {
        if (i < 2 || j < 2 || i > height - 3 || j > width - 3) {
            return false;
        }
        int count = 0;
        for (int ii = -2; ii < 3; ii++) {
            for (int jj = -2; jj < 3; jj++) {
                if (arr[i + ii, j + jj] != 0) {
                    count += 1;
                }
            }
        }
        return count < 1;
    }

    int countSlice(int[,] arr, int i, int j) {
        int count = 0;
        for (int k = -1; k < 2; k++) {
            count += arr[i + k, j];
        }
        return count;
    }

    void PrintMap() {
        Print2DArray(tiles);
    }

    void Print2DArray(int[,] arr) {
        int height = arr.GetLength(0);
        int width = arr.GetLength(1);
        StringBuilder sb = new StringBuilder("", (height + 1) * width);
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                sb.Append(GetConsoleChar(arr[i,j]));
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
            int i = Random.Range(0, mapHeight);
            int j = Random.Range(0, mapWidth);
            if (tiles[i, j] == 0) {
                return new Vector2(j, i);
            }
        }
    }
}
