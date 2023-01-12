
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

public enum EnemyType
{
    Grunt,
    Flyer,
    Priestess,
    Tank
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

    public List<GameObject> activeEnemies;
    private Dictionary<EnemyType, int> typesPopulation;

    [Header("Enemy Respawn Settings")] [SerializeField]
    private float secondsToWaitToCheck = 10f;
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
    [SerializeField] private bool unlimitedRespawns = true;
    
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

    private List<StagesData.StageEnemyData> _stageEnemyDatas;

    private bool _waitingToCheck = false;

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
            DontDestroyOnLoad(this);
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

    // Do this at the start of every level.
    public void InitializeLevel()
    {
        // Temporarily disable enemy respawning
        _allowEnemyRespawning = false;
        
        
        activeEnemies = new List<GameObject>();
        typesPopulation = new Dictionary<EnemyType, int>();
        
        // Access the stage enemy datas for this stage
        _stageEnemyDatas = GameManager.Instance.CurrentStage.StageEnemyDatas.ToList();

        // Grab the player game object
        _player = GameManager.Instance.aPlayer.gameObject;

        _maxNumRespawns = GetMaxRespawns();
        
        // Grab all of the respawn zones in this level
        enemyRespawnZones = GameObject.FindGameObjectsWithTag("EnemyRespawn");
        
        typesPopulation.Add(EnemyType.Grunt, 0);
        typesPopulation.Add(EnemyType.Flyer, 0);
        typesPopulation.Add(EnemyType.Priestess, 0);
        typesPopulation.Add(EnemyType.Tank, 0);
        
        // DoInitialSpawns(_stageEnemyDatas);
        
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

        var spawnParameters = new SpawnParameters
        {
            EnemyObject = stageEnemyData.enemyObject,
            HealthBuff = Mathf.Lerp(1f, stageEnemyData.maxBuffScale, normalizedDifficultyValue)
        };

        var currentTotalDesiredEnemies = Mathf.Lerp(minNumberOfEnemiesDesiredOnMap, maxNumberOfEnemiesAllowedOnMap, normalizedDifficultyValue);
        Debug.Log($"number of total enemies desired on map is {currentTotalDesiredEnemies}");

        DifficultyLevel currentDifficultyLevel = (DifficultyLevel) Mathf.Floor(GameManager.Instance.SessionData.sessionTotalTime /
                                                 (GameManager.Instance.SessionData.secondsToMaxDifficulty / 5f));
        Debug.Log($"current enum difficulty is {currentDifficultyLevel}");

        float desiredPercentage = 0f;
        foreach (var populationAtDifficulty in stageEnemyData.PopulationAtDifficulties)
        {
            if (populationAtDifficulty.DifficultyLevel == currentDifficultyLevel)
            {
                desiredPercentage = populationAtDifficulty.percentageOfPopulation;
                Debug.Log($"Desired percent of spawns for {stageEnemyData.EnemyType} is {desiredPercentage}");
                break;
            }
        }
        
        var desiredTotalOfType = (int)Mathf.Floor(currentTotalDesiredEnemies * desiredPercentage);
        Debug.Log($"Desired total type of {stageEnemyData.EnemyType} is {desiredTotalOfType}");
        Debug.Log($"num {stageEnemyData.EnemyType} on stage is {typesPopulation[stageEnemyData.EnemyType]}");
        
        Debug.Log($"Desired spawns spawns for {stageEnemyData.EnemyType} is {desiredTotalOfType - typesPopulation[stageEnemyData.EnemyType]}");

        spawnParameters.DesiredSpawnNumber =
            desiredTotalOfType - typesPopulation[stageEnemyData.EnemyType];

        return spawnParameters;
        
    }
    
    void DoInitialSpawns(List<StagesData.StageEnemyData> stageEnemyDatas)
    {
        foreach (var stageEnemyData in stageEnemyDatas)
        {
            var spawnParameters = GetSpawnParameters(stageEnemyData);
            for (var i = 0; i < spawnParameters.DesiredSpawnNumber; i++)
            {
                SpawnEnemy(stageEnemyData.enemyObject, spawnParameters, false);
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentStage.isMenu) return;
        // If enemy respawning is enabled and more enemies can be respawned
        if (_allowEnemyRespawning && !_waitingToCheck)
        {
            if (unlimitedRespawns || (!unlimitedRespawns && _numRespawns < _maxNumRespawns))
            {
                TryRespawn();
                StartCoroutine(WaitToCheck(secondsToWaitToCheck));
            }
        }

        // If no more enemies can be respawned
        if (!unlimitedRespawns && _numRespawns >= _maxNumRespawns)
        {
            // If all enemies are dead, move to the next level
            if (activeEnemies.Count == 0)
            {
                Debug.Log("all enemies dead");
                GameManager.Instance.InitLoadNewStage();
            }
        }
        
    }

    IEnumerator WaitToCheck(float seconds)
    {
        _waitingToCheck = true;
        yield return new WaitForSeconds(seconds);
        _waitingToCheck = false;
    }

    private void TryRespawn()
    {
        foreach (var stageEnemyData in _stageEnemyDatas)
        {
            var spawnParams = GetSpawnParameters(stageEnemyData);
            if (spawnParams.DesiredSpawnNumber <= 0) continue;
            for (var i = 0; i < spawnParams.DesiredSpawnNumber; i++)
            {
                SpawnEnemy(stageEnemyData.enemyObject, spawnParams, true);
            }
        }
    }

    // Function that spawns an instance of an enemy type meant to appear upon level startup.
    private void SpawnEnemy(GameObject enemy, SpawnParameters parameters, bool isRespawn)
    {
        
        var respawnPoint = SelectRespawnPoint(SelectSpawnZone(!isRespawn));

        if (!respawnPoint.Item2) return;

        GameObject thisEnemy = Instantiate(enemy, respawnPoint.Item1, Quaternion.identity);

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

        var rand = 0;
        if (!random)
        {
            
            var numChecked = 0;
            // Choose a respawn zone that is far away from the player and not currently in the camera view
            while (numChecked < 4)
            {
                rand = Random.Range(0, enemyRespawnZones.Length);
                var respawnZone = enemyRespawnZones[rand];
                BoxCollider volume = respawnZone.GetComponent<BoxCollider>();
                if (Vector3.Distance(volume.bounds.center, _player.transform.position) >= minRespawnDistance)
                    if (!respawnZone.GetComponent<Renderer>().isVisible)
                        return respawnZone.GetComponent<BoxCollider>();
                numChecked++;
            }
            
        }
        return enemyRespawnZones[rand].GetComponent<BoxCollider>();
        // var rand = Random.Range(0, enemyRespawnZones.Length);
        // Debug.Log("Selected spawn zone index " + rand);
        // return enemyRespawnZones[rand].GetComponent<BoxCollider>();
        
    }

    void RegisterSAi(SAi sAi)
    {
        activeEnemies.Add(sAi.gameObject);
        
        switch (sAi)
        {
            case Grunta ai:
                if (ai is Tank)
                {
                    typesPopulation[EnemyType.Tank]++;
                    break;
                }
                typesPopulation[EnemyType.Grunt]++;
                break;
        
            case Flyer:
                typesPopulation[EnemyType.Flyer]++;
                break;
            case Priestess:
                typesPopulation[EnemyType.Priestess]++;
                break;
        }
    }

    void DeregisterSAi(SAi sAi)
    {
        Debug.Log($"sAi {sAi.gameObject.name} requests deregister from AI manager list. It's null status is {sAi.gameObject == null}");
        activeEnemies.Remove(sAi.gameObject);
        Debug.Log($"after removal, contains state is {activeEnemies.Contains(sAi.gameObject)}");
        switch (sAi)
        {
            case Grunta:
                if (sAi is Tank)
                {
                    typesPopulation[EnemyType.Tank]--;
                    break;
                }
                typesPopulation[EnemyType.Grunt]--;
                break;

            case Flyer:
                typesPopulation[EnemyType.Flyer]--;
                break;
            case Priestess:
                typesPopulation[EnemyType.Priestess]--;
                break;
        }
    }

    // Helper function used to randomly select a respawn point for an enemy within a spawn zone.
    private (Vector3, bool) SelectRespawnPoint(BoxCollider spawnZone)
    {
        int count = 0;
        Vector3 respawnPoint = Vector3.zero;
        bool validRespawnPoint = false;

        // Try no more than 3 different spawn points to avoid infinite loop crash
        while (!validRespawnPoint && count < 3)
        {
            var bounds = spawnZone.bounds;
            respawnPoint = GetRandomPointInBounds(spawnZone);

            // Check if this respawn point collides with another game object.
            Collider[] hitColliders = Physics.OverlapSphere(respawnPoint, 0.55f);
            if (hitColliders.Length <= 1)
                validRespawnPoint = true;

            count++;
        }

        return (respawnPoint, validRespawnPoint);
    }
    
    public Vector3 GetRandomPointInBounds(BoxCollider boxCollider)
    {
        var bounds = boxCollider.bounds;
        float minX = bounds.size.x * -0.5f;
        float minY = bounds.size.y * -0.5f;
        float minZ = bounds.size.z * -0.5f;

        return (Vector3)boxCollider.gameObject.transform.TransformPoint(
            new Vector3(Random.Range (minX, -minX),
                Random.Range (minY, -minY),
                Random.Range (minZ, -minZ))
        );
    }
}
