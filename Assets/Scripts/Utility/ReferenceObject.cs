using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceObject : MonoBehaviour
{
    private MeshRenderer obj;
    
    // Hides reference obj upon entering Play mode.
    void Start()
    {
        obj = this.gameObject.GetComponent<MeshRenderer>();
        Color newColor = obj.material.color;
        newColor.a = 0;
        obj.material.color = newColor;
    }
}
