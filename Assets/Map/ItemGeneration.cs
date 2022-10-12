using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Text;

public class ItemGeneration : MonoBehaviour
{
    DungeonGeneration dungeonGeneration;
    public Tilemap itemTilemap;

    public GameObject sword;
    
    List<GameObject> itemList = new List<GameObject>();
    
    void Start()
    {
        dungeonGeneration = GetComponent<DungeonGeneration>();
        for (int _ = 0; _ < 10; _++) {
            SpawnRandomSword();
        }
    }


    void SpawnRandomSword() {
        (int, int) spot = dungeonGeneration.FindRandomOpenTileTuple();
        GameObject newSword = Instantiate(sword, new Vector3(spot.Item1 + 0.5f, spot.Item2 + 0.5f, 0), Quaternion.identity);
        itemList.Add(newSword);
    }
}
