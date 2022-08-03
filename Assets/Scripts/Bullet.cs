using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Bullet : MonoBehaviour
{

    public abstract void Fire(Vector3 pos, Vector3 dir, float force);

}
