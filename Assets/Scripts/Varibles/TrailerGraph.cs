using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//有向图中线段对应轨道的种类
public enum LineType
{
    Straight=0, Curve=1//直线和曲线
}
//轨道有向图的拓扑结构信息
public class TrailerGraph {
    //边
    [System.Serializable]
    public struct Edge
    {
        public int vertexNum1;//边的点序号1
        public int vertexNum2;//边的点序号2
        public LineType lineType;//边的类型
        public float weight;//边的权值（长度，曲线的弧长）
        public Vector3 curveCenter;//曲线的中心点
        //曲线的开始和结束角度
        public float startDeg;
        public float endDeg;
        public Edge(int v1, int v2, LineType lt, float w, Vector3 curveC, float sDeg, float eDeg)
        {
            vertexNum1 = v1;
            vertexNum2 = v2;
            lineType = lt;
            weight = w;
            curveCenter = curveC;
            startDeg = sDeg;
            endDeg = eDeg;
        }
    }
    //结点
    public class Vertex
    {
        public int index;
        public string data;//顶点数据
        public Vector3 vertexPos;//顶点的坐标
        public Vertex(int index, string data, Vector3 vertexPos)
        {
            this.index = index;
            this.data = data;
            this.vertexPos = vertexPos;
        }
    }
    //两点之间的最短路径数据
    public class PathData
    {
        public float shortLength;
        public List<Vertex> pathList;
        public PathData()
        {
            this.shortLength = GlobalVaribles.INFINITY;
            this.pathList = new List<Vertex>(100);
        }
        public PathData(float shortLength, List<Vertex> pathList)
        {
            this.pathList = pathList;
            this.shortLength = shortLength;
        }
        public void PrintPathData()
        {
            Debug.Log("pathData");
            for (int i = 0; i < this.pathList.Count; i++)
            {
                Debug.Log(pathList[i].index + ", ");
            }
        }
    }
    //图中所能包含的点上限
    public const int MAX_NUMBER = 100;
    //顶点数组
    public Vertex[] vertexes;
    //邻接矩阵
    public Edge[,] adjMatrix;
    //点数目
    public int numVertex = 0;
    //半径
    public float curveRadius;
    //最短路径
    public float[,] distMap;//最短路径长度矩阵
    public int[,] path;//最短路径节点矩阵

    public PathData[,] pathMap;//所有点的最短路径矩阵（每个点的数据都是一条路径列表）
    //初始化num个点的图
    public TrailerGraph(int num = MAX_NUMBER)
    {
        //初始化邻接矩阵和顶点数组
        adjMatrix = new Edge[num, num];
        vertexes = new Vertex[num];
        //将代表邻接矩阵的表全初始化为0
        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < num; j++)
            {
                adjMatrix[i, j].vertexNum1 = i;
                adjMatrix[i, j].vertexNum2 = j;
                adjMatrix[i, j].lineType = LineType.Straight;
                adjMatrix[i, j].weight = GlobalVaribles.INFINITY;//初始化weight为最大值
            }
        }
    }
    //向图中添加节点
    public void AddVertex(int i, string d, Vector3 pos)
    {
        vertexes[numVertex] = new Vertex(i, d, pos);
        numVertex++;
    }
    //向图中添加有向边,weight就是两点之间的距离
    public void AddEdge(int vertex1, int vertex2, LineType lineType, float weight, Vector3 curveCenter)
    {
        adjMatrix[vertex1, vertex2].vertexNum1 = vertex1;
        adjMatrix[vertex1, vertex2].vertexNum2 = vertex2;
        adjMatrix[vertex1, vertex2].lineType = lineType;
        adjMatrix[vertex1, vertex2].weight = weight;
        adjMatrix[vertex1, vertex2].curveCenter = curveCenter;
    }
    public void AddEdge(Edge edge)
    {
        adjMatrix[edge.vertexNum1, edge.vertexNum2] = edge;
    }
    //打印点信息
    public void PrintVertex(int vertexNum)
    {
        Debug.Log("点数据：" + this.vertexes[vertexNum].data + ", " 
            + "点坐标" + this.vertexes[vertexNum].vertexPos);
    }
    //生成一个轨道图的对象
    public static TrailerGraph CreateTrailer(GlobalVaribles.Trailer_Para trailer_Para, List<Edge> edges, List<Vertex> vertexes)
    {
        //生成轨道对应的图
        TrailerGraph trailerGraph = new TrailerGraph(vertexes.Count);
        trailerGraph.numVertex = vertexes.Count;
        //给点赋值（坐标）
        for (int i = 0; i < vertexes.Count; i++)
        {
            trailerGraph.vertexes[i] = new Vertex(vertexes[i].index, vertexes[i].data, vertexes[i].vertexPos);
        }
        //给边赋值（边的两点序号、边的种类（直还是曲）、边长
        for (int i = 0; i < vertexes.Count; i++)
        {
            trailerGraph.AddEdge(i, i, LineType.Straight, 0, new Vector3(0,0,0));
        }
        for (int i = 0; i < edges.Count; i++)
        {
            trailerGraph.AddEdge(edges[i]);
        }
        trailerGraph.curveRadius = (trailer_Para.inCurveRadius + trailer_Para.outCurveRadius) / 2;

        trailerGraph.distMap = new float[trailerGraph.numVertex, trailerGraph.numVertex];
        trailerGraph.path = new int[trailerGraph.numVertex, trailerGraph.numVertex];
        trailerGraph.pathMap = new PathData[trailerGraph.numVertex, trailerGraph.numVertex];
        for (int i = 0; i < trailerGraph.numVertex; i++)//必须这样初始化
        {
            for (int j = 0; j < trailerGraph.numVertex; j++)
            {
                trailerGraph.pathMap[i, j] = new PathData();
            }
        }
        //根据图来初始化最短路径
        trailerGraph.floyd();
        trailerGraph.getAllPathList();
        return trailerGraph;
    }
    //打印图的信息
    public void printGraph()
    {
        for (int i = 0; i < numVertex; i++)
        {
            for (int j = 0; j < numVertex; j++)
            {
                Debug.Log(i + ", " + j + ", "+ adjMatrix[i, j].weight);
            }
        }
    }
    //路径有关计算
    //计算所有点到所有其他点的最短路径
    public void floyd()
    {
        int i, j, k;
        //初始化路径数组
        for (i = 0; i < numVertex; i++)
        {
            for (j = 0; j < numVertex; j++)
            {
                distMap[i, j] = adjMatrix[i, j].weight;
                path[i, j] = -1;
            }
        }
        //计算最短路径
        for (k = 0; k < numVertex; k++)
        {
            for (i = 0; i < numVertex; i++)
            {
                for (j = 0; j < numVertex; j++)
                {
                    if (distMap[i, j] > distMap[i, k] + distMap[k, j])
                    {
                        distMap[i, j] = distMap[i, k] + distMap[k, j];
                        path[i, j] = k;
                    }
                }
            }
        }
    }
    //计算每个节点到其他所有点的路径列表
    public void getAllPathList()
    {
        int i, j;
        for (i = 0; i < numVertex; i++)
        {
            for (j = 0; j < numVertex; j++)
            {
                if (distMap[i, j] == GlobalVaribles.INFINITY)
                {
                    if (i != j)
                    {
                        pathMap[i, j] = null;
                    }
                }
                else
                {
                    pathMap[i, j].shortLength = distMap[i, j];
                    pathMap[i, j].pathList.Add(vertexes[i]);
                    getPathList(pathMap[i, j], i, j);
                    pathMap[i, j].pathList.Add(vertexes[j]);
                }
            }
        }
    }
    //计算一个点到其他所有点的路径列表
    public void getPathList(PathData pd, int i, int j)
    {

        int k;
        k = path[i, j];
        if (k == -1)
        {
            return;
        }
        getPathList(pd, i, k);
        pd.pathList.Add(vertexes[k]);
        getPathList(pd, k, j);
    }
    //打印
    public void printPathData(int i, int j)
    {
        Debug.Log(i + "到" + j + "的路径：");
        List<Vertex> pathList = pathMap[i, j].pathList;
        foreach (Vertex v in pathList)
        {
            Debug.Log(v.index + "-->");
        }
    }
}
