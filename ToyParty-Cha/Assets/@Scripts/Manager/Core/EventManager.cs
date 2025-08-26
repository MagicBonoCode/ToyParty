using System;
using System.Collections.Generic;

public enum EEventType
{
    None,
    Update_Mission,
    Update_MoveCount,
}

public class EventManager
{
    private Dictionary<EEventType, Action> _event = new Dictionary<EEventType, Action>();

    public void Init()
    {
        _event.Clear();
    }

    public void AddEvent(EEventType eventType, Action listener)
    {
        Action thisEvent;
        if(_event.TryGetValue(eventType, out thisEvent))
        {
            thisEvent += listener;
            _event[eventType] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            _event.Add(eventType, thisEvent);
        }
    }

    public void RemoveEvent(EEventType eventType, Action listener)
    {
        if(_event == null)
        {
            return;
        }

        Action thisEvent;
        if(_event.TryGetValue(eventType, out thisEvent))
        {
            thisEvent -= listener;
            _event[eventType] = thisEvent;
        }
    }

    public void TriggerEvent(EEventType eventType)
    {
        Action thisEvent;
        if(_event.TryGetValue(eventType, out thisEvent))
        {
            thisEvent?.Invoke();
        }
    }

    public void Clear()
    {
        _event.Clear();
    }
}
