using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//一次任务包括取货和放货两个过程
[System.Serializable]
public class Task{
    public Vector3 loadGoodsPos;
    public Vector3 unloadGoodsPos;
    public TrailerGraph.Edge loadGoods_Edge;
    public TrailerGraph.Edge unloadGoods_Edge;
    public float loadTime = 1.0f;
    public float unloadTime = 1.0f;
    public Task()
    {

    }
    public Task(Vector3 loadGoodsPos, Vector3 unloadGoodsPos, TrailerGraph.Edge loadGoods_Edge,
        TrailerGraph.Edge unloadGoods_Edge, float loadTime = 1.0f, float unloadTime = 1.0f)
    {
        this.loadGoodsPos = loadGoodsPos;
        this.unloadGoodsPos = unloadGoodsPos;
        this.loadGoods_Edge = loadGoods_Edge;
        this.unloadGoods_Edge = unloadGoods_Edge;
        this.loadTime = loadTime;
        this.unloadTime = unloadTime;
    }
    public void toString()
    {
        Debug.Log(loadGoodsPos + ", " + unloadGoodsPos);
    }
    public Task Clone()
    {
        if (this == null)
            return null;
        else
        {
            Task task = new Task(
                this.loadGoodsPos,
                this.unloadGoodsPos,
                this.loadGoods_Edge,
                this.unloadGoods_Edge,
                this.loadTime,
                this.unloadTime                
            );
            return task;
        }
    }
}
