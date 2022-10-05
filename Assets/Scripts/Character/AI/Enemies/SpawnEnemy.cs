using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject _enemy;

    private void Awake()
    {
        GameObject _thisEnemy = Instantiate(_enemy, this.transform.position, Quaternion.identity);
        _thisEnemy.name = _enemy.name;
    }
}
