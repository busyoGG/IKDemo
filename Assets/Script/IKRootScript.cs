using System;
using System.Collections.Generic;
using UnityEngine;

public class IKRootScript : MonoBehaviour
{
    public List<IKData> _ikList = new List<IKData>();
    public Transform _leaf;
    public Vector3 _lastPos;
    public List<GameObject> _boneLine = new List<GameObject>();

    void Start()
    {
        InitBone(transform);
        InitLength();
        _leaf = GameObject.Find("CtrlPoint").transform;
        _lastPos = _leaf.position;
        Debug.Log("骨骼数量" + _ikList.Count);
        CreateBoneLine();
    }

    void LateUpdate()
    {
        //控制点发生变化的时候计算IK
        if (_lastPos != _leaf.position)
        {
            //IK迭代
            for (int i = 0; i < 3; i++)
            {
                IKBack(_leaf.position);
                IKForward();
            }
            _lastPos = _leaf.position;
            UpdateBone();
        }
    }
    /// <summary>
    /// 初始化骨骼节点
    /// </summary>
    /// <param name="trans"></param>
    private void InitBone(Transform trans)
    {
        IKData bone = new IKData();
        bone.Pos = trans.position;
        bone.Node = trans;
        _ikList.Add(bone);

        if (trans.childCount > 0)
        {
            Transform child = trans.GetChild(0);
            InitBone(child);
        }
    }
    /// <summary>
    /// 初始化骨骼长度
    /// </summary>
    private void InitLength()
    {
        for (int i = 0, len = _ikList.Count - 1; i < len; i++)
        {
            IKData first = _ikList[i];
            IKData second = _ikList[i + 1];
            first.Length = Math.Abs((second.Pos - first.Pos).magnitude);
        }
    }
    /// <summary>
    /// 创建骨架
    /// </summary>
    private void CreateBoneLine()
    {
        for (int i = 0, len = _ikList.Count - 1; i < len; i++)
        {
            IKData parent = _ikList[i];
            IKData child = _ikList[i + 1];
            Vector3 center = (parent.Pos + child.Pos) * 0.5f;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = center;
            cube.transform.localScale = new Vector3(0.2f, 0.2f, parent.Length);
            cube.transform.LookAt(child.Pos);
            _boneLine.Add(cube);
        }
    }
    /// <summary>
    /// 更新骨骼位置
    /// </summary>
    private void UpdateBone()
    {
        for (int i = 0, len = _ikList.Count; i < len; i++)
        {
            //更新关节位置
            IKData parent = _ikList[i];
            parent.Node.position = parent.Pos;
            //更新骨架位置
            if (i < len - 1)
            {
                IKData child = _ikList[i + 1];
                GameObject cube = _boneLine[i];
                Vector3 center = (parent.Pos + child.Pos) * 0.5f;
                cube.transform.position = center;
                cube.transform.LookAt(child.Pos);
            }
        }
    }

    /// <summary>
    /// 反向迭代
    /// </summary>
    /// <param name="target"></param>
    private void IKBack(Vector3 target)
    {
        //末节点位置置为target
        _ikList[_ikList.Count - 1].Pos = target;
        //遍历迭代
        for (int i = _ikList.Count - 1; i > 1; i--)
        {
            IKData parent = _ikList[i - 1];
            IKData child = _ikList[i];

            Vector3 nextBone = (parent.Pos - child.Pos).normalized;
            Vector3 lastBone = (parent.Pos - _ikList[i - 2].Pos).normalized;
            Vector3 side = Vector3.Cross(nextBone, lastBone);
            Debug.Log(i + ":" + side);


            Vector3 normal = (parent.Pos - child.Pos).normalized;

            if (side.x <= 0 || side.y <= 0 || side.z <= 0)
            {
                //按照权重和旋转方向调整方向向量
                normal = Quaternion.Euler(new Vector3(2f, 0, 0)) * normal;
            }

            parent.Pos = child.Pos + normal * parent.Length;
            Debug.Log(i + ":" + normal);
        }
    }

    /// <summary>
    /// 正向迭代
    /// </summary>
    private void IKForward()
    {
        //遍历迭代
        for (int i = 0, len = _ikList.Count - 1; i < len; i++)
        {
            IKData parent = _ikList[i];
            IKData child = _ikList[i + 1];
            Vector3 normal = (child.Pos - parent.Pos).normalized;
            child.Pos = parent.Pos + normal * parent.Length;
        }
    }
}
