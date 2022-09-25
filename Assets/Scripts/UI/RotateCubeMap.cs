using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCubeMap : MonoBehaviour
{
    public float RotateSpeed = 1.2f;

    public void Update() {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * RotateSpeed);
    }
}
