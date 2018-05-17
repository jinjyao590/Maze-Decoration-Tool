using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenRoam : MonoBehaviour {

    public Transform[] paths;  //路径寻路中的所有的关键点 使用空物体路径点 
    //public Vector3[] paths;  //也可以使用这句代码直接给路径点的位置赋值
    public Hashtable args;   //设置路径键值对
    public float m_speed = 10f;  //漫游的速度
    public bool isMove = true;  //是否漫游

    void Start()
    {
        args = new Hashtable();

        int[] pathIdx = { 0, 10, 20, 21, 22, 23, 24, 14, 15, 16, 26, 36, 46, 56, 66, 76, 77, 78, 79, 89, 88, 87, 86, 85, 75, 74, 84, 94, 95, 96, 97, 98, 99 };

        paths = new Transform[pathIdx.Length];
        for (int i = 0; i < pathIdx.Length; ++i)
        {
            paths[i] = GameObject.Find("Node_" + pathIdx[i]).transform;
        }

        args.Add("path", paths);  //设置路径的点  

        args.Add("easeType", iTween.EaseType.linear);  //设置类型为线性，线性效果会好一些。  

        args.Add("speed", m_speed);  //设置寻路的速度  

        args.Add("movetopath", false);   //是否先从原始位置走到路径中第一个点的位置  

        args.Add("orienttopath", true);//是否让模型始终面朝当面目标的方向，拐弯的地方会自动旋转模型,如果你发现你的模型在寻路的时候始终都是一个方向那么一定要打开这个  

        args.Add("looktarget", Vector3.zero);   //移动过程中面朝一个点  

        args.Add("loopType", "loop");   //三个循环类型 none loop pingpong(一般 循环 来回)  

        args.Add("NamedValueColor", "_SpecColor");  //这个是处理颜色的。可以看源码的那个枚举。  

        args.Add("delay", 0f);   //延迟执行时间  

        iTween.MoveTo(gameObject, args);   //让模型开始寻路  这个方法和放在Update函数里的MoveUpdate()方法效果一样  Update里为了控制是否移动    
    }

    void Update()
    {
        RoamControl();
    }

    /// <summary>
    /// 控制动画暂停和继续的函数
    /// </summary>
    void RoamControl()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            iTween.Pause();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            iTween.Resume();
        }
    }

    void OnDrawGizmos()
    {
        //在Scene视图中绘制出路径  
        iTween.DrawLine(paths, Color.yellow);
        iTween.DrawPath(paths, Color.red);
    }
}
