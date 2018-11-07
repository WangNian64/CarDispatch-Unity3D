using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//模拟小车
//信息和CarController一致
//不同在于更新的是carMessage的carPos位置
public class SimuCarController {
    private Car carMessage;//车的信息
    public Task task;//车对应的任务

    public float moveSpeed;//运动时的速度
    private float curveRadius;//曲线半径
    public float safeDis;//小车之间的安全距离

    //private float loadStartTime;//取货开始时间
    //private float unloadStartTime;//卸货开始时间
    private float loadAccumuTime;
    private float unloadAccumuTime;
    private TrailerGraph.PathData carPathData;//小车到某个点的最短路径数据
    
    //构造函数
    public SimuCarController(Car carMessage, Task task, float moveSpeed, float curveRadius,
        float safeDis)
    {
        this.carMessage = carMessage;
        this.task = task;
        this.moveSpeed = moveSpeed;
        this.curveRadius = curveRadius;
        this.safeDis = safeDis;
    }   
    //模拟update函数
    public void SimuUpdate()
    {
        //if (carMessage.carName == "Car1")
        //    carMessage.toString();
        SimuCarMovement();
        //SimuAvoidCrash();
    }
    //模拟小车运动
    public void SimuCarMovement()
    {
        if (task != null)//有任务了
        {
            switch (carMessage.workState)
            {
                case WorkState.Empty:
                    carMessage.workState = WorkState.WayToLoad;
                    carMessage.speed = moveSpeed;
                    carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum1,
                        task.loadGoods_Edge.vertexNum2];
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
        } else { }
    }
    //模拟运动到取货点
    public void gotoLoadPoint()
    {
        //得到车的下一个节点
        int nextVertexNum = carMessage.carEdge.vertexNum2;
        TrailerGraph.Vertex nextVertex = GlobalVaribles.trailerGraph.vertexes[nextVertexNum];
        //车运动到下一个结点的位置
        if (nextVertex.index != task.loadGoods_Edge.vertexNum2)//还未到达最后
        {
            gotoVertex(nextVertex);
        }
        else//到达最后一条边
        {
            gotoPos(task.loadGoodsPos);
            Vector3 loadPos1 = new Vector3(task.loadGoodsPos.x,
                task.loadGoodsPos.y + carMessage.carSize.y / 2, task.loadGoodsPos.z);
            if (carMessage.carPos == loadPos1)
            {
                carMessage.workState = WorkState.Loading;
                carMessage.speed = 0.0f;//之后车是静止的
            }
        }
    }
    //模拟运动到卸货点
    public void gotoUnloadPoint()
    {
        //得到车的下一个节点
        int nextVertexNum = carMessage.carEdge.vertexNum2;
        TrailerGraph.Vertex nextVertex = GlobalVaribles.trailerGraph.vertexes[nextVertexNum];
        //车运动到下一个结点的位置
        if (nextVertex.index != task.unloadGoods_Edge.vertexNum2)//还未到达最后一个点
        {
            gotoVertex(nextVertex);
        }
        else//到达最后一条边
        {
            gotoPos(task.unloadGoodsPos);
            //task的y值和货物不相同
            Vector3 unloadPos1 = new Vector3(task.unloadGoodsPos.x,
                task.unloadGoodsPos.y + carMessage.carSize.y / 2, task.unloadGoodsPos.z);
            if (carMessage.carPos == unloadPos1)
            {
                carMessage.workState = WorkState.Unloading;
                carMessage.speed = 0.0f;
            }
        }
    }
    //车运动到某个点
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
            if (carMessage.workState == WorkState.WayToLoad)//更新carPath
            {
                carPathData = GlobalVaribles.trailerGraph.pathMap[vertex.index, task.loadGoods_Edge.vertexNum1];
                if (vertex.index == task.loadGoods_Edge.vertexNum1)
                {
                    carMessage.carEdge = GlobalVaribles.trailerGraph.adjMatrix[vertex.index, task.loadGoods_Edge.vertexNum2];
                }
                else
                {
                    carMessage.carEdge = GlobalVaribles.trailerGraph.adjMatrix[carPathData.pathList[0].index, carPathData.pathList[1].index];
                }
            }
            else if (carMessage.workState == WorkState.WayToUnload)
            {
                carPathData = GlobalVaribles.trailerGraph.pathMap[vertex.index, task.unloadGoods_Edge.vertexNum1];
                if (vertex.index == task.unloadGoods_Edge.vertexNum1)
                {
                    carMessage.carEdge = GlobalVaribles.trailerGraph.adjMatrix[vertex.index, task.unloadGoods_Edge.vertexNum2];
                }
                else
                {
                    carMessage.carEdge = GlobalVaribles.trailerGraph.adjMatrix[carPathData.pathList[0].index, carPathData.pathList[1].index];
                }
            }
            else
            {
                carMessage.angled = 0;
            }
            //退出该方法
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
        //Debug.Log("取货");
        if (loadAccumuTime >= task.loadTime)
        {
            //Debug.Log("取货完成");
            carMessage.workState = WorkState.WayToUnload;
            carMessage.speed = moveSpeed;
            //现在表示从取货点到卸货点的路径
            carPathData = GlobalVaribles.trailerGraph.pathMap[task.loadGoods_Edge.vertexNum1,
                task.unloadGoods_Edge.vertexNum2];
        }
        loadAccumuTime += GlobalVaribles.frameTime;
    }
    //卸货
    public void unloadingGoods()
    {
        //Debug.Log("卸货");
        if (unloadAccumuTime >= task.unloadTime)
        {
            //Debug.Log("卸货完成");
            carMessage.workState = WorkState.Empty;
            carMessage.speed = 0.0f;
            task = null;//清空任务
        }
        unloadAccumuTime += GlobalVaribles.frameTime;
    }
    //模拟小车的旋转运动
    public void SimuRotateAround(float curveRadius, Vector3 rotateCenter, float rotateSpeed)
    {
        float angularSpeed = rotateSpeed / curveRadius;//计算角速度
        //累计的角度（每次进入曲线轨道，都要给小车一个新的初始角度）
        carMessage.angled += angularSpeed % (2 * Mathf.PI);
        float posX = rotateCenter.x + curveRadius * Mathf.Sin(carMessage.angled);
        float posZ = rotateCenter.z + curveRadius * Mathf.Cos(carMessage.angled);
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
        //修改carMessage的carPos
        Vector3 moveVector3 = VectorTool.vectorSub(endPos, carMessage.carPos).normalized * speed;
        carMessage.carPos += moveVector3;
    }
    //模拟碰撞检测 并 防止碰撞
    //问题：当小车>任务数目，那么会有小车空闲，此时应该让空闲小车运动
    public void SimuAvoidCrash()
    {
        //得到所有小车
        List<Car> carList = GlobalVaribles.allCars;
        //查找本小车附近的小车
        foreach (Car car in carList)//遍历其他所有小车
        {
            if (car.carName != carMessage.carName)
            {
                //若其他小车距离本车过近,修改速度
                if (Vector3.Distance(car.carPos, carMessage.carPos) <= safeDis)
                {
                    SimuChangeSpeed(car);
                }
            }
        }
    }
    //方案1：计算目标车相对于本车的前后关系，然后调整速度
    //方案2：找到速度快的车，修改其速度为速度慢的（这里用的）
    public void SimuChangeSpeed(Car targetCar)
    {
        if (targetCar.speed >= carMessage.speed)
        {
            targetCar.speed = carMessage.speed;
        } else
        {
            carMessage.speed = targetCar.speed;
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