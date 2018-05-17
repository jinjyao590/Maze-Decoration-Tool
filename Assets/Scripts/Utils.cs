using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils {

    public static void readEBPFromFile(string pathOfFile, int sizeOfMaze, out char[][] VEBPs, out char[][] HEBPs)
    {
        string[] lines = System.IO.File.ReadAllLines(pathOfFile);
        Debug.Log(lines.Length);

        VEBPs = new char[lines.Length / 3][];
        HEBPs = new char[lines.Length / 3][];
        int k = 0;
        for (int i = 0; i < lines.Length; i += 3)
        {
            VEBPs[k] = lines[i].ToCharArray();
            HEBPs[k] = lines[i + 1].ToCharArray();
            VEBPs[k] = EBPConverter(VEBPs[k], sizeOfMaze);
            HEBPs[k] = EBPConverter(HEBPs[k], sizeOfMaze);

            k++;
        }
    }

    public static char[] ToEBPArray(int m, int n, int EBP)
    {
        string s_ebp = Convert.ToString(EBP, 2);
        char[] bits_ebp = s_ebp.PadLeft(m * n, '0').ToCharArray();

        return bits_ebp;
    }

    public static char[] EBPConverter(char[] oldEBP, int sizeOfMaze)
    {
        char[] newEBP = new char[oldEBP.Length];

        int k = 0;
        for (int i = 0; i < sizeOfMaze - 1; ++i)
        {
            for (int j = 0; j < sizeOfMaze; ++j)
            {
                newEBP[k++] = oldEBP[i + j * (sizeOfMaze - 1)];
            }
        }

        return newEBP;
    }

    public static void BFSHelper(GraphNode graphNode, bool[] visited, ArrayList path, GraphNode[] graphNodes, 
                                 ArrayList allPath, HashSet<int> deadEnd, ref ArrayList solutionPath) // Notice you need to deep copy the subArray
    {
        if (graphNode.NeighborNode.Count == 1 && path.Count > 1)
        {
            int idxOfLeave = (int)path[path.Count - 1];
            // If the leave node is dead end
            if (int.Parse(graphNodes[idxOfLeave].Id.Split('_')[1]) != GraphNode.Start && int.Parse(graphNodes[idxOfLeave].Id.Split('_')[1]) != GraphNode.End)
            {
                deadEnd.Add(idxOfLeave);
            }

            // If the leave node is end, then the path is solution path
            if (int.Parse(graphNodes[idxOfLeave].Id.Split('_')[1]) == GraphNode.End)
            {
                solutionPath = new ArrayList(path);
            }

            allPath.Add(new ArrayList(path));
            return;
        }

        foreach (int idx in graphNode.NeighborNode)
        {
            if (!visited[idx])
            {
                visited[idx] = true;
                path.Add(idx);
                BFSHelper(graphNodes[idx], visited, path, graphNodes, allPath, deadEnd, ref solutionPath);
                path.RemoveAt(path.Count - 1);
            }
        }
    }

    public static void InitialNeighborNodes(GraphNode curNode, Transform curTransform, GraphNode[] graphNodes, NeighborInfo neiObj,
                                            out GraphNode neiNode, out Transform neiTransform, Constant.NeighborType type)
    {
        if (type == Constant.NeighborType.Node)
        {
            neiNode = graphNodes[int.Parse(neiObj.Id.Split('_')[1])];
            neiTransform = neiObj.transform;
        }
        else{
            neiNode = curNode;
            neiTransform = curTransform;
        }
    }

    public static void GetScalarField(Hashtable scalarField, out float distFromCur, float decalLocal, float curLocal, float neiLocal, GraphNode curNode, GraphNode neiNode)
    {
        if (Mathf.Abs(neiLocal - curLocal) > Mathf.Epsilon)
        {
            distFromCur = (decalLocal - curLocal) / (neiLocal - curLocal);
        }
        else
        {
            distFromCur = 0;
        }

        scalarField[Constant.DistanceType.DistFromStart] = distFromCur *
            ((int)neiNode.ScalarField[Constant.DistanceType.DistFromStart] - (int)curNode.ScalarField[Constant.DistanceType.DistFromStart]) +
             (int)curNode.ScalarField[Constant.DistanceType.DistFromStart];
        scalarField[Constant.DistanceType.DistFromEnd] = distFromCur *
            ((int)neiNode.ScalarField[Constant.DistanceType.DistFromEnd] - (int)curNode.ScalarField[Constant.DistanceType.DistFromEnd]) +
             (int)curNode.ScalarField[Constant.DistanceType.DistFromEnd];
        scalarField[Constant.DistanceType.DistFromDeadEnd] = distFromCur *
            ((int)neiNode.ScalarField[Constant.DistanceType.DistFromDeadEnd] - (int)curNode.ScalarField[Constant.DistanceType.DistFromDeadEnd]) +
             (int)curNode.ScalarField[Constant.DistanceType.DistFromDeadEnd];
        scalarField[Constant.DistanceType.DistFromSolutionPath] = curNode.ScalarField[Constant.DistanceType.DistFromSolutionPath];
    }

    public static Hashtable Mapping(Vector3 decalLocalPos, Node node, GraphNode[] graphNodes, string options)
    {
        Hashtable scalarField = new Hashtable();

        GraphNode curNode = graphNodes[int.Parse(node.Id.Split('_')[1])];
        Transform curTransform = node.transform;

        if (options.Equals("H"))
        {
            GraphNode fwdNode, bwdNode;
            Transform fwdTransform, bwdTransform;

            InitialNeighborNodes(curNode, curTransform, graphNodes, node.Forward, out fwdNode, out fwdTransform, node.Forward.Type);
            InitialNeighborNodes(curNode, curTransform, graphNodes, node.Backward, out bwdNode, out bwdTransform, node.Backward.Type);

            GetHorizontalScalarField(bwdTransform, fwdTransform, curTransform, decalLocalPos, scalarField,
                                     curNode, fwdNode, bwdNode);
        }
        else if (options.Equals("V"))
        {
            GraphNode rightNode, leftNode;
            Transform rightTransform, leftTransform;

            InitialNeighborNodes(curNode, curTransform, graphNodes, node.Right, out rightNode, out rightTransform, node.Right.Type);
            InitialNeighborNodes(curNode, curTransform, graphNodes, node.Left, out leftNode, out leftTransform, node.Left.Type);

            GetVerticalScalarField(leftTransform, rightTransform, curTransform, decalLocalPos, scalarField,
                                   curNode, rightNode, leftNode);
        }

        return scalarField;
    }

    static void GetHorizontalScalarField(Transform bwdTransform, Transform fwdTransform, Transform curTransform, Vector3 decalLocalPos, Hashtable scalarField,
                                  GraphNode curNode, GraphNode fwdNode, GraphNode bwdNode)
    {
        if (bwdTransform.localPosition.z < fwdTransform.localPosition.z) // bwd < cur < fwd
        {
            if (curTransform.localPosition.z < decalLocalPos.z) // bwd < cur < Decal < fwd
            {
                float distFromCur;
                GetScalarField(scalarField, out distFromCur, decalLocalPos.z, curTransform.localPosition.z, fwdTransform.localPosition.z, curNode, fwdNode);
            }
            else // bwd < Decal < cur < fwd
            {
                float distFromCur;
                GetScalarField(scalarField, out distFromCur, decalLocalPos.z, curTransform.localPosition.z, bwdTransform.localPosition.z, curNode, bwdNode);
            }
        }
        else // fwd < cur < bwd
        {
            if (curTransform.localPosition.z < decalLocalPos.z) // fwd < cur < Decal < bwd
            {
                float distFromCur;
                GetScalarField(scalarField, out distFromCur, decalLocalPos.z, curTransform.localPosition.z, bwdTransform.localPosition.z, curNode, bwdNode);
            }
            else // fwd < Decal < cur < bwd
            {
                float distFromCur;
                GetScalarField(scalarField, out distFromCur, decalLocalPos.z, curTransform.localPosition.z, fwdTransform.localPosition.z, curNode, fwdNode);
            }
        }
    }

    static void GetVerticalScalarField(Transform leftTransform, Transform rightTransform, Transform curTransform, Vector3 decalLocalPos, Hashtable scalarField,
                                              GraphNode curNode, GraphNode rightNode, GraphNode leftNode)
    {
        if (leftTransform.localPosition.x < rightTransform.localPosition.x) // left < cur < right
        {
            if (curTransform.localPosition.x < decalLocalPos.x) // left < cur < Decal < right
            {
                float distFromCur;
                GetScalarField(scalarField, out distFromCur, decalLocalPos.x, curTransform.localPosition.x, rightTransform.localPosition.x, curNode, rightNode);
            }
            else // left < Decal < cur < right
            {
                float distFromCur;
                GetScalarField(scalarField, out distFromCur, decalLocalPos.x, curTransform.localPosition.x, leftTransform.localPosition.x, curNode, leftNode);
            }
        }
        else // right < cur < left
        {
            if (curTransform.localPosition.x < decalLocalPos.x) // right < cur < Decal < left
            {
                float distFromCur;
                GetScalarField(scalarField, out distFromCur, decalLocalPos.x, curTransform.localPosition.x, leftTransform.localPosition.x, curNode, leftNode);
            }
            else // right < Decal < cur < left
            {
                float distFromCur;
                GetScalarField(scalarField, out distFromCur, decalLocalPos.x, curTransform.localPosition.x, rightTransform.localPosition.x, curNode, rightNode);
            }
        }
    }

    public static float Function(AnimationCurve animationCurve, float value)
    {
        return animationCurve.Evaluate(value);
    }

    /* ======= Customized Function ======= */
    public static float Function(Constant.FunctionType functionType, float value)
    {
        if (functionType == Constant.FunctionType.Multiple_3_Add_1)
        {
            return 3 * value + 1;
        }
        return value;
    }

    /* ======= Logger ======= */
    // For the test
    public static void PrintAllPath(ArrayList allPath)
    {
        foreach (ArrayList ap in allPath)
        {
            string output = "Path: ";
            foreach (var p in ap)
            {
                output += p;
                output += ", ";
            }
            Debug.Log(output);
        }
    }

    public static void PrintScalarField(GraphNode[] graphNodes)
    {
        for (int i = 0; i < graphNodes.Length; ++i)
        {
            string output = "For node " + i + ": DistFromStart = " + graphNodes[i].ScalarField[Constant.DistanceType.DistFromStart];
            output += ", DistFromEnd = " + graphNodes[i].ScalarField[Constant.DistanceType.DistFromEnd];
            output += ", DistFromDeadEnd = " + graphNodes[i].ScalarField[Constant.DistanceType.DistFromDeadEnd];
            output += ", DistFromSolutionPath = " + graphNodes[i].ScalarField[Constant.DistanceType.DistFromSolutionPath] + ".";
            Debug.Log(output);
        }
    }

    public static void PrintEBPS(string label, char[][] ebps)
    {
        int cnt = 0;
        Debug.Log("Test: " + ebps.Length);
        foreach (char[] chs in ebps)
        {
            //Debug.Log("Test: " + chs.Length);
            string output = "";
            foreach (char ch in chs)
            {
                output += ch;
            }
            Debug.Log(label + " " + cnt + ": "+ output);
            cnt++;
        }
    }
}
