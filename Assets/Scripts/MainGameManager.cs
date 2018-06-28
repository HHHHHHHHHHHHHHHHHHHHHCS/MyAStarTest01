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

    private float startX, startY;

    private PointInfo[,] mapArray;
    public PointInfo[,] MapArray { get { return mapArray; } }
    private Camera mainCamera;

    private PointInfo startPoint, endPoint;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        InitMap();
        RandomSpawnPoint();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetPointByInput(Input.mousePosition);
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            StartAStar();
        }
    }

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

    private void RandomSpawnPoint()
    {
        //产生起点
        RandomDoThing(item => { startPoint = item; item.PointType = PointEnum.Start; } );
        //产生终点
        RandomDoThing(item => {  endPoint = item ; item.PointType = PointEnum.End; });
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

    private void GetPointByInput(Vector2 inputPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(inputPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GetPosByHitPoint(hit.point);
        }
    }

    public Vector2Int GetPosByHitPoint(Vector3 hitPoint)
    {//从0开始  到最大-1
        int x = (int)(hitPoint.x - startX + stepX / 2);
        int y = (int)(hitPoint.z - startY + stepY / 2);
        Debug.LogFormat("HitPoint:({0},{1})",x,y);
        return new Vector2Int(x, y);
    }

    public void StartAStar()
    {
        List<PointData> list;
        PointFinding.FindPath(startPoint.pointPos, endPoint.pointPos, out list);
        foreach(var item in list)
        {
            Debug.Log(mapArray[item.pointPos.x, item.pointPos.y].PointType == PointEnum.End);
            mapArray[item.pointPos.x, item.pointPos.y].SetPass();
        }
    }
}
