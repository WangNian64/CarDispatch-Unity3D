using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {
    public Car carMessage;//车的信息
    public float moveSpeed;//运动时的速度
    private float curveRadius;//半径

    private float loadStartTime;//取货开始时间
    private float unloadStartTime;//卸货开始时间

    private TrailerGraph.PathData carPathData;//小车到某个点的最短路径数据
    public float safeDis;//小车之间的安全距离
    public Task task;//车对应的任务

	// Use this for initialization
	void Awake () {
        curveRadius = GlobalVaribles.trailerGraph.curveRadius;
        carMessage = this.GetComponent<ShowCarMessage>().carMessage;
        moveSpeed = GlobalVaribles.moveSpeed;
        safeDis = GlobalVaribles.safeDis;
    }
	
	// Update is called once per frame
	void Update () {
        CarMovement();
        AvoidCrash();
	}
    //车的运动函数
    public void CarMovement()
    {
        if (task != null)//有任务了
        {
            //if(carMessage.carName == "Car2" || carMessage.carName == "Car3")
            //task.toString();
            switch (carMessage.workState)
            {
                case WorkState.Empty:
                    carMessage.workState = WorkState.WayToLoad;
                    carMessage.speed = moveSpeed;
                    carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum1,
                        task.loadGoods_Edge.vertexNum2];
                    break;
                case WorkState.WayToLoad:
                    carMessage.carPos = transform.localPosition;
                    gotoLoadPoint();//运动到取货点
                    break;
                case WorkState.Loading:
                    loadingGoods();
                    break;
                case WorkState.WayToUnload:
                    carMessage.carPos = transform.localPosition;
                    gotoUnloadPoint();
                    break;
                case WorkState.Unloading:
                    unloadingGoods();
                    break;
                default:
                    break;
            }
        } else//此时车的状态一定是empty
        {
            carMessage.workState = WorkState.Empty;
            carMessage.speed = 0;
        }
    }
    //运动到取货点
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
            if (transform.localPosition == loadPos1)
            {
                carMessage.workState = WorkState.Loading;
                carMessage.speed = 0.0f;//之后车是静止的
                loadStartTime = Time.time;
                //更新可分配的车和任务
                Car carToRemove = new Car();
                foreach (Car c in GlobalVaribles.allocableCars)
                {
                    if (c.carName.Equals(this.carMessage.carName))
                    {
                        carToRemove = c;
                    }
                }
                GlobalVaribles.allocableCars.Remove(carToRemove);
                
                GlobalVaribles.allocableTasks.Remove(task);
            }
        }
    }
    //运动到卸货点
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
            Vector3 unloadPos1 = new Vector3(task.unloadGoodsPos.x, task.unloadGoodsPos.y + carMessage.carSize.y / 2, task.unloadGoodsPos.z);
            if (transform.localPosition == unloadPos1)
            {
                carMessage.workState = WorkState.Unloading;
                carMessage.speed = 0.0f;
                unloadStartTime = Time.time;
            }
        }
    }
    //运动到某个点
    public void gotoVertex(TrailerGraph.Vertex vertex)
    {
        Vector3 vPos1 = vertex.vertexPos;
        vPos1.y = transform.localPosition.y;
        if (!VectorTool.IsCloseEnough(vPos1, transform.localPosition, carMessage.speed))
        {
            //判断车在直线还是曲线
            if (carMessage.carEdge.lineType.Equals(LineType.Curve))//曲线
            {
                //旋转运动
                transform.RotateAround(carMessage.carEdge.curveCenter, new Vector3(0, 1, 0), carMessage.speed * Time.deltaTime);
            }
            else if (carMessage.carEdge.lineType.Equals(LineType.Straight))
            {
                //直线运动
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, vPos1, carMessage.speed * Time.deltaTime);
            } else { }
        } else//已经到达该点
        {
            transform.localPosition = vPos1;
            if (carMessage.workState == WorkState.WayToLoad)//更新carPath
            {
                carPathData = GlobalVaribles.trailerGraph.pathMap[vertex.index, task.loadGoods_Edge.vertexNum1];
                if (vertex.index == task.loadGoods_Edge.vertexNum1)
                {
                    carMessage.carEdge = GlobalVaribles.trailerGraph.adjMatrix[vertex.index, task.loadGoods_Edge.vertexNum2];
                } else
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
            //退出该方法
            return;
        }
    }
    //运动到某个位置
    public void gotoPos(Vector3 position)
    {
        position.y = transform.localPosition.y;
        if (!VectorTool.IsCloseEnough(position, transform.localPosition, carMessage.speed))
        {
            //判断车在直线还是曲线
            if (carMessage.carEdge.lineType.Equals(LineType.Curve))
            {
                //旋转运动
                transform.RotateAround(carMessage.carEdge.curveCenter, new Vector3(0, 1, 0), carMessage.speed * Time.deltaTime);
            }
            else if (carMessage.carEdge.lineType.Equals(LineType.Straight))
            {
                //直线运动
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, carMessage.speed * Time.deltaTime);
            }
            else { }
        }
        else//已经到达该点
        {
            transform.localPosition = position;
            //退出该方法
            return;
        }
    }
    //取货
    public void loadingGoods()
    {
        //Debug.Log("取货" + loadStartTime + "," + Time.time);
        if (Time.time >= loadStartTime + task.loadTime)
        {
            carMessage.workState = WorkState.WayToUnload;
            carMessage.speed = moveSpeed;
            //现在表示从取货点到卸货点的路径
            carPathData = GlobalVaribles.trailerGraph.pathMap[task.loadGoods_Edge.vertexNum1,
                task.unloadGoods_Edge.vertexNum2];
        }
    }
    //卸货
    public void unloadingGoods()
    {
        //Debug.Log("卸货" + unloadStartTime + "," + Time.time);
        if (Time.time >= unloadStartTime + task.unloadTime)
        {
            carMessage.workState = WorkState.Empty;
            carMessage.speed = 0.0f;
            task = null;//清空任务
            //去除该任务
            GlobalVaribles.allTasks.Remove(task);
        }
    }
    //避免碰撞
    public void AvoidCrash()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, safeDis, 1 << LayerMask.NameToLayer("Car"));//只检测小车点之间的碰撞
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].gameObject != this.gameObject)
                {
                    calcuDirection(colliders[i].gameObject);//判断两辆小车到达安全距离时的前后位置
                }
            }
        }
    }
    //计算小车之间的方向（前后） 并 修改小车速度
    public void calcuDirection(GameObject target)
    {
        float dot = Vector3.Dot(transform.forward, target.transform.position);
        //调整速度
        if (dot > 0)//目标车在自己的前方，自己要减速，减到和前车一致
        {
            carMessage.speed = target.GetComponent<ShowCarMessage>().carMessage.speed;
        }
        else if (dot < 0)//目标车在自己的后方，自己速度不变
        {

        }
    }
}