using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode {

    public static int Start { get; set; }
    public static int End { get; set; }
    public static HashSet<int> DeadEnd = new HashSet<int>();
    public static ArrayList SolutionPath = new ArrayList();
    public static ArrayList AllPath = new ArrayList();

    public string Id { get; set; }
    public ArrayList NeighborNode { get; set; } // Index of node
    public Hashtable ScalarField { get; set; }

    public GraphNode(string id)
    {
        Id = id;
        NeighborNode = new ArrayList();

        ScalarField = new Hashtable();
        ScalarField.Add(Constant.DistanceType.DistFromStart, int.MaxValue);
        ScalarField.Add(Constant.DistanceType.DistFromEnd, int.MaxValue);
        ScalarField.Add(Constant.DistanceType.DistFromDeadEnd, int.MaxValue);
        ScalarField.Add(Constant.DistanceType.DistFromSolutionPath, int.MaxValue);
    }

    public static void ComputeScalarField(GraphNode[] graphNodes)
    {
        foreach (ArrayList ap in AllPath)
        {
            // From 0 to Count - 1
            // If begin with start
            if ((int)ap[0] == Start)
            {
                for (int i = 0; i < ap.Count; ++i)
                {
                    graphNodes[(int)ap[i]].ScalarField[Constant.DistanceType.DistFromStart] = i;
                }
            }
            else if ((int)ap[0] == End)
            { // If begin with end
                for (int i = 0; i < ap.Count; ++i)
                {
                    graphNodes[(int)ap[i]].ScalarField[Constant.DistanceType.DistFromEnd] = i;
                }
            }

            // If end with dead end
            if (DeadEnd.Contains((int)ap[ap.Count - 1]))
            {
                for (int i = 0; i < ap.Count; ++i)
                {
                    graphNodes[(int)ap[i]].ScalarField[Constant.DistanceType.DistFromDeadEnd] =
                        Mathf.Min((int)graphNodes[(int)ap[i]].ScalarField[Constant.DistanceType.DistFromDeadEnd], ap.Count - 1 - i);
                }
            }
        }

        foreach (int sp in SolutionPath)
        {
            graphNodes[sp].ScalarField[Constant.DistanceType.DistFromSolutionPath] = 0;
        }

        Utils.PrintScalarField(graphNodes);
    }

    public static void GetPathOfMaze(GraphNode[] graphNodes)
    {
        // From start node
        GraphNode nodeStart = graphNodes[Start];
        bool[] visited = new bool[graphNodes.Length];
        ArrayList path = new ArrayList();
        visited[Start] = true;
        path.Add(Start);
        Utils.BFSHelper(nodeStart, visited, path, graphNodes, AllPath, DeadEnd, ref SolutionPath);

        // From end node
        GraphNode nodeEnd = graphNodes[End];
        visited = new bool[graphNodes.Length];
        path = new ArrayList();
        visited[End] = true;
        path.Add(End);
        Utils.BFSHelper(nodeEnd, visited, path, graphNodes, AllPath, DeadEnd, ref SolutionPath);

        Utils.PrintAllPath(AllPath);
    }

    public static Node[] ConnectNodes(GameObject[] gameObjectsOfNodes, ref GraphNode[] graphNodes, float maxDist)
    {
        Node[] nodes = new Node[gameObjectsOfNodes.Length];
        for (int i = 0; i < gameObjectsOfNodes.Length; ++i)
        {
            Node node = new Node();
            node.Id = gameObjectsOfNodes[i].name;
            node.transform = gameObjectsOfNodes[i].transform;

            GraphNode graphNode = graphNodes[i];

            NeighborInfo neiObj = new NeighborInfo();
            // Z-axis+ is forward
            neiObj = GetNeighborNode(graphNode, gameObjectsOfNodes[i].transform.position, gameObjectsOfNodes[i].transform.forward, maxDist);
            node.Forward = neiObj;

            // Z-axis- is backward
            neiObj = GetNeighborNode(graphNode, gameObjectsOfNodes[i].transform.position, -gameObjectsOfNodes[i].transform.forward, maxDist);
            node.Backward = neiObj;

            // X-axis+ is right
            neiObj = GetNeighborNode(graphNode, gameObjectsOfNodes[i].transform.position, gameObjectsOfNodes[i].transform.right, maxDist);
            node.Right = neiObj;

            // X-axis- is left
            neiObj = GetNeighborNode(graphNode, gameObjectsOfNodes[i].transform.position, -gameObjectsOfNodes[i].transform.right, maxDist);
            node.Left = neiObj;

            nodes[i] = node;
        }
        return nodes;
    }

    static NeighborInfo GetNeighborNode(GraphNode graphNode, Vector3 pos, Vector3 dir, float maxDist)
    {
        NeighborInfo neiObj = new NeighborInfo();
        RaycastHit hit;
        if (Physics.Raycast(pos, dir, out hit, maxDist))
        {
            if (hit.transform.name == "Wall")
            {
                neiObj.Id = hit.transform.parent.name;
                neiObj.Type = Constant.NeighborType.Wall;
                neiObj.transform = hit.transform.parent;
            }
            else if (hit.transform.name.Substring(0, 4) == "Node")
            {
                neiObj.Id = hit.transform.name;
                neiObj.Type = Constant.NeighborType.Node;
                neiObj.transform = hit.transform;

                int idx = int.Parse(neiObj.Id.Split('_')[1]);
                graphNode.NeighborNode.Add(idx);
            }
        }
        else // Not hit anything
        {
            neiObj.Type = Constant.NeighborType.Empty;
        }

        return neiObj;
    }
}
