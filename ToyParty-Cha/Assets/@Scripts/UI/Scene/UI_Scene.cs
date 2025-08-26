using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UI_Scene : UI_Base
{
    protected override void Awake()
    {
        Managers.UI.SetCanvas(gameObject, false);
    }

    protected override void Start()
    {
    }
}
