using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public StagesData StarfallStages;

    public SessionData SessionData;

    //public static Leaderboards leaderboard { get; private set; }

    [Header("Player Character")] public APlayer aPlayer;
    public RangedWeapon playerWeapon { get; private set; }
    public Light dirLight;

    // TODO(mish): have SCharacter main player and set up a function to switch
    // between different players
    private SCharacter _currentPlayer;
    //private Kuze _kuze;
    
    // TODO(cameron): use more specific types if desired
    public static Action<APlayer> PlayerDeath;
    public static Action<GameObject> EnemyDeath;

    public static int finalScore;

    [Header("Scene Loading")] 
    [SerializeField] private Material effectMaterial;
    [SerializeField] private float secondsToDistortOnSceneLoad = 5f;
    [SerializeField] private float distortionFromVertexResolution = 300f;
    [SerializeField] private float distortionFromVertexJitter = 0f;
    [SerializeField] private float distortionFromColorBalance = 0f;
    [SerializeField] private float distortionToVertexResolution = 5f;
    [SerializeField] private float distortionToVertexJitter = 3f;
    [SerializeField] private float distortionToColorBalance = 1f;
    
    [SerializeField] private bool rainbow = false;

    [Header("Pooling")]
    private BloodPool _bloodPoolComponent;
    public ObjectPool<GameObject> BloodPool;

    private MuzzleFlashPool _muzzleFlashPoolComponent;
    public ObjectPool<GameObject> MuzzleFlashPool;

    private BulletTrailPool _bulletTrailPoolComponent;
    public ObjectPool<GameObject> BulletTrailPool;

    private CloneBulletTrailPool _cloneBulletTrailPoolComponent;
    public ObjectPool<GameObject> CloneBulletTrailPool;

    private RedLazerPool _redLazerPoolComponent;
    public ObjectPool<GameObject> RedLazerPool;

    private SurfaceImpactPool[] _surfaceImpactPoolComponents;
    public readonly Dictionary<ImpactEffectSurface.ImpactSurfaceType, ObjectPool<GameObject>> SurfaceImpactPools = new();

    private LinkPool _linkPoolComponent;
    public ObjectPool<GameObject> LinkPool;

    private bool _isLoadingScene = false;
    private float _distortionTimer = 0f;
    private string _sceneNameToLoad;
    
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int VertexResolution = Shader.PropertyToID("Vector1_B2CC132F");
    private static readonly int VertexDisplacmentAmount = Shader.PropertyToID("_VertexDisplacementAmount");
    private static readonly int DistortionColorBalance = Shader.PropertyToID("_DistortionColorBalance");
    
    [Button("InitLoadNewStage", "Load new stage", false)] public bool foo;


    private void Awake()
    {
        //Initialize the instance of the Game Manager.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        
        //Leaderboards leaderboards = gameObject.AddComponent<Leaderboards>();

        if (!aPlayer) TryFindAPlayer();
        if (!dirLight) dirLight = TryFindDirLight();
        
        PlayerDeath += OnPlayerDeath;
        EnemyDeath += OnEnemyDeath;

        _bloodPoolComponent = GetComponent<BloodPool>();
        BloodPool = _bloodPoolComponent.Pool;

        _muzzleFlashPoolComponent = GetComponent<MuzzleFlashPool>();
        MuzzleFlashPool = _muzzleFlashPoolComponent.Pool;

        _bulletTrailPoolComponent = GetComponent<BulletTrailPool>();
        BulletTrailPool = _bulletTrailPoolComponent.Pool;

        _cloneBulletTrailPoolComponent = GetComponent<CloneBulletTrailPool>();
        CloneBulletTrailPool = _cloneBulletTrailPoolComponent.Pool;

        _redLazerPoolComponent = GetComponent<RedLazerPool>();
        RedLazerPool = _redLazerPoolComponent.Pool;

        _surfaceImpactPoolComponents = GetComponents<SurfaceImpactPool>();
        foreach (var component in _surfaceImpactPoolComponents)
        {
            SurfaceImpactPools.Add(component.surfaceType, component.Pool);
        }

        _linkPoolComponent = GetComponent<LinkPool>();
        LinkPool = _linkPoolComponent.Pool;
        
        //If we are in the main menu, wipe the sessionData.
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            SessionData.sessionTotalTime = 0f;
            SessionData.sessionDifficulty = 1f;
            SessionData.sessionScore = 0;
            SessionData.traversedStageNames = new List<string>();
        }

        // Set the scene as traversed, if it hasn't been marked as such already.
        if (SessionData.traversedStageNames.Count == 0)
        {
            SessionData.traversedStageNames.Add(SceneManager.GetActiveScene().name);
        }
        else
        {
            foreach (var traversedStageName in SessionData.traversedStageNames.Where(traversedStageName => SceneManager.GetActiveScene().name != traversedStageName && traversedStageName != "MainMenu"))
            {
                SessionData.traversedStageNames.Add(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void Update()
    {
        // Always add time to the session's total time.
        SessionData.sessionTotalTime += Time.deltaTime;

        if (!_isLoadingScene) return;
        _distortionTimer += Time.deltaTime;
        DoFullscreenDistortion();
    }

    void TryFindAPlayer()
    {
        aPlayer = GameObject.FindWithTag("Player").GetComponent<APlayer>();
    }

    Light TryFindDirLight()
    {
        foreach (var light in FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional) return light;
        }

        return null;
    }
    
    private void OnPlayerDeath(APlayer player)
    {
        Debug.Log("SNAKE? SNAAAAAAAAAAAAAAAAKE!!!!");
        // Do whatever cleanup
        PlayerDeath -= OnPlayerDeath;
        EnemyDeath -= OnEnemyDeath;
        finalScore = (int)Score.getSavedScore();
        Invoke(nameof(LoadLeaderboardScene), 3.0f);
    }
    
    private void OnEnemyDeath(GameObject enemy)
    {
        Debug.Log("bitchass mofo dead");
        Destroy(enemy);
    }

    private void LoadLeaderboardScene()
    {
        SceneManager.LoadScene("Leaderboard", LoadSceneMode.Single);
    }

    public void InitLoadNewStage()
    {
        // For all stages in the game, which have we not visited?
        var unvisitedStages = (from stage in StarfallStages.Stages where !SessionData.traversedStageNames.Contains(stage.StageName) select stage.StageName).ToList();

        // If there are stages we haven't seen yet, choose a random one of those to load.
        if (unvisitedStages.Count > 0)
        {
            BeginLoadNewStage(unvisitedStages[Random.Range(0, unvisitedStages.Count)]);
            return;
        }
        
        // If we have visited every stage, we should continue to repeat the same pattern.
        // Get the index at which our current scene rests in traversedStages
        int thisIdx = 0;
        for (var i = 0; i < SessionData.traversedStageNames.Count; i++)
        {
            if (SessionData.traversedStageNames[i] == SceneManager.GetActiveScene().name)
            {
                thisIdx = i;
                break;
            }
        }

        // Load a new scene according to circular array logic
        BeginLoadNewStage(thisIdx + 1 >= SessionData.traversedStageNames.Count
            ? SessionData.traversedStageNames[0]
            : SessionData.traversedStageNames[thisIdx + 1]);
    }

    private void BeginLoadNewStage(string sceneName)
    {
        _sceneNameToLoad = sceneName;
        _distortionTimer = 0f;
        _isLoadingScene = true;
        SwapAllMaterialsInScene();
        StartCoroutine(WaitThenLoadScene(secondsToDistortOnSceneLoad));
    }

    private void SwapAllMaterialsInScene()
    {
        foreach (var t in FindObjectsOfType<Transform>())
        {
            foreach (var r in t.GetComponents<Renderer>())
            {
                var newMats = new Material[r.materials.Length];
                for (var i = 0; i < newMats.Length; i++)
                {
                    newMats[i] = effectMaterial;
                    if (!rainbow) continue;
                    var randColor = new Color(Random.Range(0f, 255f), Random.Range(0f, 255f),
                        Random.Range(0f, 255f));
                    newMats[i].SetColor(Color1, randColor);
                }

                r.materials = newMats;
            }
        }
    }

    private void DoFullscreenDistortion()
    {
        foreach (var t in FindObjectsOfType<Transform>())
        {
            foreach (var r in t.GetComponents<Renderer>())
            {
                foreach (var m in r.materials)
                {
                    m.SetFloat(VertexResolution, Mathf.Lerp(distortionFromVertexResolution, distortionToVertexResolution, _distortionTimer / secondsToDistortOnSceneLoad));
                    m.SetFloat(VertexDisplacmentAmount, Mathf.Lerp(distortionFromVertexJitter, distortionToVertexJitter, _distortionTimer / secondsToDistortOnSceneLoad));
                    m.SetFloat(DistortionColorBalance, Mathf.Lerp(distortionFromColorBalance, distortionToColorBalance, _distortionTimer / secondsToDistortOnSceneLoad));
                }
            }
        }
    }

    IEnumerator WaitThenLoadScene(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(_sceneNameToLoad);
    }

    
}
