using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
public class ModelTool{
    //创建prefab
    public static void CreatePrefabObj(GameObject obj, string prefabName)
    {
        string path = "Assets/Resources/Prefabs/" + prefabName + ".prefab";
        PrefabUtility.CreatePrefab(path, obj);
    }
    //返回一个随机颜色
    public static Color getRandomColor()
    {
        int iSeed = 10;
        System.Random ro = new System.Random(10);
        long tick = DateTime.Now.Ticks;
        System.Random ran = new System.Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

        int R = ran.Next(255);
        int G = ran.Next(255);
        int B = ran.Next(255);
        B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
        B = (B > 255) ? 255 : B;
        return new Color(R, G, B);
    }
}
