using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateModel {
    //生成曲线轨道模型
    public static GameObject CreateCurve(GlobalVaribles.Curve_Para curve_Para)
    {
        GameObject curveObj = new GameObject();
        curveObj.name = curve_Para.curveName;
        int count = 100;
        float totalAngle = curve_Para.endDeg - curve_Para.startDeg;
        float changeAngle = totalAngle / count;

        float accumAngle = curve_Para.startDeg + changeAngle / 2;//角度累计
        Vector3 trailerSize = curve_Para.trailerSize;

        for (int i = 0; i < count; i++)
        {
            //建一个cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float cubeLength = 2 * Mathf.PI * curve_Para.radius * (totalAngle / 360) / count
                + Mathf.Tan(Mathf.Deg2Rad * (changeAngle / 2)) * trailerSize.z;
            float cubeX = curve_Para.centerPos.x + curve_Para.radius * Mathf.Sin(Mathf.Deg2Rad * accumAngle);
            float cubeZ = curve_Para.centerPos.z + curve_Para.radius * Mathf.Cos(Mathf.Deg2Rad * accumAngle);
            cube.transform.localScale = new Vector3(cubeLength, trailerSize.y, trailerSize.z);
            cube.transform.position = new Vector3(cubeX, trailerSize.y / 2, cubeZ);
            cube.transform.LookAt(curve_Para.centerPos);
            cube.transform.parent = curveObj.transform;
            cube.GetComponent<MeshRenderer>().material.color = Color.red;
            accumAngle += changeAngle;
        }
        return curveObj;
    }
    //生成直线轨道模型
    public static GameObject CreateLine(GlobalVaribles.Line_Para line_Para)
    {
        GameObject lineCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lineCube.name = line_Para.lineName;
        lineCube.transform.localScale = line_Para.lineSize;
        lineCube.transform.position = line_Para.lineCubePos;
        lineCube.GetComponent<MeshRenderer>().material.color = Color.blue;
        return lineCube;
    }
    //生成车
    public static GameObject CreateCar(Car car)
    {
        GameObject carCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        carCube.name = car.carName;
        carCube.transform.localScale = car.carSize;
        carCube.transform.position = car.carPos;
        carCube.GetComponent<MeshRenderer>().material.color = car.carColor;
        carCube.layer = LayerMask.NameToLayer("Car");
        carCube.AddComponent<ShowCarMessage>();
        carCube.GetComponent<ShowCarMessage>().carMessage = car;
        carCube.AddComponent<Rigidbody>();
        carCube.GetComponent<Rigidbody>().isKinematic = true;
        carCube.AddComponent<CarController>();
        return carCube;
    }
    //生成任务点标记，便于测试
    public static void CreateTaskPoint(Task task)
    {
        if (task != null)
        {
            GameObject loadPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            loadPoint.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            //Color color = ModelTool.getRandomColor();
            Color color = Color.black;
            loadPoint.GetComponent<MeshRenderer>().material.color = color;
            loadPoint.transform.localPosition = task.loadGoodsPos;
            loadPoint.AddComponent<Text>().text = Convert.ToString(task.taskID);
            loadPoint.name = "Task" + task.taskID + "_loadPoint";

            GameObject unloadPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            unloadPoint.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            unloadPoint.GetComponent<MeshRenderer>().material.color = color;
            unloadPoint.transform.localPosition = task.unloadGoodsPos;
            unloadPoint.AddComponent<Text>().text = Convert.ToString(task.taskID);
            unloadPoint.name = "Task" + task.taskID + "_unloadPoint";
        }
    }

}
