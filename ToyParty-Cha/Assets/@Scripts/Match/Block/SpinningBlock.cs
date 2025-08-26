using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningBlock : Block
{
    public int SpinCount { get; private set; } = 1;  // ȸ�� Ƚ��

    public void Init(int q, int r)
    {
        Pos = new HexPos(q, r);
        Type = BlockType.Spinning;
        Color = BlockColor.None;

        IsMatchable = false;

        IsRainbow = false;

        SpinCount = 1;

        _spriteRenderer.sprite = GetSprite();
    }

    protected override void ActiveBlock()
    {
        base.ActiveBlock();

        // ���̴� Ȱ��ȭ ȿ���� ����..
    }

    private void LateUpdate()
    {
        if(SpinCount == 0)
        {
            _spriteRenderer.transform.Rotate(0.0f, 0.0f, 360.0f * Time.deltaTime);
        }
    }

    protected override void CompleteBlock()
    {
        base.CompleteBlock();

        if(SpinCount > 0)
        {
            SpinCount--;
            return;
        }

        // TODO: �̼�ó���ϴ� Ŭ���� ������ �ۼ��Ұ�..
        foreach(MissionData mission in Managers.Match.MissionList)
        { 
            if(mission.Type == MissionType.Spinning && mission.Count != 0)
            {
                mission.Count--;
                Managers.Event.TriggerEvent(EEventType.Update_Mission);
            }
        }
        if(Managers.Match.IsCleared)
        {
            UI_GameResultPopup popup = Managers.UI.ShowPopupUI<UI_GameResultPopup>();
            popup.SetInfo(true);
        }

        // ���̴� ��ġ�� �Ϸ�Ǹ� �����˴ϴ�.
        Managers.Object.DestroyBlock(Pos);
    }

    protected override Sprite GetSprite()
    {
        return Managers.Resource.Load<Sprite>("Spinning");
    }
}
