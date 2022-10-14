using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{

    void Damage(float damage);

    void Heal(int healing);

    void Kill();

    bool IsAlive();
    
}
