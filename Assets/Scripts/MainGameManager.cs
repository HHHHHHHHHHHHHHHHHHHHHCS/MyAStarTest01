using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PointEnum
{
    Start,//蓝色-起点
    End,//黑色-终点
    Normal,//白色-普通的路
    Hard,//黄色-难走的路
    Cannot,//红色-不能走的路
    Pass,//青色-正在行走的路
}

public class PointData
{
    public int x, y;
    public GameObject go;
    private PointEnum pointType;
    private MeshRenderer renderer;

    public PointEnum PointType
    {
        get
        {
            return pointType;
        }
        set
        {
            pointType = value;
            ChangeColor();
        }
    }

    public PointData(int _x, int _y, PointEnum _pointType, GameObject _go)
    {
        x = _x;
        y = _y;
        pointType = _pointType;
        go = _go;
        if (go != null)
        {
            renderer = go.GetComponent<MeshRenderer>();
        }
    }

    public void ChangeColor()
    {
        if (renderer != null)
        {
            MaterialPropertyBlock prop = new MaterialPropertyBlock();
            Color col ;
            switch (PointType)
            {
                case PointEnum.Start:
                    col = Color.blue;
                    break;
                case PointEnum.End:
                    col = Color.black;
                    break;
                case PointEnum.Normal:
                    col = Color.white;
                    break;
                case PointEnum.Hard:
                    col = Color.yellow;
                    break;
                case PointEnum.Cannot:
                    col = Color.red;
                    break;
                case PointEnum.Pass:
                    col = Color.cyan;
                    break;
                default:
                    col = new Color(1, 0, 1, 1);
                    break;
            }
            prop.SetColor("_Color", col);
            renderer.SetPropertyBlock(prop);
        }
    }
}


public class MainGameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject pointPrefab;

    [SerializeField]
    private int width = 85, height = 40;

    private PointData[,] mapArray;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        InitMap();
        RandomSpawnPoint();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GetPointByInput(Input.mousePosition);
        }
    }

    private void InitMap()
    {
        mapArray = new PointData[width, height];
        Transform pointRoot = new GameObject("PointRoot").transform;
        float startX = -width / 2, startY = -height / 2;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var go = Instantiate(pointPrefab, pointRoot);
                go.transform.position = new Vector3(startX + j, 0, startY + i);
                mapArray[j, i] = new PointData(j, i, PointEnum.Normal, go);
            }
        }
    }

    private void RandomSpawnPoint()
    {
        //产生起点
        RandomDoThing(item => item.PointType = PointEnum.Start);
        //产生终点
        RandomDoThing(item => item.PointType = PointEnum.End);
        //产生难走的路
        for (int i = 0; i < 200; i++)
        {
            RandomDoThing(item => item.PointType = PointEnum.Hard);
        }
        //产生禁止通行的路
        for (int i = 0; i < 100; i++)
        {
            RandomDoThing(item => item.PointType = PointEnum.Cannot);
        }
    }

    private void RandomDoThing(Action<PointData> act)
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

        List<PointData> list = new List<PointData>();
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
        if(Physics.Raycast(ray,out hit))
        {
            Debug.Log(hit.point);
        }
    }
}
