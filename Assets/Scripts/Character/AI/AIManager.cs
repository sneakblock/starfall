using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    public LevelData _currLevelData; // (temp) scriptable object containing info specific to the current level
    public int minRespawnDistance; // enemies will respawn at a distance beyond this threshold from the player

    private GameObject player;
    private List<GameObject> _enemyTypes = new List<GameObject>(); // (temp) list containing the different types of enemies found in the level
    private List<GameObject> _enemies = new List<GameObject>(); // list containing all of the current alive enemies in the level
    private GameObject[] _enemySpawns; // list containing all initial spawn points for enemies
    private GameObject[] _enemyRespawns; // list containing all potential respawn points for enemies
    private bool allowRespawning = false;
    private int totalRespawnedEnemies; 

    private void Awake()
    {
        // Create singleton instance.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }

        // Do this at start of each level.
        Initialize(); // (temp)
    }

    public void Initialize()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Create references pertaining to enemy spawning.
        totalRespawnedEnemies = 0;
        _enemyTypes = _currLevelData.enemyTypes;
        _enemySpawns = GameObject.FindGameObjectsWithTag("EnemySpawn"); 
        _enemyRespawns = GameObject.FindGameObjectsWithTag("EnemyRespawn"); 

        // Spawn initial group of enemies.
        foreach(GameObject sp in _enemySpawns)
        {
            GameObject currEnemy = Instantiate(_enemyTypes[0], sp.transform.position, Quaternion.identity) as GameObject; // (temp)
            _enemies.Add(currEnemy);
        }

        allowRespawning = true;
    }

    private void FixedUpdate()
    {
        // Remove dead enemies from the list of alive enemies.
        _enemies.RemoveAll(s => s == null);
        // Determine if enemies need to be respawned.
        if (totalRespawnedEnemies < _currLevelData.maxEnemyRespawns)
        {
            if (allowRespawning && _enemies.Count < _currLevelData.maxAliveEnemyCount)
            {
                RespawnEnemy();
            }
        }
    }

    private void RespawnEnemy()
    {
        // Determine which enemy respawn points are the best to use.
        List<GameObject> validEnemyRespawnPoints = new List<GameObject>();
        foreach (GameObject rp in _enemyRespawns)
        {
            float distance = Vector3.Distance(player.transform.position, rp.transform.position);
            if (distance > minRespawnDistance)
            {
                if (!rp.GetComponent<Renderer>().isVisible)
                {
                    validEnemyRespawnPoints.Add(rp);
                }
            }
        }
        // Randomly select a valid respawn point to use.
        int rand = Random.Range(0, validEnemyRespawnPoints.Count);
        GameObject respawnPoint = validEnemyRespawnPoints[rand];
        GameObject respawnedEnemy = Instantiate(_enemyTypes[0], respawnPoint.transform.position, Quaternion.identity) as GameObject;
        
        // Add the respawned enemy to the list of alive enemies.
        _enemies.Add(respawnedEnemy);
        totalRespawnedEnemies++;
        Debug.Log("respawned enemy"); // (temp)
    }
}
