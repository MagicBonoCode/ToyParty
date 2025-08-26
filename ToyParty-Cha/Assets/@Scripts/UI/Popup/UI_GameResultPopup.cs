using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_GameResultPopup : UI_Popup
{
    public enum GameObjects
    {
        Panel_Success,
        Panel_Fail,
    }

    public enum Buttons
    {
        Button_Success,
        Button_Fail,
    }

    protected override void Awake()
    {
        base.Awake();

        BindGameObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));

        BindEvent(GetButton((int)Buttons.Button_Success).gameObject, OnClickSuccessButton);
        BindEvent(GetButton((int)Buttons.Button_Fail).gameObject, OnClickFailButton);
    }

    public void SetInfo(bool isSuccess)
    {
        GetGameObject((int)GameObjects.Panel_Success).SetActive(isSuccess);
        GetGameObject((int)GameObjects.Panel_Fail).SetActive(!isSuccess);
    }

    #region UI Event

    private void OnClickSuccessButton(PointerEventData data)
    {
        Managers.Scene.CurrentScene.SuccessGame();
        ClosePopupUI();
    }

    private void OnClickFailButton(PointerEventData data)
    {
        Managers.Scene.CurrentScene.FailGame();
        ClosePopupUI();
    }

    #endregion
}
