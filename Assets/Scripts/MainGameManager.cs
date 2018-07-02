using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class MainGameManager : MonoBehaviour
{
    public static MainGameManager Instance { get; private set; }

    [SerializeField]
    private GameObject pointPrefab;

    [SerializeField]
    private int width = 85, height = 40;
    public int Width { get { return width; } }
    public int Height { get { return height; } }
    [SerializeField]
    private float stepX = 1f, stepY = 1f;


    private PointInfo[,] mapArray;
    public PointInfo[,] MapArray { get { return mapArray; } }
    private Camera mainCamera;
    private float startX, startY;
    private PointInfo startPoint, endPoint;
    private WaitForSeconds second;
    private List<PointData> passList;
    private PointEnum nowState;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        second = new WaitForSeconds(0.1f);
        InitMap();
        SpawnMapRandomPoint();
    }

    private void Update()
    {
        UpdateCheck();
    }

    public void UpdateCheck()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartAStar();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            GetPointByInput(Input.mousePosition);
        }
        else if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            nowState = PointEnum.Normal;
        }
        else if (Input.GetKeyDown(KeyCode.F1))
        {
            nowState = PointEnum.Start;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            nowState = PointEnum.End;
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            nowState = PointEnum.Hard;
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            nowState = PointEnum.Cannot;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            nowState = PointEnum.None;
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            SpawnMapRandomPoint(true);
        }
        else if (Input.GetKeyDown(KeyCode.F8))
        {
            PointFinding.IsEight = !PointFinding.IsEight;
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            foreach (var item in mapArray)
            {
                if(item.IsPass)
                {
                    item.SwitchPassColor();
                }
            }
        }
    }

    /// <summary>
    /// 初始化地图
    /// </summary>
    private void InitMap()
    {
        mapArray = new PointInfo[width, height];
        Transform pointRoot = new GameObject("PointRoot").transform;
        startX = -width / 2;
        startY = -height / 2;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var go = Instantiate(pointPrefab, pointRoot);
                go.transform.position = new Vector3(startX + j * stepX, 0, startY + i * stepY);
                mapArray[j, i] = new PointInfo(j, i, PointEnum.Normal, go);
                go.name = string.Format("Point_{0}_{1}", j, i);
            }
        }
    }

/// <summary>
/// 生成地图上的点
/// </summary>
/// <param name="isReset">是否刷新地图</param>
    private void SpawnMapRandomPoint(bool isReset=false)
    {
        if (isReset)
        {//重新生成
            passList = null;
            foreach (var item in mapArray)
            {
                item.Reset();
            }
        }

        //产生起点
        RandomDoThing(item => { startPoint = item; item.PointType = PointEnum.Start; });
        //产生终点
        RandomDoThing(item => { endPoint = item; item.PointType = PointEnum.End; });
        //产生难走的路
        for (int i = 0; i < 300; i++)
        {
            RandomDoThing(item => item.PointType = PointEnum.Hard);
        }
        //产生禁止通行的路
        for (int i = 0; i < 150; i++)
        {
            RandomDoThing(item => item.PointType = PointEnum.Cannot);
        }
    }

    /// <summary>
    /// 在地图上随机一个正常的点  做事情
    /// </summary>
    /// <param name="act"></param>
    private void RandomDoThing(Action<PointInfo> act)
    {
        for (int i = 0; i < width * height; i++)
        {//这里不用while 防止死循环
            int rdX = Random.Range(0, width);
            int rdY = Random.Range(0, height);
            var item = mapArray[rdX, rdY];
            if (item.PointType == PointEnum.Normal)
            {
                act(item);
                return;
            }
        }

        List<PointInfo> list = new List<PointInfo>();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var item = mapArray[j, i];
                if (item.PointType == PointEnum.Normal)
                {
                    list.Add(item);
                    return;
                }
            }
        }

        if (list.Count > 0)
        {
            int rd = Random.Range(0, list.Count);
            act(list[rd]);
        }
    }

    /// <summary>
    /// 根据输入的位置得到点
    /// </summary>
    /// <param name="inputPos">输入的位置</param>
    private void GetPointByInput(Vector2 inputPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(inputPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var point = GetPosByHitPoint(hit.point);
            ChangeRoad(point);
        }
    }

    /// <summary>
    /// 根据点击的点 得到位置
    /// 位置 从0开始  到 最大-1
    /// </summary>
    /// <param name="hitPoint">点击的点</param>
    /// <returns></returns>
    public Vector2Int GetPosByHitPoint(Vector3 hitPoint)
    {
        int x = (int)(hitPoint.x - startX + stepX / 2);
        int y = (int)(hitPoint.z - startY + stepY / 2);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 开始寻路
    /// </summary>
    public void StartAStar()
    {
        foreach(var item in mapArray)
        {
            item.SetPass(false);
        }
        long time1 = DateTime.Now.Ticks;

        PointFinding.FindPath(startPoint.pointPos, endPoint.pointPos, out passList);

        Debug.Log(TimeSpan.FromTicks(DateTime.Now.Ticks-time1));
        StartCoroutine(StartPassList());
    }

    /// <summary>
    /// 慢慢展示列表
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartPassList()
    {
        foreach(var item in passList)
        {
            mapArray[item.pointPos.x, item.pointPos.y].SetPass();
        }

        while (passList != null && passList.Count > 0)
        {
            var item = passList[0];
            mapArray[item.pointPos.x, item.pointPos.y].SetPassColor();
            passList.Remove(item);
            yield return second;
        }
    }

    public void ChangeRoad(Vector2Int point)
    {
        if (nowState == PointEnum.None)
        {
            return;
        }
        var item = mapArray[point.x, point.y];
        if (item.PointType == PointEnum.Start || item.PointType == PointEnum.End)
        {
            return;
        }
        if (nowState == PointEnum.Start)
        {
            startPoint.PointType = PointEnum.Normal;
            startPoint = item;
        }
        else if (nowState == PointEnum.End)
        {
            endPoint.PointType = PointEnum.Normal;
            endPoint = item;
        }

        item.PointType = nowState;
    }
}
