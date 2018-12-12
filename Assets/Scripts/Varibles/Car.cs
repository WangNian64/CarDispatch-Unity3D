using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//小车的工作状态
public enum WorkState
{
    Empty=0, WayToLoad=1, Loading=2, WayToUnload=3, Unloading=4
}
[System.Serializable]
public class Car{
    public string carName;
    public Vector3 carPos;
    public Vector3 carSize;
    public Color carColor;
    public WorkState workState;
    public float speed;
    public TrailerGraph.Edge carEdge;//小车所在的边的信息
    public float angled;//若小车在曲线上，该变量表示小车在曲线上的角度  
    public Car()
    {

    }
    public Car(string carName, Vector3 carPos, Vector3 carSize, Color carColor, WorkState workState,
        float speed, TrailerGraph.Edge carEdge, float angled)
    {
        this.carName = carName;
        this.carPos = carPos;
        this.carSize = carSize;
        this.carColor = carColor;
        this.workState = workState;
        this.speed = speed;
        this.angled = angled;
        this.carEdge = carEdge;
    }
    public void toString()
    {
        Debug.Log(carName + ", " + carPos + ", " + carEdge.vertexNum1 + ", " + carEdge.vertexNum2 + ", " + workState);
    }
    public Car Clone()
    {
        if (this == null)
        {
            return null;
        }
        else
        {
            Car carCopy = new Car(
                this.carName,
                this.carPos,
                this.carSize,
                this.carColor,
                this.workState,
                this.speed,
                this.carEdge,
                this.angled
            );
            return carCopy;
        }
    }
}

