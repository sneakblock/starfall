
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public struct SpawnParameters
{
    public GameObject EnemyObject;
    public int DesiredSpawnNumber;
    public float HealthBuff;
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    Insane,
    Swag
}

public class AIManager : MonoBehaviour
{
    // Singleton reference.
    public static AIManager Instance { get; private set; }

    // // List containing scriptable objects for each level.
    // public List<LevelData> levelsData = new List<LevelData>();

    public List<GameObject> activeEnemies = new();
    private Dictionary<SAi, int> typesPopulation = new();

    [Header("Enemy Respawn Settings")]
    // Enemies must respawn at distance beyond this amount from the player.
    [FormerlySerializedAs("_minRespawnDistance")] [SerializeField] 
    private int minRespawnDistance = 100;
    // After an enemy respawns, there is a forced delay until the next one can respawn.
    [FormerlySerializedAs("_respawnDelay")] [SerializeField]
    private float respawnDelay = 2f;
    [SerializeField] private int minNumberOfEnemiesDesiredOnMap = 15;
    [SerializeField] private int maxNumberOfEnemiesAllowedOnMap = 100;
    [SerializeField] private int minNumberOfRespawns = 15;
    [SerializeField] private int maxNumberOfRespawns = 50;
    
    // // Dictionary containing each enemy type found in this level and how many of each can be respawned.
    // private Dictionary<GameObject, int> enemyRespawnData = new Dictionary<GameObject, int>();
    // // Dictionary containing references to instantiated enemies that can be used as 'respawned' enemies when needed.
    // private Dictionary<string, List<GameObject>> respawnedEnemies = new Dictionary<string, List<GameObject>>();
    // // Array containing all of the respawn zones enemies can use in this level.
    private GameObject[] enemyRespawnZones;

    // Enable or disable enemy respawning.
    private bool _allowEnemyRespawning;
    // How many total enemies have been respawned in this level.
    private int _numRespawns;
    // The max amount of enemies that can be respawned in this level, derived from adding up the ints in enemyRespawnData.
    private int _maxNumRespawns;
    // Array containing the max alive counts for each enemy type in this level.
    private List<int> maxAliveCounts = new List<int>();

    private GameObject _player;

    private List<StagesData.StageEnemyData> _stageEnemyDatas = new();

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
        }
    }

    private void OnEnable()
    {
        SAi.OnAIDeath += DeregisterSAi;
    }

    private void OnDisable()
    {
        SAi.OnAIDeath -= DeregisterSAi;
    }

    private void Start()
    {
        InitializeLevel();
        // StartCoroutine(DifficultyScaling());
    }

    // Do this at the start of every level.
    public void InitializeLevel()
    {
        // Access the stage enemy datas for this stage
        _stageEnemyDatas = GameManager.Instance.CurrentStage.StageEnemyDatas.ToList();

        // Temporarily disable enemy respawning
        _allowEnemyRespawning = false;

        // Grab the player game object
        _player = GameManager.Instance.aPlayer.gameObject;

        _maxNumRespawns = GetMaxRespawns();
        
        // Grab all of the respawn zones in this level
        enemyRespawnZones = GameObject.FindGameObjectsWithTag("EnemyRespawn");
        
        DoInitialSpawns(_stageEnemyDatas);
        
        // Re-enable enemy respawning now that everything is set up
        _allowEnemyRespawning = true;
        
    }

    private int GetMaxRespawns()
    {
        var normalDifficulty = GetNormalizedDifficultyValue();
        return (int)Mathf.Floor(Mathf.Lerp(minNumberOfRespawns, maxNumberOfRespawns, normalDifficulty));
    }

    private float GetNormalizedDifficultyValue()
    {
        return Mathf.Clamp(
            GameManager.Instance.SessionData.sessionTotalTime /
            GameManager.Instance.SessionData.secondsToMaxDifficulty, 0f, 1f);
    }
    
     SpawnParameters GetSpawnParameters(StagesData.StageEnemyData stageEnemyData)
    {
        
        var normalizedDifficultyValue = GetNormalizedDifficultyValue();
            
        Debug.Log($"Normalized difficulty value is {normalizedDifficultyValue}");

        var spawnParameters = new SpawnParameters();
        spawnParameters.EnemyObject = stageEnemyData.enemyType;
        spawnParameters.HealthBuff = Mathf.Lerp(1f, stageEnemyData.maxBuffScale, normalizedDifficultyValue);

        var currentTotalDesiredEnemies = Mathf.Lerp(minNumberOfEnemiesDesiredOnMap, maxNumberOfEnemiesAllowedOnMap, normalizedDifficultyValue);

        DifficultyLevel currentDifficultyLevel = (DifficultyLevel) Mathf.Floor(GameManager.Instance.SessionData.sessionTotalTime /
                                                 (GameManager.Instance.SessionData.secondsToMaxDifficulty / 5f));

        float desiredPercentage = 0f;
        foreach (var populationAtDifficulty in stageEnemyData.PopulationAtDifficulties)
        {
            if (populationAtDifficulty.DifficultyLevel == currentDifficultyLevel)
            {
                desiredPercentage = populationAtDifficulty.percentageOfPopulation;
                break;
            }
        }
        
        var desiredTotalOfType = (int)Mathf.Floor(currentTotalDesiredEnemies * desiredPercentage);

        spawnParameters.DesiredSpawnNumber =
            desiredTotalOfType - typesPopulation[stageEnemyData.enemyType.GetComponent<SAi>()];

        return spawnParameters;
        
    }
    
    void DoInitialSpawns(List<StagesData.StageEnemyData> stageEnemyDatas)
    {
        foreach (var stageEnemyData in stageEnemyDatas)
        {
            var spawnParameters = GetSpawnParameters(stageEnemyData);
            for (var i = 0; i < spawnParameters.DesiredSpawnNumber; i++)
            {
                SpawnEnemy(stageEnemyData.enemyType, spawnParameters, false);
            }
        }
    }

    private void Update()
    {
        
        // If enemy respawning is enabled and more enemies can be respawned
        if (_allowEnemyRespawning && _numRespawns < _maxNumRespawns)
        {
            TryRespawn();
        }

        // If no more enemies can be respawned
        if (_numRespawns >= _maxNumRespawns)
        {
            // If all enemies are dead, move to the next level
            if (activeEnemies.Count == 0)
            {
                Debug.Log("all enemies dead");
                GameManager.Instance.InitLoadNewStage();
            }
        }
        
    }

    private void TryRespawn()
    {
        foreach (var stageEnemyData in _stageEnemyDatas)
        {
            var spawnParams = GetSpawnParameters(stageEnemyData);
            if (spawnParams.DesiredSpawnNumber <= 0) continue;
            for (var i = 0; i < spawnParams.DesiredSpawnNumber; i++)
            {
                SpawnEnemy(stageEnemyData.enemyType, spawnParams, true);
            }
        }
    }

    // Function that spawns an instance of an enemy type meant to appear upon level startup.
    private void SpawnEnemy(GameObject enemy, SpawnParameters parameters, bool isRespawn)
    {
        
        Vector3 respawnPoint = SelectRespawnPoint(SelectSpawnZone(!isRespawn));

        GameObject thisEnemy = Instantiate(enemy, respawnPoint, Quaternion.identity);
        
        activeEnemies.Add(thisEnemy);

        var sAi = thisEnemy.GetComponent<SAi>();
        sAi.BuffHealth(parameters.HealthBuff);
        RegisterSAi(sAi);

        if (isRespawn)
        {
            _numRespawns++;
            StartCoroutine(DisableRespawns(respawnDelay));
        }
    }

    IEnumerator DisableRespawns(float seconds)
    {
        _allowEnemyRespawning = false;
        yield return new WaitForSeconds(seconds);
        _allowEnemyRespawning = true;
    }

    private BoxCollider SelectSpawnZone(bool random)
    {

        if (!random)
        {
            // Choose a respawn zone that is far away from the player and not currently in the camera view
            foreach (GameObject respawnZone in enemyRespawnZones)
            {
                BoxCollider volume = respawnZone.GetComponent<BoxCollider>();
                if (Vector3.Distance(volume.bounds.center, _player.transform.position) >= minRespawnDistance)
                    if (!respawnZone.GetComponent<Renderer>().isVisible)
                        return respawnZone.GetComponent<BoxCollider>();
            }
        }

        var rand = Random.Range(0, enemyRespawnZones.Length);
        return enemyRespawnZones[rand].GetComponent<BoxCollider>();
        
    }

    void RegisterSAi(SAi sAi)
    {
        activeEnemies.Add(sAi.gameObject);
        typesPopulation[sAi]++;
    }

    void DeregisterSAi(SAi sAi)
    {
        activeEnemies.Remove(sAi.gameObject);
        typesPopulation[sAi]--;
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
