using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

public class ResourceManager
{
    private Dictionary<string, Object> _resources = new Dictionary<string, Object>();

    #region Load

    public bool CheckResource<T>(string key) where T : Object
    {
        if (_resources.TryGetValue(key, out Object resource))
        {
            return true;
        }

        if (typeof(T) == typeof(Sprite))
        {
            key = key + ".sprite";
            if (_resources.TryGetValue(key, out Object temp))
            {
                return true;
            }
        }

        return false;
    }
    
    public T Load<T>(string key) where T : Object
    {
        if (_resources.TryGetValue(key, out Object resource))
        {
            return resource as T;
        }

        return null;
    }

    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        GameObject prefab = Load<GameObject>($"{key}");
        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab : {key}");
            return null;
        }

        if (pooling)
        { 
            return Managers.Pool.Pop(prefab);
        }

        GameObject gameObject = Object.Instantiate(prefab, parent);

        gameObject.name = prefab.name;
        return gameObject;
    }

    public void Destroy(GameObject gameObject)
    {
        if (gameObject == null)
        { 
            return;
        }

        if (Managers.Pool.Push(gameObject))
        { 
            return;
        }

        Object.Destroy(gameObject);
    }

    private IEnumerator ReserveDestroy(GameObject gameObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    #endregion

    #region Addressable

    public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        string loadKey = key;
        if (typeof(T) == typeof(Sprite))
        {
            loadKey = $"{key}[{key}]";
        }
        
        var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        asyncOperation.Completed += (op) =>
        {
            if (_resources.TryGetValue(key, out Object resource))
            {
                callback?.Invoke(op.Result);
                return;
            }

            _resources.Add(key, op.Result);
            callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;

            int totalCount = op.Result.Count;

            foreach (var result in op.Result)
            {
                if (result.ResourceType == typeof(Texture2D) ||  result.ResourceType == typeof(Sprite))
                {
                    LoadAsync<Sprite>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
                else
                { 
                    LoadAsync<T>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
            }
        };
    }

    #endregion
}
