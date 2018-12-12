using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//测试脚本
public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Vector3 trailerSize = GlobalVaribles.trailer_Para.trailerSize;
        #region //测试1
        //Task t1 = new Task(
        //    new Vector3(10, trailerSize.y, 15),
        //    new Vector3(10, trailerSize.y, 5),
        //    GlobalVaribles.edges[1],
        //    GlobalVaribles.edges[1]
        //);
        //Task t2 = new Task(
        //    new Vector3(-10, trailerSize.y, -15),
        //    new Vector3(-10, trailerSize.y, -5),
        //    GlobalVaribles.edges[4],
        //    GlobalVaribles.edges[4]
        //);
        //Task t3 = new Task(
        //    new Vector3(-10, trailerSize.y, 5),
        //    new Vector3(-10, trailerSize.y, 15),
        //    GlobalVaribles.edges[5],
        //    GlobalVaribles.edges[5]
        //);
        //GlobalVaribles.allTasks.Add(t1);
        //GlobalVaribles.allocableTasks.Add(t1);
        //GlobalVaribles.allTasks.Add(t2);
        //GlobalVaribles.allocableTasks.Add(t2);
        //GlobalVaribles.allTasks.Add(t3);
        //GlobalVaribles.allocableTasks.Add(t3);
        #endregion

        #region //测试2
        //Task t1 = new Task(
        //    new Vector3(10, trailerSize.y, -10),
        //    new Vector3(-10, trailerSize.y, -10),
        //    GlobalVaribles.edges[2],
        //    GlobalVaribles.edges[4]
        //);
        //GlobalVaribles.allTasks.Add(t1);
        //GlobalVaribles.allocableTasks.Add(t1);
        #endregion

        #region //测试3 2任务2车，测试碰撞避让
        //Task t1 = new Task(
        //    new Vector3(10, trailerSize.y, 10),
        //    new Vector3(10, trailerSize.y, -10),
        //    GlobalVaribles.edges[1],
        //    GlobalVaribles.edges[2]
        //);
        //Task t2 = new Task(
        //    new Vector3(-10, trailerSize.y, -10),
        //    new Vector3(-10, trailerSize.y, 10),
        //    GlobalVaribles.edges[4],
        //    GlobalVaribles.edges[5]
        //);
        //GlobalVaribles.allTasks.Add(t1);
        //GlobalVaribles.allocableTasks.Add(t1);
        //GlobalVaribles.allTasks.Add(t2);
        //GlobalVaribles.allocableTasks.Add(t2);
        //foreach (Task t in GlobalVaribles.allTasks)
        //{
        //    CreateModel.CreateTaskPoint(t);
        //}
        #endregion

        #region//测试4 让小车会选择最短路径
        //Task t1 = new Task(
        //    new Vector3(-10, trailerSize.y, 5),
        //    new Vector3(-10, trailerSize.y, 15),
        //    GlobalVaribles.edges[5],
        //    GlobalVaribles.edges[5]
        //);
        //GlobalVaribles.allTasks.Add(t1);
        //GlobalVaribles.allocableTasks.Add(t1);
        //foreach (Task t in GlobalVaribles.allTasks)
        //{
        //    CreateModel.CreateTaskPoint(t);
        //}
        #endregion

        #region //测试5 可以看出动态的任务分配
        Task t1 = new Task(
            new Vector3(10, trailerSize.y, 10),
            new Vector3(10, trailerSize.y, -10),
            GlobalVaribles.edges[1],
            GlobalVaribles.edges[2]
        );
        Task t2 = new Task(
            new Vector3(-10, trailerSize.y, -10),
            new Vector3(-10, trailerSize.y, 10),
            GlobalVaribles.edges[4],
            GlobalVaribles.edges[5]
        );
        GlobalVaribles.allTasks.Add(t1);
        GlobalVaribles.allocableTasks.Add(t1);
        GlobalVaribles.allTasks.Add(t2);
        GlobalVaribles.allocableTasks.Add(t2);
        foreach (Task t in GlobalVaribles.allTasks)
        {
            CreateModel.CreateTaskPoint(t);
        }
        #endregion

        #region //测试6 三车三任务
        //Task t1 = new Task(
        //    new Vector3(-10, trailerSize.y, 5),
        //    new Vector3(-10, trailerSize.y, 15),
        //    GlobalVaribles.edges[5],
        //    GlobalVaribles.edges[5]
        //);
        //Task t2 = new Task(
        //    new Vector3(10, trailerSize.y, 15),
        //    new Vector3(10, trailerSize.y, 5),
        //    GlobalVaribles.edges[1],
        //    GlobalVaribles.edges[1]
        //);
        //Task t3 = new Task(
        //    new Vector3(10, trailerSize.y, -5),
        //    new Vector3(10, trailerSize.y, -15),
        //    GlobalVaribles.edges[2],
        //    GlobalVaribles.edges[2]
        //);
        //GlobalVaribles.allTasks.Add(t1);
        //GlobalVaribles.allocableTasks.Add(t1);
        //GlobalVaribles.allTasks.Add(t2);
        //GlobalVaribles.allocableTasks.Add(t2);
        //GlobalVaribles.allTasks.Add(t3);
        //GlobalVaribles.allocableTasks.Add(t3);
        //foreach (Task t in GlobalVaribles.allTasks)
        //{
        //    CreateModel.CreateTaskPoint(t);
        //}
        #endregion
    }

    // Update is called once per frame
    void Update () {
		
	}
}
