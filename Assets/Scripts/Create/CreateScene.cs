using System.Collections;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
//创建场景脚本
public class CreateScene : MonoBehaviour {
    // Use this for initialization
    void Awake () {
        //初始化各种模型的生成参数
        initCreatePara();
        //创建轨道
        CreateTrailer(GlobalVaribles.trailer_Para);
        //创建轨道对应的有向图
        GlobalVaribles.trailerGraph = TrailerGraph.CreateTrailer(
            GlobalVaribles.trailer_Para, GlobalVaribles.edges, GlobalVaribles.vertexes);
        //创建小车
        CreateCars();
    }
    //初始化参数
    public void initCreatePara()
    {
        float inCurveRadius = 9.5f;
        float outCurveRadius = 10.5f;
        float lineLength = 20.0f;
        Vector3 trailerSize = new Vector3(0.2f, 0.2f, 0.2f);
        //轨道参数
        GlobalVaribles.trailer_Para = new GlobalVaribles.Trailer_Para("Trailer", lineLength, inCurveRadius, outCurveRadius, trailerSize);
        //内轨道曲线参数
        float trailerHighPos = GlobalVaribles.trailer_Para.trailerSize.y / 2;//轨道离地高度
        GlobalVaribles.in_curve_Para_list = new List<GlobalVaribles.Curve_Para>();
        GlobalVaribles.in_curve_Para_list.Add(new GlobalVaribles.Curve_Para
            (-90.0f, 90.0f, GlobalVaribles.trailer_Para.inCurveRadius, new Vector3(0, trailerHighPos, lineLength), GlobalVaribles.trailer_Para.trailerSize, "UpCurve_in"));
        GlobalVaribles.in_curve_Para_list.Add(new GlobalVaribles.Curve_Para
            (90.0f, 270.0f, GlobalVaribles.trailer_Para.inCurveRadius, new Vector3(0, trailerHighPos, 0), GlobalVaribles.trailer_Para.trailerSize, "MidCurve_in"));
        GlobalVaribles.in_curve_Para_list.Add(new GlobalVaribles.Curve_Para
            (90.0f, 270.0f, GlobalVaribles.trailer_Para.inCurveRadius, new Vector3(0, trailerHighPos, -lineLength), GlobalVaribles.trailer_Para.trailerSize, "DownCurve_in"));
        //直线参数，一截长度是20
        GlobalVaribles.in_line_Para_list = new List<GlobalVaribles.Line_Para>();
        GlobalVaribles.in_line_Para_list.Add(new GlobalVaribles.Line_Para(new Vector3(GlobalVaribles.trailer_Para.trailerSize.z, trailerHighPos, lineLength),
            new Vector3(-inCurveRadius, trailerHighPos, lineLength / 2), "Line1_in"));
        GlobalVaribles.in_line_Para_list.Add(new GlobalVaribles.Line_Para(new Vector3(GlobalVaribles.trailer_Para.trailerSize.z, trailerHighPos, lineLength),
            new Vector3(inCurveRadius, trailerHighPos, lineLength / 2), "Line2_in"));
        GlobalVaribles.in_line_Para_list.Add(new GlobalVaribles.Line_Para(new Vector3(GlobalVaribles.trailer_Para.trailerSize.z, trailerHighPos, lineLength),
            new Vector3(-inCurveRadius, trailerHighPos, -lineLength / 2), "Line3_in"));
        GlobalVaribles.in_line_Para_list.Add(new GlobalVaribles.Line_Para(new Vector3(GlobalVaribles.trailer_Para.trailerSize.z, trailerHighPos, lineLength),
            new Vector3(inCurveRadius, trailerHighPos, -lineLength / 2), "Line4_in"));

        //初始化外轨道参数
        //内轨道曲线参数
        GlobalVaribles.out_curve_Para_list = new List<GlobalVaribles.Curve_Para>();
        GlobalVaribles.out_curve_Para_list.Add(new GlobalVaribles.Curve_Para
            (-90.0f, 90.0f, GlobalVaribles.trailer_Para.outCurveRadius, new Vector3(0, trailerHighPos, lineLength), GlobalVaribles.trailer_Para.trailerSize, "UpCurve_out"));
        GlobalVaribles.out_curve_Para_list.Add(new GlobalVaribles.Curve_Para
            (90.0f, 270.0f, GlobalVaribles.trailer_Para.outCurveRadius, new Vector3(0, trailerHighPos, 0), GlobalVaribles.trailer_Para.trailerSize, "MidCurve_out"));
        GlobalVaribles.out_curve_Para_list.Add(new GlobalVaribles.Curve_Para
            (90.0f, 270.0f, GlobalVaribles.trailer_Para.outCurveRadius, new Vector3(0, trailerHighPos, -lineLength), GlobalVaribles.trailer_Para.trailerSize, "DownCurve_out"));
        //直线参数，一截长度是20
        GlobalVaribles.out_line_Para_list = new List<GlobalVaribles.Line_Para>();
        GlobalVaribles.out_line_Para_list.Add(new GlobalVaribles.Line_Para(new Vector3(GlobalVaribles.trailer_Para.trailerSize.z, trailerHighPos, lineLength),
            new Vector3(-outCurveRadius, trailerHighPos, lineLength / 2), "Line1_out"));
        GlobalVaribles.out_line_Para_list.Add(new GlobalVaribles.Line_Para(new Vector3(GlobalVaribles.trailer_Para.trailerSize.z, trailerHighPos, lineLength),
            new Vector3(outCurveRadius, trailerHighPos, lineLength / 2), "Line2_out"));
        GlobalVaribles.out_line_Para_list.Add(new GlobalVaribles.Line_Para(new Vector3(GlobalVaribles.trailer_Para.trailerSize.z, trailerHighPos, lineLength),
            new Vector3(-outCurveRadius, trailerHighPos, -lineLength / 2), "Line3_out"));
        GlobalVaribles.out_line_Para_list.Add(new GlobalVaribles.Line_Para(new Vector3(GlobalVaribles.trailer_Para.trailerSize.z, trailerHighPos, lineLength),
            new Vector3(outCurveRadius, trailerHighPos, -lineLength / 2), "Line4_out"));

        //初始化图参数
        float centerRadius = (inCurveRadius + outCurveRadius) / 2;
        float graphCurveLength = 2 * Mathf.PI * centerRadius;
        float graphLineLength = lineLength;
        GlobalVaribles.vertexes = new List<TrailerGraph.Vertex>{
            new TrailerGraph.Vertex(0, "", new Vector3(-centerRadius, trailerHighPos, graphLineLength)),
            new TrailerGraph.Vertex(1, "", new Vector3(centerRadius, trailerHighPos, graphLineLength)),
            new TrailerGraph.Vertex(2, "", new Vector3(centerRadius, trailerHighPos, 0)),
            new TrailerGraph.Vertex(3, "", new Vector3(centerRadius, trailerHighPos, -graphLineLength)),
            new TrailerGraph.Vertex(4, "", new Vector3(-centerRadius, trailerHighPos, -graphLineLength)),
            new TrailerGraph.Vertex(5, "", new Vector3(-centerRadius, trailerHighPos, 0))
        };
        GlobalVaribles.edges = new List<TrailerGraph.Edge>{
            new TrailerGraph.Edge(0, 1, LineType.Curve, graphCurveLength, new Vector3(0, trailerHighPos, lineLength), -90, 90),
            new TrailerGraph.Edge(1, 2, LineType.Straight, graphLineLength, new Vector3(0,0,0), 0, 0),
            new TrailerGraph.Edge(2, 3, LineType.Straight, graphLineLength, new Vector3(0,0,0), 0, 0),
            new TrailerGraph.Edge(3, 4, LineType.Curve, graphCurveLength, new Vector3(0, trailerHighPos, -lineLength), 90, 270),
            new TrailerGraph.Edge(4, 5, LineType.Straight, graphLineLength, new Vector3(0,0,0), 0, 0),
            new TrailerGraph.Edge(5, 0, LineType.Straight, graphLineLength, new Vector3(0,0,0), 0, 0),
            new TrailerGraph.Edge(2, 5, LineType.Curve, graphCurveLength, new Vector3(0, 0, 0), 90, 270)
        };

        //初始化车的参数
        float trailerWidth = outCurveRadius - inCurveRadius + trailerSize.x;
        Vector3 carSize = new Vector3(trailerWidth, trailerWidth, trailerWidth);
        float carHighPos = trailerSize.y + carSize.y / 2;
        GlobalVaribles.car_list = new List<Car>();

        #region //测试1
        //GlobalVaribles.car_list.Add(new Car("Car1", new Vector3(10, carHighPos, lineLength), carSize, Color.yellow,
        //    WorkState.Empty, 0, GlobalVaribles.edges[1], 0));
        //GlobalVaribles.car_list.Add(new Car("Car2", new Vector3(0, carHighPos, -(lineLength + centerRadius)), carSize, Color.black,
        //    WorkState.Empty, 0, GlobalVaribles.edges[3], 180));
        //GlobalVaribles.car_list.Add(new Car("Car3", new Vector3(-10, carHighPos, 0), carSize, Color.red,
        //    WorkState.Empty, 0, GlobalVaribles.edges[5], 0));
        #endregion

        #region //测试2
        //GlobalVaribles.car_list.Add(new Car("Car1", new Vector3(0, carHighPos, lineLength + centerRadius), carSize, Color.yellow,
        //   WorkState.Empty, 0, GlobalVaribles.edges[0], 0));
        //GlobalVaribles.car_list.Add(new Car("Car2", new Vector3(0, carHighPos, -(lineLength + centerRadius)), carSize, Color.black,
        //    WorkState.Empty, 0, GlobalVaribles.edges[3], 180));
        #endregion

        #region //测试3
        //GlobalVaribles.car_list.Add(new Car("Car1", new Vector3(0, carHighPos, lineLength + centerRadius), carSize, Color.yellow,
        //    WorkState.Empty, 0, GlobalVaribles.edges[0], 0));
        //GlobalVaribles.car_list.Add(new Car("Car2", new Vector3(-10, carHighPos, lineLength), carSize, Color.black,
        //    WorkState.Empty, 0, GlobalVaribles.edges[0], -90));
        #endregion

        #region //测试4
        //GlobalVaribles.car_list.Add(new Car("Car1", new Vector3(0, carHighPos, lineLength + centerRadius), carSize, Color.yellow,
        //    WorkState.Empty, 0, GlobalVaribles.edges[0], 0));
        #endregion

        #region //测试5
        GlobalVaribles.car_list.Add(new Car("Car1", new Vector3(0, carHighPos, lineLength + centerRadius), carSize, Color.yellow,
            WorkState.Empty, 0, GlobalVaribles.edges[0], 0));
        GlobalVaribles.car_list.Add(new Car("Car2", new Vector3(-10, carHighPos, 15), carSize, Color.black,
            WorkState.Empty, 0, GlobalVaribles.edges[5], 0));
        #endregion

        #region //测试6
        //GlobalVaribles.car_list.Add(new Car("Car1", new Vector3(0, carHighPos, -30), carSize, Color.yellow,
        //    WorkState.Empty, 0, GlobalVaribles.edges[3], 180));
        //GlobalVaribles.car_list.Add(new Car("Car2", new Vector3(-10, carHighPos, -10), carSize, Color.black,
        //    WorkState.Empty, 0, GlobalVaribles.edges[4], 0));
        //GlobalVaribles.car_list.Add(new Car("Car3", new Vector3(0, carHighPos, 30), carSize, Color.red,
        //    WorkState.Empty, 0, GlobalVaribles.edges[0], 0));
        #endregion
    }
    //生成整个轨道模型
    public void CreateTrailer(GlobalVaribles.Trailer_Para trailer_Para)
    {
        GameObject trailer = CreateEmpty(trailer_Para.trailerName);
        GameObject in_trailer = new GameObject("in_trailer");
        GameObject out_trailer = new GameObject("out_trailer");
        in_trailer.transform.parent = trailer.transform;
        out_trailer.transform.parent = trailer.transform;
        //创建内轨道
        //曲线
        GameObject CurveGroup_in = new GameObject("CurveGroup_in");
        CurveGroup_in.transform.parent = in_trailer.transform;
        for (int i = 0; i < GlobalVaribles.in_curve_Para_list.Count; i++)
        {
            GameObject curve = CreateModel.CreateCurve(GlobalVaribles.in_curve_Para_list[i]);
            curve.transform.parent = CurveGroup_in.transform;
        }
        //直线组
        GameObject LineGroup_in = new GameObject("LineGroup_in");
        LineGroup_in.transform.parent = in_trailer.transform;
        for (int i = 0; i < GlobalVaribles.in_line_Para_list.Count; i++)
        {
            GameObject cube = CreateModel.CreateLine(GlobalVaribles.in_line_Para_list[i]);
            cube.transform.parent = LineGroup_in.transform;
        }
        //创建外轨道
        //曲线组
        GameObject CurveGroup_out = new GameObject("CurveGroup_out");
        CurveGroup_out.transform.parent = out_trailer.transform;
        for (int i = 0; i < GlobalVaribles.out_curve_Para_list.Count; i++)
        {
            GameObject curve = CreateModel.CreateCurve(GlobalVaribles.out_curve_Para_list[i]);
            curve.transform.parent = CurveGroup_out.transform;
        }
        //直线
        GameObject LineGroup_out = new GameObject("LineGroup_out");
        LineGroup_out.transform.parent = out_trailer.transform;
        for (int i = 0; i < GlobalVaribles.out_line_Para_list.Count; i++)
        {
            GameObject cube = CreateModel.CreateLine(GlobalVaribles.out_line_Para_list[i]);
            cube.transform.parent = LineGroup_out.transform;
        }

        //ModelTool.CreatePrefabObj(trailer, "Trailer");
    }
    //生成所有小车
    public void CreateCars()
    {
        //创建车组
        GameObject carGroup = new GameObject();
        carGroup.name = "CarGroup";
        for (int i = 0; i < GlobalVaribles.car_list.Count; i++)
        {
            GameObject carObj = CreateModel.CreateCar(GlobalVaribles.car_list[i]);
            //更新allCars和allocableCars数组
            GlobalVaribles.allCars.Add(GlobalVaribles.car_list[i]);
            GlobalVaribles.allocableCars.Add(GlobalVaribles.car_list[i]);
            carObj.transform.parent = carGroup.transform;
        }
    }
    //创建空物体
    public GameObject CreateEmpty(string modelName)
    {
        GameObject obj = new GameObject();
        obj.name = modelName;
        return obj;
    }
}

