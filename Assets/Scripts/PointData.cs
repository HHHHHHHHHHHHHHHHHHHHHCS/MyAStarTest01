using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PointEnum
{
    None,//无效的空值
    Start,//蓝色-起点
    End,//黑色-终点
    Normal,//白色-普通的路
    Hard,//黄色-难走的路
    Cannot,//红色-不能走的路
}

public struct PointPos
{
    public int x, y;

    public PointPos(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public bool Equals(PointPos obj)
    {
        return x == obj.x && y == obj.y;
    }

    public override string ToString()
    {
        return string.Format("x:{0} , y:{1}", x, y);
    }
}

public class PointData
{
    public PointPos pointPos;
    public double g, h;
    public PointData parent;

    public double F { get { return g + h; } }


    public PointData(PointPos _AStartPoint, double _g, double _h, PointData _parent)
    {
        pointPos = _AStartPoint;
        g = _g;
        h = _h;
        parent = _parent;
    }
}

public class PointInfo
{
    public PointPos pointPos;
    public GameObject go;
    private PointEnum pointType;
    private MeshRenderer renderer;
    public bool IsPass { get; private set; }
    public bool IsPassColor { get; private set; }

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

    public PointInfo(int _x, int _y, PointEnum _pointType, GameObject _go)
    {
        pointPos = new PointPos(_x, _y);
        pointType = _pointType;
        go = _go;
        if (go != null)
        {
            renderer = go.GetComponent<MeshRenderer>();
        }
    }

    public void SwitchPassColor()
    {

        if (IsPass)
        {
            if(IsPassColor)
            {
                IsPassColor = false;
                ResetColor();
            }
            else
            {
                IsPassColor = true;
                SetPassColor();
            }
        }
    }

    public void SetPass(bool bo = true)
    {
        IsPass = bo;
        IsPassColor = bo;
        if(!bo)
        {
            ResetColor();
        }
    }

    public void SetPassColor()
    {
        ChangeColor(Color.cyan);
    }

    public void ResetColor()
    {
        ChangeColor();
    }

    public void ChangeColor(Color? _color = null)
    {
        if (renderer != null)
        {
            MaterialPropertyBlock prop = new MaterialPropertyBlock();
            Color col;
            if (_color != null)
            {
                col = _color.Value;
            }
            else
            {
                if (PointType == PointEnum.None)
                {
                    return;
                }
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
                    default:
                        col = new Color(1, 0, 1, 1);
                        break;
                }
            }

            prop.SetColor("_Color", col);
            renderer.SetPropertyBlock(prop);
        }
    }

    public void Reset()
    {
        PointType = PointEnum.Normal;
        IsPass = false;
    }
}