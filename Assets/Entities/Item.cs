using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Entity {
    
    Sprite sprite;
    string name;

    public Item(int x, int y, Sprite s = null, string name = "?") : base(x, y) {
        this.sprite = s;
        this.name = name;
    }
}
