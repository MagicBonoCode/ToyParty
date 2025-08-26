using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectManager
{
    public Tile[,] Tiles { get; private set; }
    public Block[,] Blocks { get; private set; }

    public Transform TileParent { get; private set; }
    public Transform BlockParent { get; private set; }

    public List<GemBlock> GemBlockList => Blocks.Cast<Block>().Where(block => block != null && block.Type == Block.BlockType.Gem).Cast<GemBlock>().ToList();
    public List<BoomBlock> BoomBlockList => Blocks.Cast<Block>().Where(block => block != null && block.Type == Block.BlockType.Boom).Cast<BoomBlock>().ToList();
    public List<SpinningBlock> spinningBlocks => Blocks.Cast<Block>().Where(block => block != null && block.Type == Block.BlockType.Spinning).Cast<SpinningBlock>().ToList();

    public void SetTileAndBlock(int width, int height)
    {
        Tiles = new Tile[width, height];
        Blocks = new Block[width, height];

        if(TileParent == null)
        {
            GameObject tileParentObject = new GameObject("TileParent");
            TileParent = tileParentObject.transform;
        }

        if(BlockParent == null)
        {
            GameObject blockParentObject = new GameObject("BlockParent");
            BlockParent = blockParentObject.transform;
        }
    }

    public Tile SpawnTile(TileData data)
    {
        GameObject go = Managers.Resource.Instantiate("Tile", TileParent);
        go.name = $"Tile_{data.Q}_{data.R}";
        go.transform.localPosition = Managers.Match.AxialToWorld(data.Q, data.R);

        Tile tile = go.GetComponent<Tile>();
        tile.Init(data.Type, data.Color, data.IsSpawnPoint);

        Tiles[data.Q, data.R] = tile;

        return tile;
    }

    public Block SpawnBlock(BlockData data)
    {
        switch(data.Type)
        {
            case Block.BlockType.Gem:
                return SpawnGemBlock(data);
            case Block.BlockType.Boom:
                return SpawnBoomBlock(data);
            case Block.BlockType.Spinning:
                return SpawnSpinningBlock(data);

            default:
                return null;
        }
    }

    public GemBlock SpawnGemBlock(BlockData data)
    {
        return SpawnGemBlock(data.Q, data.R, data.Color);
    }

    public GemBlock SpawnGemBlock(HexPos pos, Block.BlockColor color)
    {
        return SpawnGemBlock(pos.Q, pos.R, color);
    }

    public GemBlock SpawnGemBlock(int q, int r, Block.BlockColor color)
    {
        GameObject go = Managers.Resource.Instantiate("GemBlock", BlockParent);
        go.name = $"Block_Gem_{color}";
        go.transform.localPosition = Managers.Match.AxialToWorld(q, r);

        GemBlock gemBlock = go.GetComponent<GemBlock>();
        gemBlock.Init(q, r, color);

        Blocks[q, r] = gemBlock;

        return gemBlock;
    }

    public BoomBlock SpawnBoomBlock(BlockData data)
    {
        return SpawnBoomBlock(data.Q, data.R, data.BoomType, data.Color);
    }

    public BoomBlock SpawnBoomBlock(HexPos pos, BoomBlock.BoomType boomType, Block.BlockColor color)
    {
        return SpawnBoomBlock(pos.Q, pos.R, boomType, color);
    }

    public BoomBlock SpawnBoomBlock(int q, int r, BoomBlock.BoomType boomType, Block.BlockColor color)
    {
        switch(boomType)
        {
            case BoomBlock.BoomType.Rocket:
                return SpawnRocketBlock(q, r, color);
            case BoomBlock.BoomType.Rainbow:
                return SpawnRainbowBlock(q, r);

            default:
                return null;
        }
    }

    public RocketBlock SpawnRocketBlock(HexPos pos, Block.BlockColor color)
    {
        return SpawnRocketBlock(pos.Q, pos.R, color);
    }

    public RocketBlock SpawnRocketBlock(int q, int r, Block.BlockColor color)
    {
        GameObject go = Managers.Resource.Instantiate("RocketBlock", BlockParent);
        go.name = $"Block_Rocket_{color}";
        go.transform.localPosition = Managers.Match.AxialToWorld(q, r);

        RocketBlock rocketBlock = go.GetComponent<RocketBlock>();
        rocketBlock.Init(q, r, color);

        Blocks[q, r] = rocketBlock;

        return rocketBlock;
    }

    public RainbowBlock SpawnRainbowBlock(HexPos pos)
    {
        return SpawnRainbowBlock(pos.Q, pos.R);
    }

    public RainbowBlock SpawnRainbowBlock(int q, int r)
    {
        GameObject go = Managers.Resource.Instantiate("RainbowBlock", BlockParent);
        go.name = $"Block_Rainbow";
        go.transform.localPosition = Managers.Match.AxialToWorld(q, r);

        RainbowBlock rainbowBlock = go.GetComponent<RainbowBlock>();
        rainbowBlock.Init(q, r);

        Blocks[q, r] = rainbowBlock;

        return rainbowBlock;
    }

    public SpinningBlock SpawnSpinningBlock(BlockData data)
    {
        return SpawnSpinningBlock(data.Q, data.R);
    }

    public SpinningBlock SpawnSpinningBlock(HexPos pos)
    {
        return SpawnSpinningBlock(pos.Q, pos.R);
    }

    public SpinningBlock SpawnSpinningBlock(int q, int r)
    {
        GameObject go = Managers.Resource.Instantiate("SpinningBlock", BlockParent);
        go.name = $"Block_Spinning";
        go.transform.localPosition = Managers.Match.AxialToWorld(q, r);

        SpinningBlock spinningBlock = go.GetComponent<SpinningBlock>();
        spinningBlock.Init(q, r);

        Blocks[q, r] = spinningBlock;

        return spinningBlock;
    }

    public bool DestroyBlock(HexPos pos)
    {
        return DestroyBlock(pos.Q, pos.R);
    }

    public bool DestroyBlock(int q, int r)
    {
        if(Blocks[q, r] == null)
        {
            return false;
        }

        Managers.Resource.Destroy(Blocks[q, r].gameObject);
        Blocks[q, r] = null;

        return true;
    }

    public void ClearAll()
    {
        if(Tiles != null)
        {
            for(int q = 0; q < Tiles.GetLength(0); q++)
            {
                for(int r = 0; r < Tiles.GetLength(1); r++)
                {
                    if(Tiles[q, r] != null)
                    {
                        Managers.Resource.Destroy(Tiles[q, r].gameObject);
                        Tiles[q, r] = null;
                    }
                }
            }
        }
        if(Blocks != null)
        {
            for(int q = 0; q < Blocks.GetLength(0); q++)
            {
                for(int r = 0; r < Blocks.GetLength(1); r++)
                {
                    if(Blocks[q, r] != null)
                    {
                        Managers.Resource.Destroy(Blocks[q, r].gameObject);
                        Blocks[q, r] = null;
                    }
                }
            }
        }
    }
}
