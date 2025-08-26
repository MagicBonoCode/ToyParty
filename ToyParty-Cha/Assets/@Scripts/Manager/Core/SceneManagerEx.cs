using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EScene
{
    None,
    Match,
}

public class SceneManagerEx
{
    public BaseScene CurrentScene { get; set; }
    
    public void LoadScene(EScene type, bool isAsync = true, Action onCompleted = null)
    {
        Managers.Clear();

        if (isAsync)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GetSceneName(type));
            asyncOperation.completed += (AsyncOperation obj) =>
            {
                onCompleted?.Invoke();
            };
        }
        else
        {
            SceneManager.LoadScene(GetSceneName(type));
            onCompleted?.Invoke();
        }
    }

    private string GetSceneName(EScene type)
    {
        string name = Enum.GetName(typeof(EScene), type);
        return name;
    }
}
