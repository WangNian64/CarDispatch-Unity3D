using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//控制全局的任务分配
public class MainController : MonoBehaviour
{
    //实际分配的车和任务
    private List<Car> trueCars;
    private List<Task> trueTasks;

    public static SimuCarController[] allSCC;//存储所有SimuCarController
    // Use this for initialization
    void Awake()
    {

    }
    // Update is called once per frame
    void FixedUpdate() 
    {
        trueCars = new List<Car>();
        trueTasks = new List<Task>();
        //重新分配任务，先清空已经分配的任务
        foreach (Car car in GlobalVaribles.car_list)
        {
            CarController cc = GameObject.Find(car.carName).gameObject.GetComponent<CarController>();
            if (cc.carMessage.workState == WorkState.Empty || cc.carMessage.workState == WorkState.WayToLoad)
            {
                cc.task = null;
            }
        }
        if (GlobalVaribles.allocableTasks.Count > 0 && GlobalVaribles.allocableCars.Count > 0)
        {
            Dictionary<Car, Task> minMatch = new Dictionary<Car, Task>();        //matchList中的最小匹配minMatch
            //Task>=Car, 从任务队列中取出小车数目的任务，再进行全排列
            if (GlobalVaribles.allocableTasks.Count >= GlobalVaribles.allocableCars.Count)
            {
                List<Dictionary<Car, Task>> matchList = new List<Dictionary<Car, Task>>(); //匹配Dic
                for (int i = 0; i < GlobalVaribles.allocableCars.Count; i++)
                {
                    trueTasks.Add(GlobalVaribles.allocableTasks.ElementAt(i));
                }
                trueCars = GlobalVaribles.allocableCars;
                //得到对应的匹配组合（car和task一样多）
                getMatchList(ref matchList, trueCars.ToArray(), trueTasks, 0, trueCars.Count);

                //matchList的每一项都应该包括unAllocableMatch
                foreach (Dictionary<Car, Task> matchDic in matchList)
                {
                    foreach (KeyValuePair<Car, Task> kvp in GlobalVaribles.UnAllocableMatch)
                    {
                        matchDic.Add(kvp.Key, kvp.Value);//进行深拷贝
                    }
                }
                float minTime = getMinMatch(matchList, ref minMatch);
            }
            else//Task<Car，会有空闲的小车，此时需要进行（多选少），再进行全排列
            {
                //所有任务都要分配
                foreach (Task t in GlobalVaribles.allocableTasks)
                {
                    trueTasks.Add(t);
                }
                List<Car[]> carLists = MathTool<Car>.GetCombination(GlobalVaribles.allocableCars.ToArray(), trueTasks.Count);

                float minTime = GlobalVaribles.INFINITY;
                foreach (Car[] carList in carLists)
                {
                    List<Dictionary<Car, Task>> aMatchList = new List<Dictionary<Car, Task>>();
                    getMatchList(ref aMatchList, carList, trueTasks, 0, carList.Length);
                    //matchList的每一项都应该包括unAllocableMatch
                    foreach (Dictionary<Car, Task> matchDic in aMatchList)
                    {
                        foreach (KeyValuePair<Car, Task> kvp in GlobalVaribles.UnAllocableMatch)
                        {
                            matchDic.Add(kvp.Key, kvp.Value);//进行深拷贝
                        }
                    }
                    //计算matchList中的最小匹配minMatch
                    Dictionary<Car, Task> aMinMatch = new Dictionary<Car, Task>();
                    float aMinTime = getMinMatch(aMatchList, ref aMinMatch);
                    //aMinTime只是局部最小，还要比较多个matchList的最小值
                    if (aMinTime < minTime)
                    {
                        minTime = aMinTime;
                        minMatch = aMinMatch;
                    }
                }
            }
            //给各个小车分发任务
            DistributeTasks(minMatch);
        }
    }
    #region //计算x个车和x个任务的所有排列
    public void getMatchList(ref List<Dictionary<Car, Task>> matchList, Car[] carList, List<Task> taskList, int begin, int end)
    {
        if (begin == end)
        {
            //添加一个匹配
            Dictionary<Car, Task> match = new Dictionary<Car, Task>();
            for (int i = 0; i < carList.Length; i++)
            {
                match.Add(carList[i], taskList[i]);
            }
            matchList.Add(match);
        }
        for (int i = begin; i < end; i++)
        {
            if (IsSwap(taskList, begin, i))
            {
                Swap(taskList, begin, i);
                getMatchList(ref matchList, carList, taskList, begin + 1, end);
                Swap(taskList, begin, i);
            }
        }
    }
    //判断是否重复,重复的话要交换
    public static bool IsSwap(List<Task> tasks, int nBegin, int nEnd)
    {
        for (int i = nBegin; i < nEnd; i++)
            if (tasks[i] == tasks[nEnd])
                return false;
        return true;
    }
    //交换数组中指定元素
    static void Swap(List<Task> tasks, int x, int y)
    {
        Task t = tasks[x];
        tasks[x] = tasks[y];
        tasks[y] = t;
    }
    #endregion

    #region //n中取出m个的全部组合
    public List<List<Car>> GetCombination(Car[] cars, int m)
    {
        if (cars.Length < m)
        {
            return null;
        }
        int[] temp = new int[m];
        List<Car[]> carLists = new List<Car[]>();
        GetCombination(ref carLists, cars, cars.Length, m, temp, m);
        //转换格式
        List<List<Car>> carLists1 = new List<List<Car>>(); 
        for (int i = 0; i < carLists.Count; i++)
        {
            carLists1.Add(carLists[i].ToList());
        }
        return carLists1;
    }
    private void GetCombination(ref List<Car[]> carLists, Car[] cars, int n, int m, int[] b, int M)
    {
        for (int i = n; i >= m; i--)
        {
            b[m - 1] = i - 1;
            if (m > 1)
            {
                GetCombination(ref carLists, cars, i - 1, m - 1, b, M);
            }
            else
            {
                if (carLists == null)
                {
                    carLists = new List<Car[]>();
                }
                Car[] temp = new Car[M];
                for (int j = 0; j < b.Length; j++)
                {
                    temp[j] = cars[b[j]];
                }
                carLists.Add(temp);
            }
        }
    }
    #endregion
    //计算一个匹配组合完成所有任务的时间
    public float calcuTasksTime(Dictionary<Car, Task> match)
    {
        float completeTime = 0.0f;
        float unFinishNum = match.Count;
        allSCC = new SimuCarController[match.Count];//每次更新allSCC
        int i = 0;
        foreach (KeyValuePair<Car, Task> kvp in match)
        {
            allSCC[i] = new SimuCarController(kvp.Key, kvp.Value, GlobalVaribles.moveSpeed,
                GlobalVaribles.trailerGraph.curveRadius, GlobalVaribles.safeDis);
            i++;
        }
        while (true)
        {
            foreach (SimuCarController scc in allSCC)
            {
                if (unFinishNum <= 0)
                {
                    return completeTime;
                }
                if (scc.task == null)
                {
                    unFinishNum--;
                }
                scc.SimuUpdate();
            }
            completeTime += GlobalVaribles.frameTime;
        }
    }
    //得到matchList中的时间最小Match, 返回最小match对应的时间, count是每个match的车或任务的数目
    public float getMinMatch(List<Dictionary<Car, Task>> matchList, ref Dictionary<Car, Task> minMatch)
    {
        float[] times = new float[matchList.Count];//任务所需时间
        int index = 0;
        float minTime = GlobalVaribles.INFINITY;//最短时间
        minMatch = matchList[0];//最短时间的匹配
        foreach (Dictionary<Car, Task> match in matchList)
        {
            //进行深拷贝,因为minMatch是实际的car、task状态，不能在SimuCarController中更改
            Dictionary<Car, Task> tempMatch = new Dictionary<Car, Task>();
            foreach (Car c in match.Keys)
            {
                Car car = c.Clone();
                Task task = match[c].Clone();
                tempMatch.Add(car, task);
            }
            times[index] = calcuTasksTime(tempMatch);//求所有小车完成所有任务的时间
            if (times[index] < minTime)
            {
                minTime = times[index];
                minMatch = match;
            }
            index++;
        }
        return minTime;
    }
    //根据匹配给小车分配任务（之后自动执行）
    public void DistributeTasks(Dictionary<Car, Task> match)
    {
        foreach (KeyValuePair<Car, Task> kvp in match)
        {
            GameObject car = GameObject.Find(kvp.Key.carName).gameObject;
            Task task = kvp.Value;
            car.GetComponent<CarController>().task = task;
            //同时给任务点模型染色
            GameObject taskLoadPoint = GameObject.Find("Task" + kvp.Value.taskID + "_loadPoint");
            GameObject taskUnLoadPoint = GameObject.Find("Task" + kvp.Value.taskID + "_unloadPoint");
            taskLoadPoint.GetComponent<MeshRenderer>().material.color = 
                taskUnLoadPoint.GetComponent<MeshRenderer>().material.color = car.GetComponent<MeshRenderer>().material.color;
        }
    }
}

//问题：在模拟计算时carMessage不能修改
//解决：对match进行深拷贝，构造一个副本进行模拟，不对原来的match进行修改
