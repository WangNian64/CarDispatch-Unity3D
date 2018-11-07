using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ModelTool{
    //创建prefab
    public static void CreatePrefabObj(GameObject obj, string prefabName)
    {
        string path = "Assets/Resources/Prefabs/" + prefabName + ".prefab";
        PrefabUtility.CreatePrefab(path, obj);
    }
    //

}
