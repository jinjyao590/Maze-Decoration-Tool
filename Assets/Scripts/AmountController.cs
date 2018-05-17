using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmountController : MonoBehaviour {

    public ArrayList indepVars;
    public ArrayList matIds;
    public ArrayList curves;

    public AmountController()
    {
        matIds = new ArrayList();
        indepVars = new ArrayList();
        curves = new ArrayList();
    }
}
