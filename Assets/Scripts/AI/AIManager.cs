using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    private GameObject[] _enemySpawns;
    private GameObject[] _enemyRespawns;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // Do at start of each level
        Initialize();
    }

    public void Initialize()
    {
        _enemySpawns = GameObject.FindGameObjectsWithTag("EnemySpawn"); // Grab all enemy spawn points in current level
        _enemyRespawns = GameObject.FindGameObjectsWithTag("EnemyRespawn"); // Grab all enemy respawn points in current level
    }
}
