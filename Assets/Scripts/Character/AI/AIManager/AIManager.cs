
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    public GameObject test;

    // Singleton reference.
    public static AIManager Instance { get; private set; }

    // List containing scriptable objects for each level.
    public List<LevelData> levelsData = new List<LevelData>();

    // Enemies must respawn at distance beyond this amount from the player.
    [SerializeField] 
    private int _minRespawnDistance = 100;
    // After an enemy respawns, there is a forced delay until the next one can respawn.
    [SerializeField]
    private float _respawnDelay = 2f;

    // Dictionary containing each enemy type found in this level and how many of each can be respawned.
    private Dictionary<GameObject, int> enemyRespawnData = new Dictionary<GameObject, int>();
    // Dictionary containing references to instantiated enemies that can be used as 'respawned' enemies when needed.
    private Dictionary<string, List<GameObject>> respawnedEnemies = new Dictionary<string, List<GameObject>>();
    // Array containing all of the respawn zones enemies can use in this level.
    private GameObject[] enemyRespawnZones;

    // Enable or disable enemy respawning.
    private bool _allowEnemyRespawning = false;
    // How many total enemies have been respawned in this level.
    private int _numRespawns;
    // The max amount of enemies that can be respawned in this level, derived from adding up the ints in enemyRespawnData.
    private int _maxNumRespawns;
    // Array containing the max alive counts for each enemy type in this level.
    private List<int> maxAliveCounts = new List<int>();

    private GameObject _player;
    private static int _currLevelNum = -1;
    private LevelData _currlevelData;

    private void Start()
    {
        // Create singleton instance.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            //TODO(Soham): This is a band-aid solution, however, I have no issue with the Instance being reinitialized on level load.
            // DontDestroyOnLoad(this.gameObject);
        }

        InitializeLevel();
        StartCoroutine(DifficultyScaling());
    }

    // Do this at the start of every level.
    public void InitializeLevel()
    {
        // Access the scriptable object for this level
        _currLevelNum++;
        _currlevelData = levelsData[_currLevelNum];

        // Temporarily disable enemy respawning
        _allowEnemyRespawning = false;

        // Grab the player game object
        _player = GameObject.FindGameObjectWithTag("Player");
        // Grab all of the respawn zones in this level
        enemyRespawnZones = GameObject.FindGameObjectsWithTag("EnemyRespawn");

        // Create a dictionary with enemy types as keys and the amount of times they respawn as values
        foreach (LevelData.Enemies enemy in _currlevelData.enemies)
        {
            enemyRespawnData.Add(enemy.enemyType, enemy.enemyNumRespawns);
            maxAliveCounts.Add(enemy.enemyNumSpawns);
        }

        // Spawn the initial group of enemies that appear in this level
        foreach (LevelData.Enemies enemy in _currlevelData.enemies)
        {
            for (int i = 0; i < enemy.enemyNumSpawns; i++)
            {
                SpawnEnemy(enemy.enemyType);
            }
        }

        // Calculate how many enemies need to be respawned in this level
        foreach (int enemyTypeMaxRespawns in enemyRespawnData.Values)
        {
            _maxNumRespawns += enemyTypeMaxRespawns;
        }

        // Instantiate the enemies that will be respawned and deactivate them until they need to actually be 'respawned' (object pooling)
        foreach (KeyValuePair<GameObject, int> keyValue in enemyRespawnData)
        {
            GameObject enemyType = keyValue.Key;
            int numRespawns = keyValue.Value;

            List<GameObject> respawnedEnemiesOfThisType = new List<GameObject>();

            for (int i = 0; i < numRespawns; i++)
            {
                GameObject _currEnemy = Instantiate(enemyType);
                _currEnemy.SetActive(false);
                _currEnemy.name = enemyType.name;
                respawnedEnemiesOfThisType.Add(_currEnemy);
            }

            respawnedEnemies.Add(enemyType.name, respawnedEnemiesOfThisType);
        }

        // Re-enable enemy respawning now that everything is set up
        _allowEnemyRespawning = true;
    }

    private void Update()
    {
        // If enemy respawning is enabled and more enemies can be respawned
        if (_allowEnemyRespawning && _numRespawns < _maxNumRespawns)
        {
            // Find all of the enemies that are currently alive and active
            List<GameObject> aliveEnemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
            aliveEnemies.RemoveAll(enemy => !enemy.activeSelf);

            // Count how many enemies of each type are currently alive and active
            int[] numAliveEnemyPerType = new int[_currlevelData.enemies.Length];
            for (int i = 0; i < _currlevelData.enemies.Length; i++)
            {
                string _enemyName = _currlevelData.enemies[i].enemyType.name;
                numAliveEnemyPerType[i] = aliveEnemies.Count(enemy => enemy.gameObject.name == _enemyName);
                
                // Respawn each enemy type until the threshold for how many of them can be alive at once is reached
                int numToRespawn = maxAliveCounts[i] - numAliveEnemyPerType[i];
                for (int j = 0; j < numToRespawn; j++)
                {
                    List<GameObject> availableBackupsOfThisEnemyType = respawnedEnemies[_enemyName];
                    // Check if inactive enemy game objects of this type are available to be used for respawning
                    if (availableBackupsOfThisEnemyType.Count > 0)
                    {
                        GameObject backupEnemy = availableBackupsOfThisEnemyType[0];
                        StartCoroutine(RespawnEnemy(backupEnemy));
                        availableBackupsOfThisEnemyType.RemoveAt(0);
                    }
                }
            }
        }

        // OLD // 
        //// If enemy respawning is enabled and more enemies can be respawned
        //if (_allowEnemyRespawning && _numRespawns < _maxNumRespawns)
        //{
        //    // Find all of the enemies that have just been killed
        //    List<GameObject> deadEnemies = GameObject.FindGameObjectsWithTag("Dead").ToList();
        //    // 'Respawn' another instance of each dead enemy type, if appropriate
        //    foreach (GameObject deadEnemy in deadEnemies)
        //    {
        //        if (!alreadyMarkedDead.Contains(deadEnemy))
        //        {
        //            List<GameObject> availableBackups = respawnedEnemies[deadEnemy.name];
        //            if (availableBackups.Count > 0)
        //            {
        //                GameObject backupEnemy = availableBackups[0];
        //                StartCoroutine(RespawnEnemy(backupEnemy));
        //                availableBackups.RemoveAt(0);
        //            }
        //            // Mark this dead enemy so it doesn't get counted in the next Update
        //            alreadyMarkedDead.Add(deadEnemy);
        //        }
        //    }
        //}

        // If no more enemies can be respawned
        if (_numRespawns >= _maxNumRespawns)
        {
            // If all enemies are dead, move to the next level
            List<GameObject> aliveEnemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
            if (aliveEnemies.Count == 0)
            {
                Debug.Log("all enemies dead");
                // TODO: go to next level
            }
        }
    }

    // Scales the difficulty of this level every one-minute interval for 5 intervals by increasing the amount of enemies that can be alive at once.
    private IEnumerator DifficultyScaling()
    {
        int minutes = 0;
        yield return new WaitForSeconds(60f);

        while (minutes < 5)
        {
            minutes++;

            for (int i = 0; i < _currlevelData.enemies.Length; i++)
            {
                maxAliveCounts[i] += _currlevelData.enemies[i].enemyIncrementMaxAlive;
            }

            Debug.Log("scaled difficulty!");
            yield return new WaitForSeconds(60f);
        }
    }

    // Function that spawns an instance of an enemy type meant to appear upon level startup.
    private void SpawnEnemy(GameObject enemy)
    {
        int rand = Random.Range(0, enemyRespawnZones.Length);
        BoxCollider randSpawnZone = enemyRespawnZones[rand].GetComponent<BoxCollider>();

        Vector3 respawnPoint = SelectRespawnPoint(randSpawnZone);

        GameObject _thisEnemy = Instantiate(enemy, respawnPoint, Quaternion.identity);
        _thisEnemy.name = enemy.name;
    }

    // Function that respawns an instance of an enemy type.
    private IEnumerator RespawnEnemy(GameObject enemy)
    {
        _allowEnemyRespawning = false;
        BoxCollider randSpawnZone = enemyRespawnZones[0].GetComponent<BoxCollider>();

        // Choose a respawn zone that is far away from the player and not currently in the camera view
        foreach (GameObject respawnZone in enemyRespawnZones)
        {
            BoxCollider volume = respawnZone.GetComponent<BoxCollider>();
            if (Vector3.Distance(volume.bounds.center, _player.transform.position) >= _minRespawnDistance)
                if (!respawnZone.GetComponent<Renderer>().isVisible)
                    randSpawnZone = respawnZone.GetComponent<BoxCollider>();
        }

        Vector3 respawnPoint = SelectRespawnPoint(randSpawnZone);

        // 'Respawn' the enemy at the chosen respawn point
        MoveEnemyPos(enemy, respawnPoint);
        _numRespawns++;

        // Wait to respawn more enemies
        yield return new WaitForSeconds(_respawnDelay);
        _allowEnemyRespawning = true;
    }

    // Helper function used to change the transform of an instantiated instance prior to 'respawning' it in the level.
    private void MoveEnemyPos(GameObject enemy, Vector3 newPos)
    {
        enemy.GetComponent<KinematicCharacterController.KinematicCharacterMotor>().enabled = false;
        enemy.GetComponent<KinematicCharacterController.KinematicCharacterMotor>().SetPosition(newPos);
        enemy.GetComponent<KinematicCharacterController.KinematicCharacterMotor>().enabled = true;
        enemy.SetActive(true);
    }

    // Helper function used to randomly select a respawn point for an enemy within a spawn zone.
    private Vector3 SelectRespawnPoint(BoxCollider spawnZone)
    {
        int count = 0;
        Vector3 respawnPoint = Vector3.zero;
        Vector3 extents = spawnZone.bounds.size / 2f;
        bool validRespawnPoint = false;

        // Try no more than 3 different spawn points to avoid infinite loop crash
        while (!validRespawnPoint && count < 3)
        {
            respawnPoint = new Vector3(
                            Random.Range(-extents.x, extents.x),
                            Random.Range(-extents.y, extents.y),
                            Random.Range(-extents.z, extents.z)
                           ) + spawnZone.bounds.center;

            // Check if this respawn point collides with another game object. Only check for game objects in the Default layer
            Collider[] hitColliders = Physics.OverlapSphere(respawnPoint, 0.5f, 1 << 0);
            if (hitColliders.Length <= 1)
                validRespawnPoint = true;

            count++;
        }

        return respawnPoint;
    }
}
