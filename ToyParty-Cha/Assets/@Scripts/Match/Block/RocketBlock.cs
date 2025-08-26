using System.Collections.Generic;
using UnityEngine;

public class RocketBlock : BoomBlock
{
    public void Init(int q, int r, BlockColor color)
    {
        Pos = new HexPos(q, r);
        Type = BlockType.Boom;
        Color = color;
        Boom = BoomType.Rocket;

        IsMatchable = true;

        IsRainbow = false;

        _spriteRenderer.sprite = GetSprite();
    }

    protected override void ActiveBlock()
    {
        base.ActiveBlock();

        // 로켓은 활성화 시 해당 방향의 모든 블록을 활성화 시킵니다.
        List<HexPos> activatePosList = Managers.Match.GetCollectLine(Pos, LineType.Vertical);
        activatePosList.Remove(Pos); // 자기 자신은 제외
        Managers.Match.ActivateBlockList(activatePosList);
    }

    protected override void CompleteBlock()
    {
        base.CompleteBlock();

        // 로켓은 매치가 완료되면 삭제됩니다.
        Managers.Object.DestroyBlock(Pos);
    }

    protected override Sprite GetSprite()
    {
        switch(Color)
        {
            case BlockColor.Red:
                return Managers.Resource.Load<Sprite>("Rocket_Red");
            case BlockColor.Green:
                return Managers.Resource.Load<Sprite>("Rocket_Green");
            case BlockColor.Blue:
                return Managers.Resource.Load<Sprite>("Rocket_Blue");
            case BlockColor.Yellow:
                return Managers.Resource.Load<Sprite>("Rocket_Yellow");
            case BlockColor.Purple:
                return Managers.Resource.Load<Sprite>("Rocket_Purple");
            case BlockColor.Orange:
                return Managers.Resource.Load<Sprite>("Rocket_Orange");
            default:
                return null;
        }
    }
}
