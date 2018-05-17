using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalInfo
{
    public int IdxOfPosMat { get; set; }
    public int IdxOfNegMat { get; set; }

    public int IdxOfMat { get; set; }
    public bool HasColor { get; set; }
    public bool IsOnSolutionPath { get; set; }

    public float ColorH { get; set; }
    public float ColorS { get; set; }
    public float ColorV { get; set; }
    public float ColorAlpha { get; set; }

    public Vector3 Size { get; set; }
    public Vector3 Rot { get; set; }
    public Vector3 LocalPos { get; set; }

    public Vector3 hitPoint { get; set; }
    public Vector3 normal { get; set; }

    public static int UpdateDepVar(int baseValue, Hashtable indepVar, Constant.CurveDepType type, Hashtable curveObjects)
    {
        int value = baseValue;
        if (curveObjects[type] != null)
        {
            for (int k = 0; k < ((CurveInfo)curveObjects[type]).InDepVars.Count; ++k)
            {
                float iv = (int)indepVar[(Constant.CurveIndepType)((CurveInfo)curveObjects[type]).InDepVars[k]];
                float dv = value;
                if (k < ((CurveInfo)curveObjects[type]).CurveVars.Count)
                {
                    dv = Utils.Function((AnimationCurve)((CurveInfo)curveObjects[type]).CurveVars[k], iv);
                }
                else if (k < ((CurveInfo)curveObjects[type]).FunctionTypeVars.Count)
                {
                    dv = Utils.Function((Constant.FunctionType)((CurveInfo)curveObjects[type]).FunctionTypeVars[k], iv);
                    Debug.Log("IV: " + iv + ", Got DV From customized function: " + dv);
                }
                //float dv = MasterProject.Util.Function((AnimationCurve)((CurveObject)curveObjects[type]).CurveVars[k], iv);
                value = (int) GetValue(value, (Constant.CurveOperationType)((CurveInfo)curveObjects[type]).OperationVars[k], dv);
            }
        }
        return value;
    }

    public static float GetValue(float baseValue, Constant.CurveOperationType opType, float offset)
    {
        if (opType == Constant.CurveOperationType.Add)
        {
            baseValue += offset;
        }
        else if (opType == Constant.CurveOperationType.Sub)
        {
            baseValue -= offset;
        }
        else if (opType == Constant.CurveOperationType.Mul)
        {
            baseValue *= offset;
        }
        return baseValue;
    }

    public static float UpdateDepVar(float baseValue, Hashtable indepVar, Constant.CurveDepType type, Hashtable curveObjects)
    {
        float value = baseValue;
        if (curveObjects[type] != null)
        {
            for (int k = 0; k < ((CurveInfo)curveObjects[type]).InDepVars.Count; ++k)
            {
                float iv = (float) indepVar[(Constant.CurveIndepType)((CurveInfo)curveObjects[type]).InDepVars[k]];
                float dv = value;
                if (k < ((CurveInfo)curveObjects[type]).CurveVars.Count)
                {
                    dv = Utils.Function((AnimationCurve)((CurveInfo)curveObjects[type]).CurveVars[k], iv);
                }
                else if (k < ((CurveInfo)curveObjects[type]).FunctionTypeVars.Count)
                {
                    dv = Utils.Function((Constant.FunctionType)((CurveInfo)curveObjects[type]).FunctionTypeVars[k], iv);
                    Debug.Log("IV: " + iv + ", Got DV From customized function: " + dv);
                }
                //float dv = MasterProject.Util.Function((AnimationCurve)((CurveObject)curveObjects[type]).CurveVars[k], iv);
                value = GetValue(value, (Constant.CurveOperationType)((CurveInfo)curveObjects[type]).OperationVars[k], dv);
            }
        }
        return value;
    }

    public float UpdateSize(float baseSZ, Hashtable indepVar, Hashtable curveObjects)
    {
        float szPartial = UpdateDepVar(baseSZ, indepVar, Constant.CurveDepType.Size, curveObjects);
        Size = new Vector3(szPartial, 1f, szPartial);
        return szPartial;
    }

    public float UpdateRotation(float baseROT, Hashtable indepVar, Hashtable curveObjects)
    {
        float rotPartial = UpdateDepVar(baseROT, indepVar, Constant.CurveDepType.Rotation, curveObjects);
        Rot = new Vector3(0, rotPartial, 0);
        return rotPartial;
    }

    public float UpdateColorH(float baseColorH, Hashtable indepVar, Hashtable curveObjects)
    {
        ColorH = UpdateDepVar(baseColorH, indepVar, Constant.CurveDepType.ColorH, curveObjects);
        return ColorH;
    }

    public float UpdateColorS(float baseColorS, Hashtable indepVar, Hashtable curveObjects)
    {
        ColorS = UpdateDepVar(baseColorS, indepVar, Constant.CurveDepType.ColorS, curveObjects);
        return ColorS;
    }

    public float UpdateColorV(float baseColorV, Hashtable indepVar, Hashtable curveObjects)
    {
        ColorV =  UpdateDepVar(baseColorV, indepVar, Constant.CurveDepType.ColorV, curveObjects);
        return ColorV;
    }

    public float UpdateColorAlpha(float baseColorAlpha, Hashtable indepVar, Hashtable curveObjects)
    {
        ColorAlpha = UpdateDepVar(baseColorAlpha, indepVar, Constant.CurveDepType.ColorAlpha, curveObjects);
        return ColorAlpha;
    }

}
