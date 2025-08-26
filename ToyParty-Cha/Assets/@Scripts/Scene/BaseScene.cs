using UnityEngine;
using UnityEngine.Rendering;

public abstract class BaseScene : MonoBehaviour
{
    public EScene SceneType { get; protected set; } = EScene.Match;

    private void Awake()
    {
        // Default Setting
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        // Initialize Scene
        Initialize();
    }

    public abstract void Initialize();

    public abstract void Clear();

    #region Game

    public virtual void SuccessGame()
    { 
    
    }

    public virtual void FailGame()
    { 
    
    }

    #endregion
}
