using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {
    public Car carMessage;//车的信息
    public float moveSpeed;//运动时的速度
    private float curveRadius;//半径

    private float loadStartTime;//取货开始时间
    private float unloadStartTime;//卸货开始时间

    public TrailerGraph.PathData carPathData;//小车到某个点的最短路径数据
    public float safeDis;//小车之间的安全距离
    public Task task;//车对应的任务

    public bool speedChangeable;
    //public bool SpeedChangeable
    //{
    //    get { return speedChangeable; }
    //    set { speedChangeable = value; }
    //}
	// Use this for initialization
	void Awake () {
        curveRadius = GlobalVaribles.trailerGraph.curveRadius;
        carMessage = this.GetComponent<ShowCarMessage>().carMessage;
        moveSpeed = GlobalVaribles.moveSpeed;
        safeDis = GlobalVaribles.safeDis;
        speedChangeable = true;//初始可修改
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (carPathData != null)//防止循环从0->0
        {
            if (carPathData.pathList[0].index == carPathData.pathList[1].index)
            {
                carPathData = null;
            }
        }
        CarMovement();
        AvoidCrash();
    }
    //车的运动函数
    public void CarMovement()
    {
        if (task != null)//有任务了
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
                    gotoLoadPoint();
                    break;
                case WorkState.Loading:
                    loadingGoods();
                    break;
                case WorkState.WayToUnload:
                    gotoUnloadPoint();
                    break;
                case WorkState.Unloading:
                    unloadingGoods();
                    break;
                default:
                    break;
            }
        }
        else
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
    //运动到取货点
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

                Task taskRemoveALLT = new Task();
                foreach (Task t in GlobalVaribles.allocableTasks)
                {
                    if (t.taskID == task.taskID)
                    {
                        taskRemoveALLT = t;
                    }
                }
                GlobalVaribles.allocableTasks.Remove(taskRemoveALLT);

                //更新UnAllocableMatch
                GlobalVaribles.UnAllocableMatch.Add(carMessage, task);
            } else {
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
    //运动到卸货点
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
            if (transform.localPosition == unloadPos1)
            {
                carMessage.workState = WorkState.Unloading;
                carMessage.speed = 0.0f;
                unloadStartTime = Time.time;
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
        vPos1.y = transform.localPosition.y;
        if (!VectorTool.IsCloseEnough(vPos1, transform.localPosition, carMessage.speed))
        {
            //判断车在直线还是曲线
            if (carMessage.carEdge.lineType.Equals(LineType.Curve))//曲线
            {
                //旋转运动
                MyRotateAround(carMessage.carEdge.curveCenter, new Vector3(0, 1, 0), carMessage.speed * Time.deltaTime);
                carMessage.carPos = transform.localPosition;
            }
            else if (carMessage.carEdge.lineType.Equals(LineType.Straight))
            {
                //直线运动
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, vPos1, carMessage.speed * Time.deltaTime);
                carMessage.carPos = transform.localPosition;
            } else { }
        }
        else//已经到达该点
        {
            transform.localPosition = vPos1;
            carMessage.carPos = transform.localPosition;

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
            //进入新的曲线, 更新angled
            carMessage.angled = carMessage.carEdge.startDeg;
            //旋转值的修正
            Vector3 rotation = transform.localEulerAngles;
            rotation.y = carMessage.carEdge.endDeg;
            transform.localEulerAngles = rotation;
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
                MyRotateAround(carMessage.carEdge.curveCenter, new Vector3(0, 1, 0), carMessage.speed * Time.deltaTime);
                carMessage.carPos = transform.localPosition;
            }
            else if (carMessage.carEdge.lineType.Equals(LineType.Straight))
            {
                //直线运动
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, carMessage.speed * Time.deltaTime);
                carMessage.carPos = transform.localPosition;
            }
            else { }
        }
        else//已经到达该点
        {
            transform.localPosition = position;
            carMessage.carPos = transform.localPosition;
            return;
        }
    }
    //取货
    public void loadingGoods()
    {
        speedChangeable = false;
        if (Time.time >= loadStartTime + task.loadTime)
        {
            carMessage.workState = WorkState.WayToUnload;
            speedChangeable = true;
            carMessage.speed = moveSpeed;
            //现在表示从取货点到卸货点的路径
            carPathData = GlobalVaribles.trailerGraph.pathMap[task.loadGoods_Edge.vertexNum1,
                task.unloadGoods_Edge.vertexNum1];
        }
    }
    //卸货
    public void unloadingGoods()
    {
        speedChangeable = false;
        if (Time.time >= unloadStartTime + task.unloadTime)
        {
            carMessage.workState = WorkState.Empty;
            speedChangeable = true;
            carMessage.speed = 0.0f;
            
            //任务完成，变为白色
            GameObject taskLoadPoint = GameObject.Find("Task" + task.taskID + "_loadPoint");
            GameObject taskUnLoadPoint = GameObject.Find("Task" + task.taskID + "_unloadPoint");
            taskLoadPoint.GetComponent<MeshRenderer>().material.color =
                taskUnLoadPoint.GetComponent<MeshRenderer>().material.color = Color.white;
            //去除该任务
            Task taskToRemove = new Task();
            foreach (Task t in GlobalVaribles.allTasks)
            {
                if (t.taskID == task.taskID)
                {
                    taskToRemove = t;
                }
            }
            GlobalVaribles.allTasks.Remove(taskToRemove);

            KeyValuePair<Car, Task> kvpToRemove = new KeyValuePair<Car, Task>();
            foreach (KeyValuePair<Car, Task> kvp in GlobalVaribles.UnAllocableMatch)
            {
                if (kvp.Key.carName == carMessage.carName)
                {
                    kvpToRemove = kvp;
                }
            }
            GlobalVaribles.UnAllocableMatch.Remove(kvpToRemove.Key);//删除UnAllocableMatch对应项
            GlobalVaribles.allocableCars.Add(carMessage);//小车重新可分配


            task = null;//清空任务
            carPathData = null;
        }
    }
    //避免碰撞
    public void AvoidCrash()
    {
        bool noOther = true;//是否有其他车与本车发生碰撞
        Collider[] colliders = Physics.OverlapSphere(transform.localPosition, safeDis, 1 << LayerMask.NameToLayer("Car"));//只检测小车点之间的碰撞
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].gameObject != this.gameObject)
                {
                    noOther = false;
                    changeSpeed(colliders[i].gameObject);//修改小车速度
                }
            }
        }
        if (noOther)
        {
            SpeedRecover();
        }
    }
    //判断两车前后关系
    public bool isFront(GameObject target)
    {
        //本车到目标车的路径距离，目标车到本车的路径距离
        float s2t, t2s;
        s2t = t2s = 0.0f;
        Car targetCar = target.GetComponent<ShowCarMessage>().carMessage;
        //车所在边
        TrailerGraph.Vertex selfVertex1 = GlobalVaribles.trailerGraph.vertexes[carMessage.carEdge.vertexNum1];
        TrailerGraph.Vertex selfVertex2 = GlobalVaribles.trailerGraph.vertexes[carMessage.carEdge.vertexNum2];
        TrailerGraph.Vertex targetVertex1 = GlobalVaribles.trailerGraph.vertexes[targetCar.carEdge.vertexNum1];
        TrailerGraph.Vertex targetVertex2 = GlobalVaribles.trailerGraph.vertexes[targetCar.carEdge.vertexNum2];
        if (targetCar.carEdge.Equals(carMessage.carEdge))//2车在同一条边上
        {
            t2s = Vector3.Distance(targetCar.carPos, targetVertex2.vertexPos);
            s2t = Vector3.Distance(carMessage.carPos, selfVertex2.vertexPos);
        } else//不在同一条边，分别计算两车经过轨道到达彼此的距离，距离短的那个在后面
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
        }else
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
        //if (carMessage.workState == WorkState.Empty)//???
        //{
        //    if (carPathData == null)
        //    {
        //        carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum2, carMessage.carEdge.vertexNum1];
        //    }
        //}
    }
    //计算小车之间的方向（前后）并 修改小车速度
    public void changeSpeed(GameObject target)
    {
        Car targetCar = target.GetComponent<ShowCarMessage>().carMessage;
        if (speedChangeable)
        {
            bool isfront = isFront(target);//判断前后关系
            if (isfront)//本车在前，修改自己的速度
            {
                if (carPathData == null)//只有可能在empty时发生，此时前车需要先获得路径
                {
                    carPathData = GlobalVaribles.trailerGraph.pathMap[carMessage.carEdge.vertexNum2,
                carMessage.carEdge.vertexNum1];
                }
                carMessage.speed = moveSpeed;
            }
            else//本车在后面，自己的速度变为前车的速度，同时速度不能在更改了（除非脱离了当前的碰撞）
            {
                carMessage.speed = targetCar.speed;
                speedChangeable = false;
            }
        }
    }
    //自己实现一个圆周运动函数
    public void MyRotateAround(Vector3 rotateCenter, Vector3 axis, float lineSpeed)
    {
        //线速度->角速度
        float angularSpeed = lineSpeed / curveRadius;//计算角速度
        //旋转运动
        //这里的角速度是弧度制
        //RotateAround使用的是角度制
        transform.RotateAround(rotateCenter, axis, Mathf.Rad2Deg * angularSpeed);
        //累计angled
        carMessage.angled += (Mathf.Rad2Deg * angularSpeed) % 360;
    }
}