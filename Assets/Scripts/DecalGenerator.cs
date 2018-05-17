using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalGenerator {

    public static Vector3 GetDecalLocalPosition(GameObject decalCube, Vector3 castOrigin, Vector3 castDir, float maxDst, NeighborInfo neiObj, string parent, out Vector3 hitPoint, out Vector3 normal)
    {
        Vector3 local = Vector3.down;
        RaycastHit hit;
        hitPoint = Vector3.down;
        normal = Vector3.down;

        if (Physics.Raycast(castOrigin, castDir, out hit, maxDst))
        {
            // If hit the wall we supposed
            if (hit.transform.parent != null && hit.transform.parent.name.Equals(neiObj.Id))
            {
                GameObject obj = Object.Instantiate<GameObject>(decalCube, hit.point, Quaternion.LookRotation(hit.normal));
                hitPoint = hit.point;
                normal = hit.normal;
                obj.transform.Rotate(new Vector3(90, 0, 0));
                obj.transform.parent = GameObject.Find(parent).transform;
                local = obj.transform.localPosition;
                Object.Destroy(obj);
            }
        }

        //Debug.Log("Local: " + local);

        return local; // if -1, -1, -1, miss
    }

    public static Vector3 GetCastDirection(Constant.DirectionType type, string parent)
    {
        float dirX = -1, dirY = -1, dirZ = -1;

        if (type == Constant.DirectionType.Left || type == Constant.DirectionType.Right)
        {
            dirX = type == Constant.DirectionType.Left ? Random.Range(Vector3.left.x, 0) : Random.Range(0, Vector3.right.x);
            dirY = Random.Range(0, Vector3.up.y);
            dirZ = Random.Range(Vector3.back.z, Vector3.forward.z);
        }
        else if (type == Constant.DirectionType.Forward || type == Constant.DirectionType.Backward)
        {
            dirX = Random.Range(Vector3.left.x, Vector3.right.x);
            dirY = Random.Range(0, Vector3.up.y);
            dirZ = type == Constant.DirectionType.Forward ? Random.Range(0, Vector3.forward.z) : Random.Range(Vector3.back.z, 0);
        }

        Quaternion rotParent = GameObject.Find(parent).transform.rotation;
        return rotParent * new Vector3(dirX, dirY, dirZ);
    }

    public static ArrayList GenerateDecalObjects(Constant.DirectionType dirType, int[] amount, bool[] hasColor, bool[] onlyOnSolutionPath, GameObject nodeObj, NeighborInfo neiObj, GraphNode[] graphNodes, Node node, string parent,
                                             GameObject decalCube, Hashtable curveObjects, float baseSize, float baseRot, Color baseColor, float maxDist, string options,
                                                 AmountController amountController)
    {
        ArrayList objs = new ArrayList();
        Quaternion rotParent = GameObject.Find(parent).transform.rotation;
        for (int i = 0; i < amount.Length; ++i)
        {
            int flag = 0, temp = amount[i];
            for (int j = 0; j < amount[i]; ++j)
            {
                Vector3 hitPoint, normal;
                Vector3 decalLocal = GetDecalLocalPosition(decalCube, nodeObj.transform.position, GetCastDirection(dirType, parent), maxDist, neiObj, parent,
                                                           out hitPoint, out normal);

                if (Mathf.Abs(decalLocal.y + 1) > Mathf.Epsilon)
                {
                    Hashtable indepVar = GetIndepVar(decalLocal, graphNodes, node, options);
                    if (flag == 0)
                    {
                        Debug.Log("For " + i + ": Before - " + amount[i]);

                        amount[i] = UpdateAmount(amountController, i, amount[i], indepVar);
                        if (amount[i] < 1)
                        {
                            continue;
                        }
                        Debug.Log("For " + i + ": After - " + amount[i]);
                        flag = 1;
                    }

                    DecalInfo obj = new DecalInfo();

                    obj.hitPoint = hitPoint;
                    obj.normal = normal;
                    obj.IdxOfMat = i;

                    UpdateAll(obj, indepVar, baseSize, baseRot, baseColor, hasColor[i], curveObjects);
                    if (obj.IsOnSolutionPath || !onlyOnSolutionPath[i])
                    {
                        objs.Add(obj);
                    }

                }
            }
            amount[i] = temp;
        }

        return objs;
    }

    public static ArrayList GenerateDecalObjects(Constant.DirectionType dirType, int posAmount, int negAmount, int noOfPos, int noOfNeg, bool[] hasColor, 
                                                 bool[] onlyOnSolutionPath, GameObject nodeObj, NeighborInfo neiObj, GraphNode[] graphNodes, Node node, string parent,
                                                 GameObject decalCube, Hashtable curveObjects, float baseSize, float baseRot, Color baseColor, float maxDist, string options)
    {
        ArrayList objs = new ArrayList();
        Quaternion rotParent = GameObject.Find(parent).transform.rotation;

        Hashtable indepVarForAmount = GetIndepVar(graphNodes, node, options);
        posAmount = UpdatePosAmount(posAmount, indepVarForAmount, curveObjects);
        negAmount = UpdateNegAmount(negAmount, indepVarForAmount, curveObjects);

        if (posAmount < 0) posAmount = 0;
        if (negAmount < 0) negAmount = 0;

        for (int i = 0; i < posAmount + negAmount; ++i)
        {
            Vector3 hitPoint, normal;
            Vector3 decalLocal = GetDecalLocalPosition(decalCube, nodeObj.transform.position, GetCastDirection(dirType, parent), maxDist, neiObj, parent,
                                                       out hitPoint, out normal);

            if (Mathf.Abs(decalLocal.y + 1) > Mathf.Epsilon)
            {
                Hashtable indepVar = GetIndepVar(decalLocal, graphNodes, node, options);

                DecalInfo obj = new DecalInfo();

                obj.hitPoint = hitPoint;
                obj.normal = normal;

                if (i < posAmount)
                {
                    obj.IdxOfPosMat = Random.Range(0, noOfPos - 1);
                    obj.IdxOfNegMat = -1;
                    UpdateAll(obj, indepVar, baseSize, baseRot, baseColor, hasColor[obj.IdxOfPosMat], curveObjects);
                    if (obj.IsOnSolutionPath || !onlyOnSolutionPath[obj.IdxOfPosMat])
                    {
                        objs.Add(obj);
                    }
                }
                else
                {
                    obj.IdxOfNegMat = Random.Range(0, noOfNeg - 1);
                    obj.IdxOfPosMat = -1;
                    UpdateAll(obj, indepVar, baseSize, baseRot, baseColor, hasColor[noOfPos + obj.IdxOfNegMat], curveObjects);
                    if (obj.IsOnSolutionPath || !onlyOnSolutionPath[noOfPos + obj.IdxOfNegMat])
                    {
                        objs.Add(obj);
                    }
                }
            }
        }

        return objs;
    }

    public static int UpdateAmount(AmountController amountController, int matId, int baseAmount, Hashtable indepVar)
    {
        for (int k = 0; k < amountController.curves.Count; ++k)
        {
            if ((int)amountController.matIds[k] == matId)
            {
                int iv = (int)(float)indepVar[(Constant.CurveIndepType)amountController.indepVars[k]];
                baseAmount = (int)((AnimationCurve)amountController.curves[k]).Evaluate(iv);
            }
        }
        return baseAmount;
    }

    public static int UpdatePosAmount(int baseAmount, Hashtable indepVar, Hashtable curveObjects)
    {
        return (int) DecalInfo.UpdateDepVar(baseAmount, indepVar, Constant.CurveDepType.PosAmount, curveObjects);
    }

    public static int UpdateNegAmount(int baseAmount, Hashtable indepVar, Hashtable curveObjects)
    {
        return (int) DecalInfo.UpdateDepVar(baseAmount, indepVar, Constant.CurveDepType.NegAmount, curveObjects);
    }

    public static void UpdateAll(DecalInfo obj, Hashtable indepVar, float baseSize, float baseRot, Color baseColor, bool hasColor, Hashtable curveObjects)
    {
        if (indepVar.Count > 0)
        {
            obj.UpdateSize(baseSize, indepVar, curveObjects);
            obj.UpdateRotation(baseRot, indepVar, curveObjects);
            float colorH, colorS, colorV;
            Color.RGBToHSV(baseColor, out colorH, out colorS, out colorV);
            obj.UpdateColorH(colorH, indepVar, curveObjects);
            obj.UpdateColorS(colorS, indepVar, curveObjects);
            obj.UpdateColorV(colorV, indepVar, curveObjects);
            obj.UpdateColorAlpha(baseColor.a, indepVar, curveObjects);
            obj.HasColor = hasColor;
            if ((int)indepVar[Constant.CurveIndepType.SF_SolutionPath] == 0)
            {
                obj.IsOnSolutionPath = true;
            }
            else
            {
                obj.IsOnSolutionPath = false;
            }
        }
    }

    public static ArrayList GenerateDecalGameObjects(GraphNode[] graphNodes, Node[] nodes, GameObject[] gameObjectsOfNodes, int[] amount, bool[] hasColor, bool[] onlyOnSolutionPath, string parent,
                                                     GameObject decalCube, Hashtable curveObjects, float baseSize, float baseRot, Color baseColor, float maxDist,
                                                     AmountController amountController)
    {
        ArrayList decalObjects = new ArrayList();

        for (int i = 0; i < nodes.Length; ++i)
        {
            Constant.NeighborType[] neighborTypes = { nodes[i].Left.Type, nodes[i].Right.Type, nodes[i].Backward.Type, nodes[i].Forward.Type };
            Constant.DirectionType[] directionTypes = { Constant.DirectionType.Left, Constant.DirectionType.Right, Constant.DirectionType.Backward, Constant.DirectionType.Forward };
            NeighborInfo[] neighborObjects = { nodes[i].Left, nodes[i].Right, nodes[i].Backward, nodes[i].Forward };
            string[] options = { "H", "H", "V", "V" };

            for (int j = 0; j < directionTypes.Length; ++j)
            {
                if (neighborTypes[j] == Constant.NeighborType.Wall) // To assure left neighbor is wall
                {
                    ArrayList objs = GenerateDecalObjects(directionTypes[j], amount, hasColor, onlyOnSolutionPath, gameObjectsOfNodes[i], neighborObjects[j], graphNodes, nodes[i], parent,
                                                          decalCube, curveObjects, baseSize, baseRot, baseColor, maxDist, options[j], amountController);
                    decalObjects.AddRange(objs);
                }
            }
        }
        return decalObjects;
    }

    public static ArrayList GenerateDecalGameObjects(GraphNode[] graphNodes, Node[] nodes, GameObject[] gameObjectsOfNodes, int posAmount, int negAmount, int noOfPos, int noOfNeg, 
                                                     bool[] hasColor, bool[] onlyOnSolutionPath, string parent,
                                                     GameObject decalCube, Hashtable curveObjects, float baseSize, float baseRot, Color baseColor, float maxDist)
    {
        ArrayList decalObjects = new ArrayList();

        for (int i = 0; i < nodes.Length; ++i)
        {
            Constant.NeighborType[] neighborTypes = { nodes[i].Left.Type, nodes[i].Right.Type, nodes[i].Backward.Type, nodes[i].Forward.Type };
            Constant.DirectionType[] directionTypes = { Constant.DirectionType.Left, Constant.DirectionType.Right, Constant.DirectionType.Backward, Constant.DirectionType.Forward };
            NeighborInfo[] neighborObjects = { nodes[i].Left, nodes[i].Right, nodes[i].Backward, nodes[i].Forward };
            string[] options = { "H", "H", "V", "V" };

            for (int j = 0; j < directionTypes.Length; ++j)
            {
                if (neighborTypes[j] == Constant.NeighborType.Wall) // To assure left neighbor is wall
                {
                    ArrayList objs = GenerateDecalObjects(directionTypes[j], posAmount, negAmount, noOfPos, noOfNeg, hasColor, onlyOnSolutionPath, 
                                                          gameObjectsOfNodes[i], neighborObjects[j], graphNodes, nodes[i], parent,
                                                          decalCube, curveObjects, baseSize, baseRot, baseColor, maxDist, options[j]);
                    decalObjects.AddRange(objs);
                }
            }
        }
        return decalObjects;
    }

    public static Hashtable GetIndepVar(Vector3 decalLocal, GraphNode[] graphNodes, Node node, string options)
    {
        Hashtable indepVar = new Hashtable();
        if (decalLocal.y >= 0)
        {
            Hashtable sf = Utils.Mapping(decalLocal, node, graphNodes, options);

            float PN_XY = Mathf.PerlinNoise(decalLocal.x, decalLocal.y);
            float PN_ZY = Mathf.PerlinNoise(decalLocal.z, decalLocal.y);

            indepVar.Add(Constant.CurveIndepType.PN_XY, PN_XY);
            indepVar.Add(Constant.CurveIndepType.PN_ZY, PN_ZY);
            indepVar.Add(Constant.CurveIndepType.SF_Start, (float)sf[Constant.DistanceType.DistFromStart]);
            indepVar.Add(Constant.CurveIndepType.SF_End, (float)sf[Constant.DistanceType.DistFromEnd]);
            indepVar.Add(Constant.CurveIndepType.SF_DeadEnd, (float)sf[Constant.DistanceType.DistFromDeadEnd]);
            indepVar.Add(Constant.CurveIndepType.SF_SolutionPath, (int)sf[Constant.DistanceType.DistFromSolutionPath]);
        }
        return indepVar;
    }

    public static Hashtable GetIndepVar(GraphNode[] graphNodes, Node node, string options)
    {
        Hashtable indepVar = new Hashtable();

        GraphNode curNode = graphNodes[int.Parse(node.Id.Split('_')[1])];

        indepVar.Add(Constant.CurveIndepType.SF_Start, (int) curNode.ScalarField[Constant.DistanceType.DistFromStart]);
        indepVar.Add(Constant.CurveIndepType.SF_End, (int) curNode.ScalarField[Constant.DistanceType.DistFromEnd]);
        indepVar.Add(Constant.CurveIndepType.SF_DeadEnd, (int) curNode.ScalarField[Constant.DistanceType.DistFromDeadEnd]);
        indepVar.Add(Constant.CurveIndepType.SF_SolutionPath, (int) curNode.ScalarField[Constant.DistanceType.DistFromSolutionPath]);

        return indepVar;
    }

    public static void InstantiateDecal(DecalInfo obj, GameObject decalCube, Material mat, string parent, ArrayList decalClones)
    {
        GameObject go = Object.Instantiate(decalCube, obj.hitPoint, Quaternion.LookRotation(obj.normal));
        go.transform.Rotate(new Vector3(90, 0, 0));
        go.transform.parent = GameObject.Find(parent).transform;
        go.transform.Rotate(obj.Rot);
        go.transform.localScale = obj.Size;

        ThreeEyedGames.Decal target = go.GetComponent<ThreeEyedGames.Decal>();
        target.Material = new Material(mat);
        Material m = target.Material;
        Color color = m.color;
        if (obj.HasColor)
        {
            color = Color.HSVToRGB(obj.ColorH, obj.ColorS, obj.ColorV);
        }
        color.a = obj.ColorAlpha;

        m.color = color;

        decalClones.Add(go);
    }

    public static void ApplyDecalGameObjects(ArrayList decalObjects, GameObject decalCube, Material[] mat, string parent, ArrayList decalClones)
    {
        foreach (DecalInfo decalobj in decalObjects)
        {
            InstantiateDecal(decalobj, decalCube, mat[decalobj.IdxOfMat], parent, decalClones);
        }
    }

    // For Graffiti
    public static void ApplyDecalGameObjects(ArrayList decalObjects, GameObject decalCube, Material[] posMat, Material[] negMat, string parent, ArrayList decalClones)
    {
        foreach (DecalInfo decalobj in decalObjects)
        {
            if (decalobj.IdxOfPosMat != -1)
            {
                InstantiateDecal(decalobj, decalCube, posMat[decalobj.IdxOfPosMat], parent, decalClones);
            }
            else if (decalobj.IdxOfNegMat != -1)
            {
                InstantiateDecal(decalobj, decalCube, negMat[decalobj.IdxOfNegMat], parent, decalClones);
            }
        }
    }
}
