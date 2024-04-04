using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MissingTypes : MonoBehaviour
{
    [UnityEditor.MenuItem("KripKode/Find Missing Script Objects")]
    static void TypeMissingLog()
    {
        string[] prefabPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)).ToArray();

        foreach (string prefabPath in prefabPaths)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
            if (obj != null)
            {
                Component[] components = obj.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        Debug.LogError("Missing Component in: " + prefabPath, obj);
                    }
                }
            }
        }
    }
}
