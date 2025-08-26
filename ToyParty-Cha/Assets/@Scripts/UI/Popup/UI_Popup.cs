using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UI_Popup : UI_Base
{
    protected override void Awake()
    {
        Managers.UI.SetCanvas(gameObject, true);
    }

    protected override void Start()
    {
    }

    public virtual void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI(this);
    }
}
