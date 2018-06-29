using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * UI的制作没有做
 * F1改变起点 F2 改变终点  F3 改变困难点 F4 改变不能行走的路径  
 * F5刷新   ESC 把困难和不能行走的路变成普通的路  F8 改变成 八个方向的走法
 * 
 * 
 */
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

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        second = new WaitForSeconds(0.25f);
        InitMap();
        SpawnMapRandomPoint();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetPointByInput(Input.mousePosition);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            StartAStar();
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
    /// 生成地图上随机的点
    /// </summary>
    private void SpawnMapRandomPoint()
    {
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
            GetPosByHitPoint(hit.point);
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
        Debug.LogFormat("HitPoint:({0},{1})", x, y);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 开始寻路
    /// </summary>
    public void StartAStar()
    {
        PointFinding.FindPath(startPoint.pointPos, endPoint.pointPos, out passList);
        StartCoroutine(StartPassList());
    }

    /// <summary>
    /// 慢慢展示列表
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartPassList()
    {
        while (passList != null && passList.Count > 0)
        {
            var item = passList[0];
            mapArray[item.pointPos.x, item.pointPos.y].SetPass();
            passList.Remove(item);
            yield return second;
        }
    }
}
