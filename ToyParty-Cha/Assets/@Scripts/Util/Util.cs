using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Util : MonoBehaviour
{
    public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if(component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    public static GameObject FindChild(GameObject gameObject, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(gameObject, name, recursive);
        if(transform == null)
        {
            return null;
        }

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject gameObject, string name = null, bool recursive = false) where T : Object
    {
        if(gameObject == null)
        {
            return null;
        }

        if(recursive == false)
        {
            for(int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform transform = gameObject.transform.GetChild(i);
                if(string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if(component != null)
                    {
                        return component;
                    }
                }
            }
        }
        else
        {
            foreach(T component in gameObject.GetComponentsInChildren<T>())
            {
                if(string.IsNullOrEmpty(name) || component.name == name)
                {
                    return component;
                }
            }
        }

        return null;
    }

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static int GetRandomInt(int min, int max)
    {
        return Random.Range(min, max + 1);
    }

    public static float GetRandomfloat(float min, float max)
    {
        return Random.Range(min, max + Mathf.Epsilon);
    }

    public static Sprite GetMissionIcon(MissionType type)
    {
        // TODO: 확장 하세용..
        return Managers.Resource.Load<Sprite>("Spinning");
    }
}
