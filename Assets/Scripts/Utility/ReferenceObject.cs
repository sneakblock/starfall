using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceObject : MonoBehaviour
{
    private MeshRenderer obj;
    
    // Hides reference obj upon entering Play mode.
    void Start()
    {
        obj = GetComponent<MeshRenderer>();
        obj.enabled = false;
    }
}
