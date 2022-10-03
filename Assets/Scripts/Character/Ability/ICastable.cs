using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICastable
{
    bool IsCasting();

    // gets called if the ability wants to do something in the meantime it is casting.
    // for example: healing
    void DuringCast();

    // gets called only once
    void OnCast();
}


