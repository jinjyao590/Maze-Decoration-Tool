using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionGenerator : MonoBehaviour {

    public static void LoadCurve(string label, Hashtable curveObjects)
    {
        CurveScript[] curveScripts = GameObject.Find(label).GetComponents<CurveScript>();

        foreach (CurveScript cs in curveScripts)
        {
            if (curveObjects[cs.DepVar] == null)
            {
                curveObjects.Add(cs.DepVar, new CurveInfo());
            }
            ((CurveInfo)curveObjects[cs.DepVar]).InDepVars.Add(cs.IndepVar);
            ((CurveInfo)curveObjects[cs.DepVar]).CurveVars.Add(cs.CurveVar);
            ((CurveInfo)curveObjects[cs.DepVar]).OperationVars.Add(cs.OperationVar);
        }
    }

    public static void LoadFunction(string label, Hashtable curveObjects)
    {
        FunctionScript[] functionScripts = GameObject.Find(label).GetComponents<FunctionScript>();

        foreach (FunctionScript fs in functionScripts)
        {
            if (curveObjects[fs.DepVar] == null)
            {
                curveObjects.Add(fs.DepVar, new CurveInfo());
            }
            ((CurveInfo)curveObjects[fs.DepVar]).InDepVars.Add(fs.IndepVar);
            ((CurveInfo)curveObjects[fs.DepVar]).OperationVars.Add(fs.OperationVar);
            ((CurveInfo)curveObjects[fs.DepVar]).FunctionTypeVars.Add(fs.functionType);
        }
    }

    public static void LoadAmountFunctionCurve(string label, AmountController amountController)
    {
        AmountControlScript[] amountControlScripts = GameObject.Find(label).GetComponents<AmountControlScript>();

        foreach (AmountControlScript cs in amountControlScripts)
        {
            amountController.matIds.Add(cs.matId);
            amountController.indepVars.Add(cs.IndepVar);
            amountController.curves.Add(cs.CurveVar);
        }
    }
}
