using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MatchScene : UI_Scene
{
    public enum GameObjects
    {
        Content_MissionSlot,
    }

    public enum Texts
    {
        Text_MoveCount,
    }

    private List<UI_MissionSlot> _slotList = new List<UI_MissionSlot>();

    protected override void Awake()
    {
        base.Awake();

        BindGameObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
    }

    private void OnEnable()
    {
        Managers.Event.AddEvent(EEventType.Update_MoveCount, OnMoveCountUpdate);
        Managers.Event.AddEvent(EEventType.Update_Mission, UpdateMissionSlot);
    }

    private void OnDisable()
    {
        Managers.Event.RemoveEvent(EEventType.Update_MoveCount, OnMoveCountUpdate);
        Managers.Event.RemoveEvent(EEventType.Update_Mission, UpdateMissionSlot);
    }

    public void SetInfo()
    {
        GetText((int)Texts.Text_MoveCount).text = Managers.Match.MoveCount.ToString();

        Transform content = GetGameObject((int)GameObjects.Content_MissionSlot).transform;
        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach(MissionData mission in Managers.Match.MissionList)
        {
            UI_MissionSlot slot = Managers.UI.MakeSubItem<UI_MissionSlot>(content);
            slot.SetInfo(Util.GetMissionIcon(mission.Type), mission.Count);
            _slotList.Add(slot);
        }

        UpdateMissionSlot();
    }
    
    private void OnMoveCountUpdate()
    {
        GetText((int)Texts.Text_MoveCount).text = Managers.Match.MoveCount.ToString();
    }

    private void UpdateMissionSlot()
    {
        Transform content = GetGameObject((int)GameObjects.Content_MissionSlot).transform;
        int index = 0;
        foreach(MissionData mission in Managers.Match.MissionList)
        {
            _slotList[index].SetInfo(Util.GetMissionIcon(mission.Type), mission.Count);
            index++;
        }
    }
}
