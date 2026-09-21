using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] private Tilemap _groundMap, _unitMap, _pitMap, _monsterMap, _itemMap, _pickupMap, _playerMap, _doorMap;
    [SerializeField] private int _levelIndex;
    public List<Vector3>monsterPositions = new List<Vector3>();
    public List<Vector3>itemPositions = new List<Vector3>();
    public List<Vector3>pickupPositions = new List<Vector3>();
    public List<Vector3>playerPositions = new List<Vector3>();
    public List<Vector3>doorPositions = new List<Vector3>();
    [SerializeField] private Spawner monsterSpawner;
#if UNITY_EDITOR
    public void SaveMap()
    {
        var newLevel = ScriptableObject.CreateInstance<ScriptableLevel>();

        newLevel.LevelIndex = _levelIndex;
        newLevel.name = $"Level {_levelIndex}";

        newLevel.GroundTiles = GetTilesFromMap(_groundMap).ToList();
        newLevel.UnitTiles = GetTilesFromMap(_unitMap).ToList();
        newLevel.PitTiles = GetTilesFromMap(_pitMap).ToList();
        newLevel.MonsterTiles = GetTilesFromMap(_monsterMap).ToList();
        newLevel.ItemTiles = GetTilesFromMap(_itemMap).ToList();
        newLevel.PickupTiles = GetTilesFromMap(_pickupMap).ToList();
        newLevel.DoorTiles = GetTilesFromMap(_doorMap).ToList();
        newLevel.PlayerTiles = GetTilesFromMap(_playerMap).ToList();

        ScriptableObjectUtility.SaveLevelFile(newLevel);

        IEnumerable<SavedTile> GetTilesFromMap(Tilemap map)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    var levelTile = map.GetTile<LevelTile>(pos);
                    yield return new SavedTile()
                    {
                        Position = pos,
                        Tile = levelTile
                    };
                }
            }
        }
    }
#endif
    public void ClearMap()
    {
        var maps = GetComponentsInChildren<Tilemap>();
        foreach (var tilemap in maps)
        {
            tilemap.ClearAllTiles();
        }
    }
    public void LoadLevel(int  levelIndex)
    {
        _levelIndex = levelIndex;
        LoadMap();
    }
    public void LoadMap()
    {
        var level = Resources.Load<ScriptableLevel>($"Levels/Level {_levelIndex}");
        if (level==null)
        {
            Debug.LogError($"Level {_levelIndex} does not exist.");
            return;
        }
        ClearMap();
        foreach (var savedTile in level.GroundTiles)
        {
            switch (savedTile.Tile.Type)
            {
                case TileType.Ground:
                    SetTile(_groundMap, savedTile);
                    break;
                default:
                    break;
            }
        }
        foreach (var savedTile in level.UnitTiles)
        {
            switch (savedTile.Tile.Type)
            {
                case TileType.Wall:
                    SetTile(_unitMap, savedTile);
                    break;
                default:
                    break;
            }
        }
        foreach (var savedTile in level.PitTiles)
        {
            switch (savedTile.Tile.Type)
            {
                case TileType.Pit:
                    SetTile(_pitMap, savedTile);
                    break;
                default:
                    break;
            }
        }
        Vector3 offset = new Vector3(0.5f, 0.5f, 0f);
        monsterPositions.Clear();
        foreach (var savedTile in level.MonsterTiles)
        {
            switch (savedTile.Tile.Type)
            {
                case TileType.Monster:
                    SetTile(_monsterMap, savedTile);
                    monsterPositions.Add(savedTile.Position + offset);
                    break;
                default:
                    break;
            }
        }
        itemPositions.Clear();
        foreach (var savedTile in level.ItemTiles)
        {
            switch (savedTile.Tile.Type)
            {
                case TileType.Item:
                    SetTile(_itemMap, savedTile);
                    itemPositions.Add(savedTile.Position + offset);
                    break;
                default:
                    break;
            }
        }
        pickupPositions.Clear();
        foreach (var savedTile in level.PickupTiles)
        {
            switch (savedTile.Tile.Type)
            {
                case TileType.Pickup:
                    SetTile(_pickupMap, savedTile);
                    pickupPositions.Add(savedTile.Position+offset);
                    break;
                default:
                    break;
            }
        }
        doorPositions.Clear();
        foreach (var savedTile in level.DoorTiles)
        {
            switch (savedTile.Tile.Type)
            {
                case TileType.Door:
                    SetTile(_doorMap, savedTile);
                    doorPositions.Add(savedTile.Position + offset);
                    break;
                default:
                    break;
            }
        }
        playerPositions.Clear();
        foreach (var savedTile in level.PlayerTiles)
        {
            switch (savedTile.Tile.Type)
            {
                case TileType.Player:
                    SetTile(_playerMap, savedTile);
                    playerPositions.Add(savedTile.Position + offset);
                    break;
                default:
                    break;
            }
        }
        
        void SetTile(Tilemap map, SavedTile tile)
        {
            map.SetTile(tile.Position, tile.Tile);
        }
    }
}
#if UNITY_EDITOR
public static class ScriptableObjectUtility
{
    public static void SaveLevelFile(ScriptableLevel level)
    {
        AssetDatabase.CreateAsset(level, $"Assets/Resources/Levels/{level.name}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
public struct Level
{
    public int LevelIndex;
    public List<SavedTile> GroundTiles;
    public List<SavedTile> UnitTiles;
    public List<SavedTile> PitTiles;
    public string Serialize()
    {
        var builder = new StringBuilder();

        builder.Append("g[");
        foreach (var groundTile in GroundTiles)
        {
            builder.Append($"{(int)groundTile.Tile.Type}({groundTile.Position.x},{groundTile.Position.y})");

        }
        builder.Append("]");
        return builder.ToString();
    }
}