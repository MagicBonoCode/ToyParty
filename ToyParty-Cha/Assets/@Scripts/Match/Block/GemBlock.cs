using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemBlock : Block
{
    public void Init(int q, int r, BlockColor color)
    {
        Pos = new HexPos(q, r);
        Type = BlockType.Gem;
        Color = color;

        IsMatchable = true;

        IsRainbow = false;

        _spriteRenderer.sprite = GetSprite();
    }

    protected override void ActiveBlock()
    {
        base.ActiveBlock();

        // 잼은 활성화 효과가 없죠..
    }

    protected override void CompleteBlock()
    {
        base.CompleteBlock();

        // 잼은 매치가 완료되면 삭제됩니다.
        Managers.Object.DestroyBlock(Pos);
    }

    protected override Sprite GetSprite()
    {
        switch(Color)
        {
            case BlockColor.Red:
                return Managers.Resource.Load<Sprite>("Gem_Red");
            case BlockColor.Green:
                return Managers.Resource.Load<Sprite>("Gem_Green");
            case BlockColor.Blue:
                return Managers.Resource.Load<Sprite>("Gem_Blue");
            case BlockColor.Yellow:
                return Managers.Resource.Load<Sprite>("Gem_Yellow");
            case BlockColor.Purple:
                return Managers.Resource.Load<Sprite>("Gem_Purple");
            case BlockColor.Orange:
                return Managers.Resource.Load<Sprite>("Gem_Orange");

            default:
                return null;
        }
    }
}
