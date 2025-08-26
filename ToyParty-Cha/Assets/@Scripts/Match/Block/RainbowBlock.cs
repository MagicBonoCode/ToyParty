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

        // 레인보우는 ActiveRainbowBlock에서 처리..
    }

    protected override void CompleteBlock()
    {
        base.CompleteBlock();
    }

    public void ActiveRainbowBlock(BlockColor targetColor)
    {
        // 레인보우는 활성화 시 같은 색 블록을 모두 활성화 시킵니다.
        List<HexPos> activatePosList = Managers.Match.GetSameColorHexPosList(targetColor);
        Managers.Match.ActivateBlockList(activatePosList);

        // 레인보우는 매치가 완료되면 삭제됩니다.
        Managers.Object.DestroyBlock(Pos);
    }

    protected override Sprite GetSprite()
    {
        return Managers.Resource.Load<Sprite>("Rainbow");
    }
}
