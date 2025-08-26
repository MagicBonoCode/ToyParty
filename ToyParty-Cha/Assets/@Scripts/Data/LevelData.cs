using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MissionData
{
    public MissionType Type;
    public int Count;
}

[Serializable]
public class TileData
{
    public int Q;
    public int R;
    public Tile.TileType Type;
    public Tile.TileColor Color;
    public bool IsSpawnPoint;
}

[Serializable]
public class BlockData
{
    public int Q;
    public int R;
    public Block.BlockType Type;
    public Block.BlockColor Color;

    // TODO: 폭탄의 경우 *추후 확장되면 별도의 데이터로 관리하는게 좋쵸..
    public BoomBlock.BoomType BoomType;
}

[CreateAssetMenu(fileName="LevelData", menuName="Match3/LevelData")]
public class LevelData : ScriptableObject
{
    public int MaxWidth;
    public int MaxHeight;
    public int Level;
    public List<MissionData> MissionList;
    public List<TileData> TileList;
    public List<BlockData> BlockList;
}
