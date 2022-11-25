using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerHitbox : MonoBehaviour
{
    private Dagger _parent;

    private void Awake()
    {
        _parent = GetComponentInParent<Dagger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
        if (_parent.daggerState is not (DaggerState.Inbound or DaggerState.Outbound)) return;
        var struckCharacter = other.gameObject.GetComponent<SCharacter>();
        if (!struckCharacter) return;
        struckCharacter.StartBleeding(_parent.bleedDamage, _parent.bleedDuration);
    }
}
