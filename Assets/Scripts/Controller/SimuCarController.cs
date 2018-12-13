using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//模拟小车
//信息和CarController一致
//不同在于更新的是carMessage的carPos位置
public class SimuCarController {
    public Car carMessage;//车的信息
    public Task task;//车对应的任务

    public float moveSpeed;//运动时的速度
    private float curveRadius;//曲线半径
    public float safeDis;//小车之间的安全距离

    private float loadAccumuTime;//累计取货时间
    private float unloadAccumuTime;//累计卸货时间
    private TrailerGraph.PathData carPathData;//小车到某个点的最短路径数据

    public bool speedChangeable;//速度是否可变
    //public bool SpeedChangeable
    //{
    //    get { return speedChangeable; }
    //    set { speedChangeable = value; }
    //}
    //构造函数
    public SimuCarController(Car carMessage, Task task, float moveSpeed, float curveRadius,
        float safeDis)
    {
        this.carMessage = carMessage;
        this.task = task;
        this.moveSpeed = moveSpeed;
        this.curveRadius = curveRadius;
        this.safeDis = safeDis;
        loadAccumuTime = unloadAccumuTime = 0;
        speedChangeable = true;
    }   
    //模拟update函数
    public void SimuUpdate()
    {
        if (carPathData != null)
        {
            if (carPathData.pathList[0].index == carPathData.pathList[1].index)
            {
                carPathData = null;
            }
        }
        SimuCarMovement();
        SimuAvoidCrash();
    }
    //模拟小车运动
    public void SimuCarMovement()
    {
        if (task != null)//有任务
        {
            switch (carMessage.workState)
            {
                case WorkState.Empty:
                    carMessage.workState = WorkState.WayToLoad;
                    carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum1,
                        task.loadGoods_Edge.vertexNum1];
                    if (speedChangeable)
                        carMessage.speed = moveSpeed;
                    break;
                case WorkState.WayToLoad:
                    gotoLoadPoint();//运动到取货点
                    break;
                case WorkState.Loading:
                    loadingGoods();//取货
                    break;
                case WorkState.WayToUnload:
                    gotoUnloadPoint();//运动到卸货点
                    break;
                case WorkState.Unloading:
                    unloadingGoods();//卸货
                    break;
                default:
                    break;
            }
        }
        else//此时车的状态一定是empty
        {
            switch (carMessage.workState)
            {
                case WorkState.Empty:
                    break;
                case WorkState.WayToLoad:
                    carMessage.workState = WorkState.Empty;
                    carMessage.speed = 0;
                    carPathData = null;
                    break;
                case WorkState.Loading:
                    break;
                case WorkState.WayToUnload:
                    break;
                case WorkState.Unloading:
                    break;
                default:
                    break;
            }
            if (carPathData != null)
            {
                int nextVertexNum = carMessage.carEdge.vertexNum2;//得到车要运动到的下一个节点
                TrailerGraph.Vertex nextVertex = GlobalVaribles.trailerGraph.vertexes[nextVertexNum];
                if (speedChangeable)
                    carMessage.speed = moveSpeed;
                gotoVertex(nextVertex);
            }
        }
    }
    //模拟运动到取货点
    public void gotoLoadPoint()
    {
        if (speedChangeable)
            carMessage.speed = moveSpeed;
        carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum1,
            task.loadGoods_Edge.vertexNum1];
        //若小车已经在最后一条边上
        if (carMessage.carEdge.vertexNum1 == task.loadGoods_Edge.vertexNum1)
        {
            //判断是否已经到达取货点
            Vector3 loadPos1 = new Vector3(task.loadGoodsPos.x,
                    task.loadGoodsPos.y + carMessage.carSize.y / 2, task.loadGoodsPos.z);
            if (carMessage.carPos == loadPos1)
            {
                carMessage.workState = WorkState.Loading;
                carMessage.speed = 0.0f;//之后车是静止的
            }
            else
            {
                gotoPos(task.loadGoodsPos);
            }
        }
        else
        {
            //得到车的下一个节点
            int nextVertexNum = carMessage.carEdge.vertexNum2;
            TrailerGraph.Vertex nextVertex = GlobalVaribles.trailerGraph.vertexes[nextVertexNum];
            gotoVertex(nextVertex);
        }
    }
    //模拟运动到卸货点
    public void gotoUnloadPoint()
    {
        if (speedChangeable)
            carMessage.speed = moveSpeed;
        carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum1,
            task.unloadGoods_Edge.vertexNum1];
        //若小车已经在最后一条边上
        if (carMessage.carEdge.vertexNum1 == task.unloadGoods_Edge.vertexNum1)
        {
            //判断是否已经到达卸货点
            Vector3 unloadPos1 = new Vector3(task.unloadGoodsPos.x,
                    task.unloadGoodsPos.y + carMessage.carSize.y / 2, task.unloadGoodsPos.z);
            if (carMessage.carPos == unloadPos1)
            {
                carMessage.workState = WorkState.Unloading;
                carMessage.speed = 0.0f;
            }
            else
            {
                gotoPos(task.unloadGoodsPos);
            }
        }
        else
        {
            //得到车的下一个节点
            int nextVertexNum = carMessage.carEdge.vertexNum2;
            TrailerGraph.Vertex nextVertex = GlobalVaribles.trailerGraph.vertexes[nextVertexNum];
            gotoVertex(nextVertex);
        }
    }
    //运动到某个点
    public void gotoVertex(TrailerGraph.Vertex vertex)
    {
        Vector3 vPos1 = vertex.vertexPos;
        vPos1.y = carMessage.carPos.y;
        if (!VectorTool.IsCloseEnough(vPos1, carMessage.carPos, carMessage.speed))
        {
            //判断车在直线还是曲线
            if (carMessage.carEdge.lineType.Equals(LineType.Curve))//曲线
            {
                //模拟旋转运动
                SimuRotateAround(curveRadius, carMessage.carEdge.curveCenter, carMessage.speed * GlobalVaribles.frameTime);
            }
            else if (carMessage.carEdge.lineType.Equals(LineType.Straight))
            {
                //模拟直线运动
                SimuMoveTowards(vPos1, carMessage.speed * GlobalVaribles.frameTime);
            }
            else { }
        }
        else//已经到达该点
        {
            carMessage.carPos = vPos1;
            carPathData = GlobalVaribles.trailerGraph.pathMap[vertex.index, carPathData.pathList[carPathData.pathList.Count - 1].index];
            if (carMessage.workState == WorkState.WayToLoad && vertex.index == task.loadGoods_Edge.vertexNum1)
            {
                carMessage.carEdge = GlobalVaribles.trailerGraph.adjMatrix[vertex.index, task.loadGoods_Edge.vertexNum2];
            }
            else if (carMessage.workState == WorkState.WayToUnload && vertex.index == task.unloadGoods_Edge.vertexNum1)
            {
                carMessage.carEdge = GlobalVaribles.trailerGraph.adjMatrix[vertex.index, task.unloadGoods_Edge.vertexNum2];
            }
            else
            {
                carMessage.carEdge = GlobalVaribles.trailerGraph.adjMatrix[carPathData.pathList[0].index, carPathData.pathList[1].index];
            }
            carMessage.angled = carMessage.carEdge.startDeg;
            return;
        }
    }
    //运动到某个位置
    public void gotoPos(Vector3 position)
    {
        position.y = carMessage.carPos.y;
        if (!VectorTool.IsCloseEnough(position, carMessage.carPos, carMessage.speed))
        {
            //判断车在直线还是曲线
            if (carMessage.carEdge.lineType.Equals(LineType.Curve))
            {
                //模拟旋转运动
                SimuRotateAround(curveRadius, carMessage.carEdge.curveCenter, carMessage.speed * GlobalVaribles.frameTime);
            }
            else if (carMessage.carEdge.lineType.Equals(LineType.Straight))
            {
                //模拟直线运动
                SimuMoveTowards(position, carMessage.speed * GlobalVaribles.frameTime);
            }
            else { }
        }
        else//说明已经到达该点
        {
            carMessage.carPos = position;
            //退出该方法
            return;
        }
    }
    //取货
    public void loadingGoods()
    {
        if (loadAccumuTime >= task.loadTime)
        {
            carMessage.workState = WorkState.WayToUnload;
            speedChangeable = true;
            carMessage.speed = moveSpeed;
            //现在表示从取货点到卸货点的路径
            carPathData = GlobalVaribles.trailerGraph.pathMap[task.loadGoods_Edge.vertexNum1,
                task.unloadGoods_Edge.vertexNum1];
            loadAccumuTime = 0;
        }
        loadAccumuTime += GlobalVaribles.frameTime;
    }
    //卸货
    public void unloadingGoods()
    {
        if (unloadAccumuTime >= task.unloadTime)
        {
            carMessage.workState = WorkState.Empty;
            speedChangeable = true;
            carMessage.speed = 0.0f;
            task = null;//清空任务
            carPathData = null;
            unloadAccumuTime = 0;
        }
        unloadAccumuTime += GlobalVaribles.frameTime;
    }
    //模拟小车的旋转运动
    public void SimuRotateAround(float curveRadius, Vector3 rotateCenter, float lineSpeed)
    {
        float angularSpeed = lineSpeed / curveRadius;//计算角速度 radian
        //deg是角度，radian是弧度
        //累计的角度（每次进入曲线轨道，都要给小车一个新的初始角度）
        carMessage.angled += (Mathf.Rad2Deg * angularSpeed) % 360;
        float posX = rotateCenter.x + curveRadius * Mathf.Sin(carMessage.angled * Mathf.Deg2Rad);
        float posZ = rotateCenter.z + curveRadius * Mathf.Cos(carMessage.angled * Mathf.Deg2Rad);
        //更新carPos
        carMessage.carPos = new Vector3(posX, carMessage.carPos.y, posZ);
    }
    //模拟小车的直线运动
    public void SimuMoveTowards(Vector3 endPos, float speed)
    {
        if (VectorTool.IsCloseEnough(carMessage.carPos, endPos, carMessage.speed))
        {
            carMessage.carPos = endPos;
            return;
        }
        Vector3 moveVector3 = VectorTool.vectorSub(endPos, carMessage.carPos).normalized * speed;
        carMessage.carPos += moveVector3;
    }
    //模拟碰撞检测 并 防止碰撞
    public void SimuAvoidCrash()
    {
        bool noOther = true;
        if (MainController.allSCC != null)
        {
            foreach (SimuCarController scc in MainController.allSCC)
            {
                if (scc.carMessage.carName != carMessage.carName)//是其他小车
                {
                    noOther = false;
                    if (Vector3.Distance(scc.carMessage.carPos, carMessage.carPos) <= safeDis)//若其他小车距离本车过近,修改速度
                    {
                        SimuChangeSpeed(scc.carMessage);//修改小车速度
                    }
                }
            }
        }
        if (noOther)
        {
            SpeedRecover();
        }
    }
    //判断两车前后关系
    public bool isFront(Car targetCar)
    {
        //本车到目标车的路径距离，目标车到本车的路径距离
        float s2t, t2s;
        s2t = t2s = 0.0f;
        //车所在边
        TrailerGraph.Vertex selfVertex1 = GlobalVaribles.trailerGraph.vertexes[carMessage.carEdge.vertexNum1];
        TrailerGraph.Vertex selfVertex2 = GlobalVaribles.trailerGraph.vertexes[carMessage.carEdge.vertexNum2];
        TrailerGraph.Vertex targetVertex1 = GlobalVaribles.trailerGraph.vertexes[targetCar.carEdge.vertexNum1];
        TrailerGraph.Vertex targetVertex2 = GlobalVaribles.trailerGraph.vertexes[targetCar.carEdge.vertexNum2];
        if (targetCar.carEdge.Equals(carMessage.carEdge))//2车在同一条边上
        {
            t2s = Vector3.Distance(targetCar.carPos, targetVertex2.vertexPos);
            s2t = Vector3.Distance(carMessage.carPos, selfVertex2.vertexPos);
        }
        else//不在同一条边，分别计算两车经过轨道到达彼此的距离，距离短的那个在后面
        {
            s2t += Vector3.Distance(targetCar.carPos, targetVertex2.vertexPos);
            t2s += Vector3.Distance(carMessage.carPos, selfVertex2.vertexPos);

            s2t += GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum2, targetCar.carEdge.vertexNum1].shortLength;
            t2s += GlobalVaribles.trailerGraph.pathMap[targetCar.carEdge.vertexNum2, carMessage.carEdge.vertexNum1].shortLength;

            s2t += Vector3.Distance(targetCar.carPos, targetVertex1.vertexPos);
            t2s += Vector3.Distance(carMessage.carPos, selfVertex1.vertexPos);
        }
        if (s2t <= t2s)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    //速度恢复
    public void SpeedRecover()
    {
        if (carMessage.workState != WorkState.Loading && carMessage.workState != WorkState.Unloading)
        {
            speedChangeable = true;
        }
        //if (carMessage.workState == WorkState.Empty)
        //{
        //    if (carPathData == null)
        //    {
        //        carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum2, carMessage.carEdge.vertexNum1];
        //    }
        //}
    }
    //模拟在碰撞时修改小车速度
    public void SimuChangeSpeed(Car targetCar)
    {
        if (speedChangeable)
        {
            bool isfront = isFront(targetCar);
            if (isfront)
            {
                if (carPathData == null)//只有可能在empty时发生，此时前车需要先获得路径
                {
                    carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum2,
                carMessage.carEdge.vertexNum1];
                }
                carMessage.speed = moveSpeed;
            }
            else
            {
                carMessage.speed = targetCar.speed;
                speedChangeable = false;
            }
        }
    }
}

////问题：
///模拟小车的旋转运动
//方案：给曲线（Curve）Edge 设置一个属性为startDeg和endDeg，
//一旦小车进入曲线就初始化angled为startDeg,然后angle不断积累
//注意：速度单位要统一

///直线运动
//方案：模拟实现MoveTowards函数

///模拟小车之间的碰撞检测
//方案：遍历所有小车的carMessage，发现当前小车的safeDis之内有其他小车，则有碰撞风险
//然后判断小车的前后关系

///时间的计算问题
//只有所有任务遍历一次，才累加frameTime

//问题:不同点y值不统一的问题
//已解决

//问题：angled不能修改？？？