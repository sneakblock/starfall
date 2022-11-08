
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
    // The enemies in the level that have already been killed.
    private List<GameObject> alreadyMarkedDead = new List<GameObject>();

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
            // Find all of the enemies that have just been killed
            List<GameObject> deadEnemies = GameObject.FindGameObjectsWithTag("Dead").ToList();
            // 'Respawn' another instance of each dead enemy type, if appropriate
            foreach (GameObject deadEnemy in deadEnemies)
            {
                if (!alreadyMarkedDead.Contains(deadEnemy))
                {
                    List<GameObject> availableBackups = respawnedEnemies[deadEnemy.name];
                    if (availableBackups.Count > 0)
                    {
                        GameObject backupEnemy = availableBackups[0];
                        StartCoroutine(RespawnEnemy(backupEnemy));
                        availableBackups.RemoveAt(0);
                    }
                    // Mark this dead enemy so it doesn't get counted in the next Update
                    alreadyMarkedDead.Add(deadEnemy);
                }
            }
        }
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

    private IEnumerator RespawnEnemy(GameObject enemy)
    {
        _allowEnemyRespawning = false;
        BoxCollider randSpawnZone = enemyRespawnZones[0].GetComponent<BoxCollider>();

        foreach (GameObject respawnZone in enemyRespawnZones)
        {
            BoxCollider volume = respawnZone.GetComponent<BoxCollider>();
            if (Vector3.Distance(volume.center, _player.transform.position) >= _minRespawnDistance)
                if (!respawnZone.GetComponent<Renderer>().isVisible)
                    randSpawnZone = respawnZone.GetComponent<BoxCollider>();
        }

        Vector3 respawnPoint = SelectRespawnPoint(randSpawnZone);

        // 'Respawn' the enemy at the chosen respawn point
        MoveEnemyPos(enemy, respawnPoint);
        _numRespawns++;
        // Debug.Log("respawned an enemy");

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

    private Vector3 SelectRespawnPoint(BoxCollider spawnZone)
    {
        Vector3 respawnPoint = Vector3.zero;
        Vector3 extents = spawnZone.bounds.size / 2f;
        bool validRespawnPoint = false;

        while (!validRespawnPoint)
        {
            respawnPoint = new Vector3(
                            Random.Range(-extents.x, extents.x),
                            Random.Range(-extents.y, extents.y),
                            Random.Range(-extents.z, extents.z)
                           ) + spawnZone.bounds.center;
            validRespawnPoint = true;

            // NOTE: causes a crash for some reason?? // 
            //Collider[] hitColliders = Physics.OverlapSphere(respawnPoint, 3f);
            //if (hitColliders.Length <= 1)
                //validRespawnPoint = true;
        }

        return respawnPoint;
    }
}
