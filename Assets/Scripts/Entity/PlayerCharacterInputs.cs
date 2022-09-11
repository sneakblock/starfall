using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerCharacterInputs
{
    public float MoveAxisForward;
    public float MoveAxisRight;
    public Quaternion CameraRotation;
    public bool JumpDown;
    public bool Primary;
    public bool Aim;
    public bool Ability;
    public bool Gadget;
    public bool Reload;
    public Vector3 Target;
}
