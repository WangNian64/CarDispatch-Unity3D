using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//一次任务包括取货和放货两个过程
public class Task{
    public int taskID;
    public Vector3 loadGoodsPos;
    public Vector3 unloadGoodsPos;
    public TrailerGraph.Edge loadGoods_Edge;
    public TrailerGraph.Edge unloadGoods_Edge;
    public float loadTime = 1.0f;
    public float unloadTime = 1.0f;
    public Task()
    { }
    public Task(int taskID, Vector3 loadGoodsPos, Vector3 unloadGoodsPos, TrailerGraph.Edge loadGoods_Edge,
        TrailerGraph.Edge unloadGoods_Edge, float loadTime = 1.0f, float unloadTime = 1.0f)
    {
        this.taskID = taskID;
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
    //防止引用
    public Task Clone()
    {
        if (this == null)
            return null;
        else
        {
            Task task = new Task(
                this.taskID,
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

    public bool ContentEqual(Task t)
    {
        return 
            (taskID.Equals(t.taskID)) &&
            (loadGoodsPos.Equals(t.loadGoodsPos)) &&
            (unloadGoodsPos.Equals(t.unloadGoodsPos)) &&
            (loadGoods_Edge.Equals(t.loadGoods_Edge)) &&
            (unloadGoods_Edge.Equals(t.unloadGoods_Edge)) &&
            (loadTime.Equals(t.loadTime)) &&
            (unloadTime.Equals(t.unloadTime));
    }
}
