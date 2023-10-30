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
        Debug.Log("��������" + _ikList.Count);
        CreateBoneLine();
    }

    void LateUpdate()
    {
        //���Ƶ㷢���仯��ʱ�����IK
        if (_lastPos != _leaf.position)
        {
            //IK����
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
    /// ��ʼ�������ڵ�
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
    /// ��ʼ����������
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
    /// �����Ǽ�
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
    /// ���¹���λ��
    /// </summary>
    private void UpdateBone()
    {
        for (int i = 0, len = _ikList.Count; i < len; i++)
        {
            //���¹ؽ�λ��
            IKData parent = _ikList[i];
            parent.Node.position = parent.Pos;
            //���¹Ǽ�λ��
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
    /// �������
    /// </summary>
    /// <param name="target"></param>
    private void IKBack(Vector3 target)
    {
        //ĩ�ڵ�λ����Ϊtarget
        _ikList[_ikList.Count - 1].Pos = target;
        //��������
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
                //����Ȩ�غ���ת���������������
                normal = Quaternion.Euler(new Vector3(2f, 0, 0)) * normal;
            }

            parent.Pos = child.Pos + normal * parent.Length;
            Debug.Log(i + ":" + normal);
        }
    }

    /// <summary>
    /// �������
    /// </summary>
    private void IKForward()
    {
        //��������
        for (int i = 0, len = _ikList.Count - 1; i < len; i++)
        {
            IKData parent = _ikList[i];
            IKData child = _ikList[i + 1];
            Vector3 normal = (child.Pos - parent.Pos).normalized;
            child.Pos = parent.Pos + normal * parent.Length;
        }
    }
}
