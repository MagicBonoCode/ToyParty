using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MatchScene : BaseScene
{
    public Dictionary<int, LevelData> LevelDataDic { get; private set; } = new Dictionary<int, LevelData>();
    public int CurrentLevel { get; private set; } = 1;

    public override void Initialize()
    {
        SceneType = EScene.Match;

        Managers.Scene.CurrentScene = this;

        // Resource Load
        Managers.Resource.LoadAllAsync<Object>("Default", (key, count, totalcout) =>
        {
            if(count == totalcout)
            {
                Object eventSystem = FindAnyObjectByType(typeof(EventSystem));
                if(eventSystem == null)
                {
                    eventSystem = Managers.Resource.Instantiate("EventSystem");
                    eventSystem.name = "EventSystem";
                    DontDestroyOnLoad(eventSystem);
                }

                // InputController 생성
                Managers.Resource.Instantiate("InputController");

                // TODO: 추후 데이터 관리는 별도로 할것.. 일단 임시로 여기서 설정합시다.
                LevelDataDic.Add(1, Managers.Resource.Load<LevelData>("LevelData_1"));
                LevelDataDic.Add(2, Managers.Resource.Load<LevelData>("LevelData_2"));

                CurrentLevel = 1;

                Managers.Match.Initialize(LevelDataDic[CurrentLevel]);

                UI_MatchScene uiMatchScene = Managers.UI.ShowSceneUI<UI_MatchScene>("UI_MatchScene");
                uiMatchScene.SetInfo();
            }
        });
    }

    public override void Clear()
    {
    }

    #region Game

    public override void SuccessGame()
    {
        base.SuccessGame();

        CurrentLevel++;
        Managers.Match.Initialize(LevelDataDic[CurrentLevel]);
    }

    public override void FailGame()
    {
        base.FailGame();

        Managers.Match.Initialize(LevelDataDic[CurrentLevel]);
    }

    #endregion
}
