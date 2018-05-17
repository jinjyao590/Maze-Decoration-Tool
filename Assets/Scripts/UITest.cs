using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UITest : MonoBehaviour {

    Rect sizeOfBox = new Rect(0, 0, 500, 500);
    public GameObject wallPref;
    public GameObject nodePref;
    public GameObject decalCube;

    //public Material mat;
    public Color baseColor;

    public float baseSize;
    public float baseRot;

    public float maxDist = 100f;

    public int mazeSize;
    public float offset;
    //public int HEBP;
    //public int VEBP;
    public int sId;
    public int eId;
    public string sWall;
    public string eWall;
    public string parent;


    public Material[] mat;
    public int[] amount;
    public bool[] hasColor;
    public bool[] onlyOnSolutionPath;

    Material[] posMat;
    Material[] negMat;
    int posAmount;
    int negAmount;

    Hashtable curveObjects;
    AmountController amountController;
    GraphNode[] graphNodes;
    GameObject[] GameObjectsOfNodes;
    Node[] nodes;
    ArrayList decalClones = new ArrayList();

    float h = 500;
    bool folded = false;
    string lb = "-";

    char[][] vebps, hebps;

    void Start()
    {
        
        Utils.readEBPFromFile("Assets/Scripts/Maze10x10TextFile.txt", 10, out vebps, out hebps);
        Utils.PrintEBPS("V", vebps);
        Utils.PrintEBPS("H", hebps);

        Object[] posM = Resources.LoadAll("posMat", typeof(Material));
        Object[] negM = Resources.LoadAll("negMat", typeof(Material));

        posMat = new Material[posM.Length];
        negMat = new Material[negM.Length];
        hasColor = new bool[posM.Length + negM.Length];
        onlyOnSolutionPath = new bool[posM.Length + negM.Length];

        for (int i = 0; i < posM.Length; ++i)
        {
            onlyOnSolutionPath[i] = false;
            hasColor[i] = true;
            posMat[i] = (Material) posM[i];
            if (i == 0) hasColor[i] = true;
        }
        for (int i = 0; i < negM.Length; ++i)
        {
            onlyOnSolutionPath[posM.Length + i] = false;
            hasColor[posM.Length + i] = true;
            negMat[i] = (Material) negM[i];
        }
            
    }

    void OnGUI()
    {
        // Make a group on the center of the screen
        GUI.BeginGroup(new Rect(50, 50, 200, h));
        // All rectangles are now adjusted to the group. (0,0) is the topleft corner of the group.

        // We'll make a box so you can see where the group is on-screen.
        GUI.Box(new Rect(0, 0, 200, 500), "Menu");
        ButtonFold(new Rect(170, 10, 20, 20), lb);

        // Load curve
        ButtonLoadFunction(new Rect(10, 40, 180, 30), "Load Func");
        ButtonLoadPhysicalMaze(new Rect(10, 80, 180, 30), "Load Physical Maze");
        ButtonLoadAbstractMaze(new Rect(10, 120, 180, 30), "Load Abstract Maze");
        ButtonLoadDecals(new Rect(10, 160, 180, 30), "Load Decals");
        ButtonWander1(new Rect(10, 200, 180, 30), "Play1");
        ButtonWander2(new Rect(10, 240, 180, 30), "Play2");

        // End the group we started above. This is very important to remember!
        GUI.EndGroup();
    }

    void ButtonFold(Rect rect, string label)
    {
        if (GUI.Button(rect, label))
        {
            if (folded)
            {
                h = 500;
                folded = false;
                lb = "-";
            }
            else
            {
                h = 35;
                folded = true;
                lb = "+";
            }
        }
    }

    // new Rect(10, 80, 80, 30)
    void ButtonLoadFunction(Rect rect, string label)
    {
        if (GUI.Button(rect, label))
        {
            curveObjects = new Hashtable();
            //amountController = new AmountController();

            FunctionGenerator.LoadCurve("CurvePanel", curveObjects);

            FunctionGenerator.LoadFunction("FunctionPanel", curveObjects);

            //FunctionGenerator.LoadAmountFunctionCurve("Others", amountController);
        }
    }

    void ButtonLoadPhysicalMaze(Rect rect, string label)
    {
        if (GUI.Button(rect, label))
        {
            //SimpleTransform[] simpleTransformsOfWalls = MazeGenerator.GetWallsPositions(mazeSize, HEBP, VEBP, offset, sWall, eWall);

            SimpleTransform[] simpleTransformsOfWalls = MazeGenerator.GetWallsPositions(10, hebps[0], vebps[0], offset, "V_Wall_0", "V_Wall_109");
            MazeGenerator.InstantiateWalls(simpleTransformsOfWalls, wallPref, parent);
        }
    }

    void ButtonLoadAbstractMaze(Rect rect, string label)
    {
        if (GUI.Button(rect, label))
        {
            //SimpleTransform[] simpleTransformsOfNodes = MazeGenerator.GetNodesPositions(mazeSize, offset);
            SimpleTransform[] simpleTransformsOfNodes = MazeGenerator.GetNodesPositions(10, offset);

            // Initialize the graph, specify the start and end.
            graphNodes = new GraphNode[simpleTransformsOfNodes.Length];
            GraphNode.Start = sId;
            GraphNode.End = eId;

            GameObjectsOfNodes = MazeGenerator.InstantiateNodes(simpleTransformsOfNodes, ref graphNodes, nodePref, parent);

            nodes = GraphNode.ConnectNodes(GameObjectsOfNodes, ref graphNodes, maxDist);

            GraphNode.GetPathOfMaze(graphNodes);

            GraphNode.ComputeScalarField(graphNodes);
        }
    }

    void ButtonLoadDecals(Rect rect, string label)
    {
        if (GUI.Button(rect, label))
        {
            CleanDecal();
            decalClones = new ArrayList();

            //ArrayList decalObjects = DecalGenerator.GenerateDecalGameObjects(graphNodes, nodes, GameObjectsOfNodes, amount, hasColor, onlyOnSolutionPath, parent, decalCube, curveObjects,
            //                                                               baseSize, baseRot, baseColor, maxDist, amountController);

            ArrayList decalObjects = DecalGenerator.GenerateDecalGameObjects(graphNodes, nodes, GameObjectsOfNodes, posAmount, negAmount, posMat.Length, negMat.Length, 
                                                                             hasColor, onlyOnSolutionPath, parent, decalCube, curveObjects,
                                                                             baseSize, baseRot, baseColor, maxDist);

            //DecalGenerator.ApplyDecalGameObjects(decalObjects, decalCube, mat, "Maze", decalClones);
            DecalGenerator.ApplyDecalGameObjects(decalObjects, decalCube, posMat, negMat, "Maze", decalClones);

            initialItween();
        }
    }

    void CleanDecal()
    {
        for (int i = 0; i < decalClones.Count; ++i)
        {
            Destroy((GameObject) decalClones[i]);
        }
    }

    Hashtable args;
    Vector3[] paths;
    public float m_speed;
    public GameObject ctrlor;
    void initialItween()
    {
        args = new Hashtable();

        //int[] pathIdx = { 0, 10, 20, 21, 22, 23, 24, 14, 15, 16, 26, 36, 46, 56, 66, 76, 77, 78, 79, 89, 88, 87, 86, 85, 75, 74, 84, 94, 95, 96, 97, 98, 99 };
        int[] pathIdx = { 0, 10, 20, 21, 22, 23, 24, 14, 15, 16, 26, 36, 46, 56, 66, 76, 77, 78, 79, 89, 88, 87, 86, 85, 75, 74, 84, 94, 93, 83, 82, 81,
                          71, 72, 73, 63, 53, 43, 42, 52, 62, 61, 60, 50, 40, 30, 31, 32, 33, 34, 44, 54, 64, 65, 55, 45, 35, 25 };

        paths = new Vector3[pathIdx.Length];
        for (int i = 0; i < pathIdx.Length; ++i)
        {
            paths[i] = GameObject.Find("Node_" + pathIdx[i]).transform.position;
            paths[i].y += 5f;
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

        //iTween.MoveTo(ctrlor, args);   //让模型开始寻路  这个方法和放在Update函数里的MoveUpdate()方法效果一样  Update里为了控制是否移动    
    }

    void ButtonWander1(Rect rect, string label)
    {
        if (GUI.Button(rect, label))
        {
            int[] pathIdx = { 0, 10, 20, 21, 22, 23, 24, 14, 15, 16, 26, 36, 46, 56, 66, 76, 77, 78, 79, 89, 88, 87, 86, 85, 75, 74, 84, 94, 95, 96, 97, 98, 99 };
            paths = new Vector3[pathIdx.Length];
            for (int i = 0; i < pathIdx.Length; ++i)
            {
                paths[i] = GameObject.Find("Node_" + pathIdx[i]).transform.position;
                paths[i].y += 5f;
            }
            args["path"] = paths;
            //initialItween();
            iTween.MoveTo(ctrlor, args);
        }
    }

    void ButtonWander2(Rect rect, string label)
    {
        if (GUI.Button(rect, label))
        {
            int[] pathIdx = { 0, 10, 20, 21, 22, 23, 24, 14, 15, 16, 26, 36, 46, 56, 66, 76, 77, 78, 79, 89, 88, 87, 86, 85, 75, 74, 84, 94, 93, 83, 82, 81,
                          71, 72, 73, 63, 53, 43, 42, 52, 62, 61, 60, 50, 40, 30, 31, 32, 33, 34, 44, 54, 64, 65, 55, 45, 35, 25 };
            paths = new Vector3[pathIdx.Length];
            for (int i = 0; i < pathIdx.Length; ++i)
            {
                paths[i] = GameObject.Find("Node_" + pathIdx[i]).transform.position;
                paths[i].y += 5f;
            }
            args["path"] = paths;
            //initialItween();
            iTween.MoveTo(ctrlor, args);
        }
    }
}
