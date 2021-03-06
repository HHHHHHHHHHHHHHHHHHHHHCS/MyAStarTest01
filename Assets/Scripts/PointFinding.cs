﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointFinding
{
    #region 固定属性
    /// <summary>
    /// 直线距离
    /// </summary>
    private const double StraightLine = 1.0;
    /// <summary>
    /// 斜线距离
    /// </summary>
    private const double SlantLine = 1.4;
    /// <summary>
    /// 四个方向的数组
    /// </summary>
    private static readonly int[,] fourDirects = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
    /// <summary>
    /// 八个方向的数组
    /// </summary>
    private static readonly int[,] eightDirects = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 1 }, { -1, 1 }, { -1, 1 }, { 1, -1 } };
    #endregion

    #region 传入属性

    /// <summary>
    /// 传入的地图的属性
    /// </summary>
    public static PointInfo[,] MainMap = null;

    /// <summary>
    /// 地图最大尺寸
    /// </summary>
    public static PointPos Max_PNT;

    /// <summary>
    /// 起点
    /// </summary>
    public static PointPos Start_Pnt;

    /// <summary>
    /// 终点
    /// </summary>
    public static PointPos End_Pnt;

    /// <summary>
    /// 是八方向的计算
    /// </summary>
    public static bool IsEight;
    #endregion

    #region G和H相关
    /// <summary>
    /// 计算G
    /// </summary>
    /// <param name="data">传入的点信息</param>
    /// <returns>G值的计算</returns>
    public static double CalG(PointData data, PointPos newPos, bool isStraightLine)
    {
        double result = data.g + (isStraightLine ? StraightLine : SlantLine);
        double temp = 0;
        var _type = MainMap[newPos.x, newPos.y].PointType;
        switch (_type)
        {
            case PointEnum.Normal:
                temp = 1;
                break;
            case PointEnum.Hard:
                temp = 10;
                break;
        }
        return result + temp;
    }

    /// <summary>
    /// 计算H的抽象方法
    /// 里面包含了权重的计算
    /// </summary>
    /// <param name="pnt">传入的点信息</param>
    /// <returns>H值的计算的抽象方法</returns>
    public static double CalH(PointPos pnt)
    {
        return HManhattanDistance(pnt);
    }

    /// <summary>
    /// 获取曼哈顿距离
    /// </summary>
    /// <param name="pnt">传入的点信息</param>
    /// <returns>H值的曼哈顿距离</returns>
    private static double HManhattanDistance(PointPos pnt)
    {
        return Math.Abs(pnt.x - End_Pnt.x) + Math.Abs(pnt.y - End_Pnt.y);
    }

    /// <summary>
    /// 获取欧几里得距离
    /// </summary>
    /// <param name="pnt">传入的点信息</param>
    /// <returns>H值的欧几里得距离</returns>
    private static double HEuclidianDistance(PointPos pnt)
    {
        return Math.Sqrt(HPowEuclidianDistance(pnt));
    }

    /// <summary>
    /// 获取欧几里得距离
    /// </summary>
    /// <param name="pnt"></param>
    /// <returns>H值的Pow欧几里得离</returns>
    private static double HPowEuclidianDistance(PointPos pnt)
    {
        return Math.Pow(pnt.x - End_Pnt.x, 2) + Math.Pow(pnt.y - End_Pnt.y, 2);
    }
    #endregion

    /// <summary>
    /// 传入地图信息
    /// </summary>
    /// <param name="s">起点</param>
    /// <param name="e">终点</param>
    /// <returns>能否进行寻路</returns>
    private static bool GenerateMap(PointPos s, PointPos e)
    {
        if (MainMap == null)
        {
            var main = MainGameManager.Instance;
            MainMap = main.MapArray;
            Max_PNT = new PointPos(main.Width, main.Height);
        }

        if (s.Equals(e))
        {
            Debug.Log("起点和终点相同");
            return false;
        }

        Start_Pnt = s;
        End_Pnt = e;

        return true;
    }

    private static bool Search(out List<PointData> pathList)
    {
        //是否走过
        bool[,] goMap = new bool[Max_PNT.x, Max_PNT.y];
        //起点数据
        PointData startData = new PointData(Start_Pnt, 0, 0, null);
        //最后一个点的数据 用于反推
        PointData endData = null;
        //用List集合做"开启列表"  来记录扩展的点
        List<PointData> openList = new List<PointData>();
        //把起点放入开启列表
        openList.Add(startData);

        //是否完成
        bool isFinish = false;
        while (!isFinish && openList.Count > 0)
        {//找到终点或者"开启列表"为空的时候退出循环
            openList.Sort((x, y) => { return x.F.CompareTo(y.F); });
            PointData data = openList[0];
            openList.RemoveAt(0);
            PointPos point = data.pointPos;

            //将取出的点表示为已访问点
            if (!goMap[point.x, point.y])
            {
                goMap[point.x, point.y] = true;
            }
            else
            {
                continue;
            }

            int[,] nowDir = IsEight ? eightDirects : fourDirects;

            for (int i = 0; i < nowDir.GetLength(0); i++)
            {
                bool isStraight = IsEight ? Mathf.Abs(nowDir[i, 0]) != Mathf.Abs(nowDir[i, 1]) : true;
                PointPos newPoint = new PointPos(point.x + nowDir[i, 0], point.y + nowDir[i, 1]);
                if (newPoint.x >= 0 && newPoint.x < Max_PNT.x && newPoint.y >= 0 && newPoint.y < Max_PNT.y)
                {
                    if (MainMap[newPoint.x, newPoint.y].PointType == PointEnum.Cannot)
                    {
                        continue;
                    }
                    //查找判断点是否在"开启列表"中
                    PointData tempData = openList.Find(x => x.pointPos.Equals(newPoint));

                    double tempG = CalG(data, newPoint, isStraight);
                    if (tempData != null)
                    {
                        if (tempData.g > tempG)
                        {
                            tempData.g = tempG;
                            tempData.parent = data;
                        }
                    }
                    else
                    {
                        double h = CalH(newPoint);
                        PointData newData = new PointData(newPoint, tempG, h, data);
                        openList.Add(newData);

                        if (MainMap[newPoint.x, newPoint.y].PointType == PointEnum.End)
                        {
                            endData = newData;
                            isFinish = true;
                            break;
                        }
                    }
                }
            }
        }

        //反向查找 找出路径
        pathList = new List<PointData>();
        pathList.Add(endData);

        PointData pointData = endData;
        while (pointData != null)
        {
            PointPos point = pointData.pointPos;
            if (goMap[point.x, point.y])
            {
                goMap[point.x, point.y] = false;
                pathList.Add(pointData);
            }
            pointData = pointData.parent;
        }
        pathList.Add(startData);
        pathList.Reverse();

        return true;
    }

    public static bool FindPath(PointPos s, PointPos e, out List<PointData> pathList)
    {
        pathList = null;
        return GenerateMap(s, e) && Search(out pathList);
    }
}
