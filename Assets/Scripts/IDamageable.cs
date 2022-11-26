using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{

    void Damage(float damage);

    void Heal(float healing);

    void Kill();

    void StartBleeding(float totalDamage, float duration);

    bool IsBleeding();

    void StopBleeding();

    void UpdateBleed();

    bool IsAlive();
    
}
