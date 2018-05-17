using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveInfo {

    public ArrayList InDepVars;
    public ArrayList CurveVars; // AnimationCurve
    public ArrayList OperationVars; // Constant.CurveOperationType
    public ArrayList FunctionTypeVars;

    public CurveInfo()
    {
        InDepVars = new ArrayList();
        CurveVars = new ArrayList();
        OperationVars = new ArrayList();
        FunctionTypeVars = new ArrayList();
    }
}
