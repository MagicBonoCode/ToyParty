using System.Collections.Generic;
using UnityEngine;

public class RainbowBlock : BoomBlock
{
    public void Init(int q, int r)
    {
        Pos = new HexPos(q, r);
        Type = BlockType.Boom;
        Color = BlockColor.None;
        Boom = BoomType.Rainbow;

        IsMatchable = false;

        IsRainbow = true;

        _spriteRenderer.sprite = GetSprite();
    }

    protected override void ActiveBlock()
    {
        base.ActiveBlock();

        // ���κ���� ActiveRainbowBlock���� ó��..
    }

    protected override void CompleteBlock()
    {
        base.CompleteBlock();
    }

    public void ActiveRainbowBlock(BlockColor targetColor)
    {
        // ���κ���� Ȱ��ȭ �� ���� �� ����� ��� Ȱ��ȭ ��ŵ�ϴ�.
        List<HexPos> activatePosList = Managers.Match.GetSameColorHexPosList(targetColor);
        Managers.Match.ActivateBlockList(activatePosList);

        // ���κ���� ��ġ�� �Ϸ�Ǹ� �����˴ϴ�.
        Managers.Object.DestroyBlock(Pos);
    }

    protected override Sprite GetSprite()
    {
        return Managers.Resource.Load<Sprite>("Rainbow");
    }
}
