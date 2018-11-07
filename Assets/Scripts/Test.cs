﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//测试脚本
public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Vector3 trailerSize = GlobalVaribles.trailer_Para.trailerSize;
        Task t1 = new Task(
            new Vector3(10, trailerSize.y, 15),
            new Vector3(10, trailerSize.y, 5),
            GlobalVaribles.edges[1],
            GlobalVaribles.edges[1]
        );
        Task t2 = new Task(
            new Vector3(-10, trailerSize.y, -15),
            new Vector3(-10, trailerSize.y, -5),
            GlobalVaribles.edges[4],
            GlobalVaribles.edges[4]
        );
        Task t3 = new Task(
            new Vector3(-10, trailerSize.y, 5),
            new Vector3(-10, trailerSize.y, 15),
            GlobalVaribles.edges[5],
            GlobalVaribles.edges[5]
        );
        GlobalVaribles.allTasks.Add(t1);
        GlobalVaribles.allocableTasks.Add(t1);
        GlobalVaribles.allTasks.Add(t2);
        GlobalVaribles.allocableTasks.Add(t2);
        GlobalVaribles.allTasks.Add(t3);
        GlobalVaribles.allocableTasks.Add(t3);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}