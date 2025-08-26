using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using TMPro;

public enum EUIEvent
{
    Click,
    Enter,
    Exit
}

public abstract class UI_Base : MonoBehaviour
{
    protected Dictionary<Type, Object[]> _objects = new Dictionary<Type, Object[]>();

    protected abstract void Awake();
    protected abstract void Start();

    protected void Bind<T>(Type type) where T : Object
    {
        string[] names = Enum.GetNames(type);
        Object[] objects = new Object[names.Length];
        _objects.Add(typeof(T), objects);

        for(int i = 0; i < names.Length; i++)
        {
            if(typeof(T) == typeof(GameObject))
            {
                objects[i] = Util.FindChild(gameObject, names[i], true);
            }
            else
            {
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);
            }

            if(objects[i] == null)
            {
                Debug.LogError($"Failed to Bind : {names[i]}");
            }
        }
    }

    protected void BindGameObjects(Type type) { Bind<GameObject>(type); }
    protected void BindTexts(Type type) { Bind<TMP_Text>(type); }
    protected void BindImages(Type type) { Bind<Image>(type); }
    protected void BindButtons(Type type) { Bind<Button>(type); }
    protected void BindToggles(Type type) { Bind<Toggle>(type); }
    protected void BindSliders(Type type) { Bind<Slider>(type); }
    protected void BindDropdowns(Type type) { Bind<TMP_Dropdown>(type); }
    protected void BindInputFields(Type type) { Bind<TMP_InputField>(type); }

    protected T Get<T>(int idx) where T : Object
    {
        Object[] objects = null;
        if(!_objects.TryGetValue(typeof(T), out objects))
        {
            return null;
        }

        return objects[idx] as T;
    }

    protected GameObject GetGameObject(int idx) { return Get<GameObject>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Toggle GetToggle(int idx) { return Get<Toggle>(idx); }
    protected Slider GetSlider(int idx) { return Get<Slider>(idx); }
    protected TMP_Dropdown GetDropdown(int idx) { return Get<TMP_Dropdown>(idx); }
    protected TMP_InputField GetInputField(int idx) { return Get<TMP_InputField>(idx); }

    public static void BindEvent(GameObject gameObejct, Action<PointerEventData> action, EUIEvent type = EUIEvent.Click)
    {
        UI_EventHandler eventHandler = gameObejct.GetOrAddComponent<UI_EventHandler>();

        switch(type)
        {
            case EUIEvent.Click:
                eventHandler.OnClickHandler -= action;
                eventHandler.OnClickHandler += action;
                break;
            case EUIEvent.Enter:
                eventHandler.OnEnterHandler -= action;
                eventHandler.OnEnterHandler += action;
                break;
            case EUIEvent.Exit:
                eventHandler.OnExitHandler -= action;
                eventHandler.OnExitHandler += action;
                break;

            default:
                break;
        }
    }
}
