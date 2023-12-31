using UnityEngine;
using System;
using System.Reflection;

public class HitVariable : MonoBehaviour
{
    public string HitReferenceScript;

    public void SwitchAnim()
    {
        Type type = Type.GetType(HitReferenceScript);

        if (type != null)
        {
            Component hitComponent = GetRootParent(gameObject.transform).GetComponent(type);

            if (hitComponent != null)
            {
                MethodInfo method = type.GetMethod("SwitchAnim");

                if (method != null)
                {
                    method.Invoke(hitComponent, null);
                }
            }
        }
    }

    public void ShootEvent()
    {
        Type type = Type.GetType(HitReferenceScript);

        if (type != null)
        {
            Component hitComponent = GetRootParent(gameObject.transform).GetComponent(type);

            if (hitComponent != null)
            {
                MethodInfo method = type.GetMethod("ShootEvent");

                if (method != null)
                {
                    method.Invoke(hitComponent, null);
                }
            }
        }
    }

    private Transform GetRootParent(Transform child)
    {
        Transform parent = child.parent;

        while (parent != null)
        {
            child = parent;
            parent = child.parent;
        }

        return child;
    }
}
