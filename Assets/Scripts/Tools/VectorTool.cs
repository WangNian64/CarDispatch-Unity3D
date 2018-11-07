using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorTool{
    //向量有关运算
    //-
    public static Vector3 vectorSub(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }
    //+
    public static Vector3 vectorAdd(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }
    //两个点的中心点
    public static Vector3 centerVector(Vector3 v1, Vector3 v2)
    {
        return new Vector3((v1.x + v2.x) / 2, (v1.y + v2.y) / 2, (v1.z + v2.z) / 2);
    }
    //得到2向量之间的角度
    public static float get2VectorAngle(Vector3 from_, Vector3 to_)
    {
        Vector3 v3 = Vector3.Cross(from_, to_);
        if (v3.z > 0)
            return Vector3.Angle(from_, to_);
        else
            return 360 - Vector3.Angle(from_, to_);
    }
    //判断两点之间是否足够接近
    //只要两点之间小于一帧运动的距离则为true
    public static bool IsCloseEnough(Vector3 v1, Vector3 v2, float speed)
    {
        float dist = Vector3.Distance(v1, v2);
        return dist <= speed * GlobalVaribles.frameTime;
    }
    public static void printVector3(Vector3 v)
    {
        Debug.Log(v.x + ", " + v.y + ", " + v.z);
    }
}
