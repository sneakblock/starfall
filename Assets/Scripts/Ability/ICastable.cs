using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICastable
{
    bool IsCasting();

    // gets called once at the beginning of the cast.
    void OnCastStarted();

    // gets called if the ability wants to do something in the meantime it is casting.
    // for example: healing
    void DuringCast();

    // gets called only once
    void OnCastEnded();
}


