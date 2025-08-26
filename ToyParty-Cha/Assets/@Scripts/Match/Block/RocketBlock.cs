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

        // ������ Ȱ��ȭ �� �ش� ������ ��� ����� Ȱ��ȭ ��ŵ�ϴ�.
        List<HexPos> activatePosList = Managers.Match.GetCollectLine(Pos, LineType.Vertical);
        activatePosList.Remove(Pos); // �ڱ� �ڽ��� ����
        Managers.Match.ActivateBlockList(activatePosList);
    }

    protected override void CompleteBlock()
    {
        base.CompleteBlock();

        // ������ ��ġ�� �Ϸ�Ǹ� �����˴ϴ�.
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
