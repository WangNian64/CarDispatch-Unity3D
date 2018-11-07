using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GlobalVaribles{
    //模型参数结构体
    public struct Trailer_Para
    {
        public string trailerName;//名称
        public float lineLength;//直线轨道的长度（一段）
        public float inCurveRadius;//内轨道半径
        public float outCurveRadius;//外轨道半径
        public Vector3 trailerSize;//轨道的尺寸(这个尺寸是横截面的尺寸)
        public Trailer_Para(string trailerName, float lineLength, float inCurveRadius, float outCurveRadius, Vector3 trailerSize)
        {
            this.trailerName = trailerName;
            this.lineLength = lineLength;
            this.inCurveRadius = inCurveRadius;
            this.outCurveRadius = outCurveRadius;
            this.trailerSize = trailerSize;
        }
    }
    public struct Curve_Para
    {
        public float startDeg;
        public float endDeg;
        public float radius;
        public Vector3 centerPos;
        public Vector3 trailerSize;//轨道横截面的尺寸
        public string curveName;
        public Curve_Para(float startDeg, float endDeg, float radius, Vector3 centerPos, Vector3 trailerSize, string curveName)
        {
            this.startDeg = startDeg;
            this.endDeg = endDeg;
            this.radius = radius;
            this.centerPos = centerPos;
            this.trailerSize = trailerSize;
            this.curveName = curveName;
        }
    }
    public struct Line_Para
    {
        public Vector3 lineSize;
        public Vector3 lineCubePos;//中间直线cube的坐标
        public string lineName;
        public Line_Para(Vector3 lineSize, Vector3 lineCubePos, string lineName)
        {
            this.lineSize = lineSize;
            this.lineCubePos = lineCubePos;
            this.lineName = lineName;
        }
    }

    public static float INFINITY = 100000f;
    public static TrailerGraph trailerGraph;//轨道对应的有向图
    public static Trailer_Para trailer_Para;//轨道参数
    public static List<Curve_Para> in_curve_Para_list;//内轨道曲线参数
    public static List<Line_Para> in_line_Para_list;//内轨道直线参数

    public static List<Curve_Para> out_curve_Para_list;//外轨道曲线参数
    public static List<Line_Para> out_line_Para_list;//外轨道直线参数

    public static List<Car> car_list;//车的初始参数
    //图的参数                                 
    public static List<TrailerGraph.Edge> edges;
    public static List<TrailerGraph.Vertex> vertexes;

    //任务相关参数
    public static List<Task> allTasks = new List<Task>();//总任务列表
    public static List<Car> allCars = new List<Car>();//所有小车
    public static List<Car> allocableCars = new List<Car>();//所有可分配的车的信息数组
    public static List<Task> allocableTasks = new List<Task>();//所有可分配的任务

    public static float safeDis = 2.0f;//安全距离
    public static float moveSpeed = 10.0f;//移动速度
    public static float frameTime = 1.0f / 30.0f;//每帧时间，用于模拟计算任务完成总时间
}
