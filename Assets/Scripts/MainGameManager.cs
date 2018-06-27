using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PointEnum
{
    Start,//蓝色-起点
    End,//黑色-终点
    Normal,//白色-普通的路
    Hard,//黄色-难走的路
    Cannot,//红色-不能走的路
}

public struct PointStruct
{
    public int x, y;
    public PointEnum pointType;
    public GameObject go;

    public PointStruct(int _x, int _y, PointEnum _pointType, GameObject _go)
    {
        x = _x;
        y = _y;
        pointType = _pointType;
        go = _go;
    }
}


public class MainGameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject pointPrefab;

    [SerializeField]
    private int width = 85, height = 40;

    private PointStruct[,] mapArray;

    private void Awake()
    {
        InitMap();
    }


    private void InitMap()
    {
        mapArray = new PointStruct[width, height];
        Transform pointRoot = new GameObject("PointRoot").transform;
        float startX = -width / 2, startY = -height / 2;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var go = Instantiate(pointPrefab, pointRoot);
                go.transform.position = new Vector3(startX + j, 0, startY + i);
                mapArray[j, i] = new PointStruct(j, i, PointEnum.Normal, go);
            }
        }
    }

    private void RandomSpawnPoint()
    {
        /*-----初始化起点-----*/
        {
            for(int i=0;i<width*height;i++)
            {
                int rdX = Random.Range(0, width);
                int rdY = Random.Range(0, height);
            }
        }
    }
}
