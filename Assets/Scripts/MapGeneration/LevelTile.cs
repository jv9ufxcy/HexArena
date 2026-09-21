using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(fileName = "New Level Tile", menuName = "2D/Tiles/Level Tile")]
public class LevelTile : Tile
{
    public TileType Type;
}
[Serializable]
public enum TileType
{
    //Enviro
    Ground = 0,
    //Collision
    Wall = 100,
    Pit = 101,

    //Actor
    Monster = 1000,
    Item = 1001,
    Player = 1002,
    Pickup = 1003,
    Door = 1004,

}