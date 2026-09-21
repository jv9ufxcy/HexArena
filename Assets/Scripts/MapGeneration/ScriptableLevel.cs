using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableLevel : ScriptableObject
{
    public int LevelIndex;
    public List<SavedTile> GroundTiles;
    public List<SavedTile> UnitTiles;
    public List<SavedTile> PitTiles;
    public List<SavedTile> MonsterTiles;
    public List<SavedTile> ItemTiles;
    public List<SavedTile> PickupTiles;
    public List<SavedTile> DoorTiles;
    public List<SavedTile> PlayerTiles;
}
[Serializable]
public class SavedTile
{
    public Vector3Int Position;
    public LevelTile Tile;
}
