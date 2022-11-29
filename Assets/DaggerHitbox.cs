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
        if (struckCharacter.IsBleeding())
        {
            Burst(struckCharacter);
        }
        else
        {
            BleedEnemy(struckCharacter, other.ClosestPoint(transform.position));
        }
    }

    private void Burst(SCharacter target)
    {
        var burstEffect = Instantiate(_parent.burstEffect, target.transform.position, Quaternion.identity);
        Destroy(burstEffect, 1f);
        target.Damage(_parent.bleedDamage * 2f);
    }

    private void BleedEnemy(SCharacter target, Vector3 struckPoint)
    {
        var impactEffect = Instantiate(_parent.bleedEffect, struckPoint, Quaternion.identity);
        Destroy(impactEffect, 1f);
        target.StartBleeding(_parent.bleedDamage, _parent.bleedDuration);
    }
}
