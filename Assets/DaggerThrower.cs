using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerThrower : MonoBehaviour
{
    public DaggerAbility daggerAbility;
    
    public void ExecuteThrow()
    {
        daggerAbility.InitializeThrownDaggers();
    }
}
