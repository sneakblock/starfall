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
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public StagesData StarfallStages;

    public SessionData SessionData;

    public StagesData.Stage CurrentStage;
    private StagesData.Stage _upNextStage;

    //public static Leaderboards leaderboard { get; private set; }

    [Header("Player Character")] public GameObject playerCharacter;
    public APlayer aPlayer;
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

    [FormerlySerializedAs("distortOnSceneLoad")] [FormerlySerializedAs("distortOnStageLoad")] [FormerlySerializedAs("distortOnStart")] [FormerlySerializedAs("distortOnAwake")] [Header("Scene Loading")] [SerializeField]
    private bool distortOnSceneActivate = true;
    [SerializeField] private Material effectMaterial;
    [FormerlySerializedAs("secondsToDistortOnSceneLoad")] [SerializeField] private float secondsToDistortOnNewScene = 5f;
    [SerializeField] private float distortionFromVertexResolution = 300f;
    [SerializeField] private float distortionFromVertexJitter = 0f;
    [SerializeField] private float distortionFromColorBalance = 0f;
    [SerializeField] private float distortionToVertexResolution = 5f;
    [SerializeField] private float distortionToVertexJitter = 3f;
    [SerializeField] private float distortionToColorBalance = 1f;

    [Header("Music")] [SerializeField] private DoubleAudioSource _doubleAudioSource;

    private List<AudioClip> _playedSongs = new();
    public AudioSpectrum audioSpectrum;

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
    public Dictionary<ImpactEffectSurface.ImpactSurfaceType, ObjectPool<GameObject>> SurfaceImpactPools = new();

    private LinkPool _linkPoolComponent;
    public ObjectPool<GameObject> LinkPool;

    private bool _isAwakeDistorting;
    private bool _isLoadingNewScene;
    private float _distortionTimer = 0f;
    private string _sceneNameToLoad;
    private Dictionary<Renderer, List<Material>> _origRenderersMats = new();
    
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int VertexResolution = Shader.PropertyToID("Vector1_B2CC132F");
    private static readonly int VertexDisplacmentAmount = Shader.PropertyToID("_VertexDisplacementAmount");
    private static readonly int DistortionColorBalance = Shader.PropertyToID("_DistortionColorBalance");
    
    [Button("InitLoadNewStage", "Load new stage", false)] public bool foo;
    [Button("WipeSessionData", "Wipe session data", false)] public bool bar;

    //Async loading management
    private AsyncOperation _asyncOperation;

    private PostProcessManager _postProcessManager;
    private AIManager _aiManager;

    private void Awake()
    {
        //Initialize the instance of the Game Manager.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        _playedSongs = new();
        if (!_doubleAudioSource) _doubleAudioSource = GetComponent<DoubleAudioSource>();

        _postProcessManager = GetComponent<PostProcessManager>();
        _aiManager = GetComponent<AIManager>();

        audioSpectrum = GetComponent<AudioSpectrum>();
        
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void InitPools()
    {
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
        SurfaceImpactPools = new();
        foreach (var component in _surfaceImpactPoolComponents)
        {
            SurfaceImpactPools.Add(component.surfaceType, component.Pool);
        }

        _linkPoolComponent = GetComponent<LinkPool>();
        LinkPool = _linkPoolComponent.Pool;
    }

    IEnumerator PreLoadRun()
    {
        yield return new WaitForSeconds(.1f);
        WipeSessionData();
        InitPools();
        _upNextStage = GetNextStage();
        StartCoroutine(LoadSceneAsyncProcess(_upNextStage.StageName));
    }

    public void ReturnToMenu()
    {
        var mainMenuStage = GetMainMenuStage();
        _upNextStage = mainMenuStage;
        StartCoroutine(LoadSceneAsyncProcess(mainMenuStage.StageName));
        QueueNextSceneActivation(mainMenuStage);
    }

    /// <summary>
    /// This is always called from the main menu. Should wipe the score, ensure the runTimer is reset, and then load the first stage of the run.
    /// </summary>
    public void StartRun()
    {
        PlayerDeath += OnPlayerDeath;
        EnemyDeath += OnEnemyDeath;
        Debug.Log("Called StartRun with upnextstage" + _upNextStage.StageName);
        QueueNextSceneActivation(_upNextStage);
    }

    public void NextStage()
    {
        StartCoroutine(LoadSceneAsyncProcess(GetNextStage().StageName));
    }

    public void EndRun()
    {
        var leaderboardStage = GetLeaderboardStage();
        _upNextStage = leaderboardStage;
        StartCoroutine(LoadSceneAsyncProcess(leaderboardStage.StageName));
        QueueNextSceneActivation(leaderboardStage);
    }

    private void OnActiveSceneChanged(Scene curr, Scene next)
    {
        //For all scenes.
        if (distortOnSceneActivate)
        {
            _isLoadingNewScene = false;
            _distortionTimer = 0f;
            SwapAllMaterialsInScene(true);
            _postProcessManager.LerpFromEffect(secondsToDistortOnNewScene, _upNextStage.StageVolumeProfile);
            _isAwakeDistorting = true;
        }

        if (next.name != "MainMenu" || next.name != "Leaderboard") OnStageActivate();
        if (next.name == "MainMenu") StartCoroutine(PreLoadRun());
    }
    
    private StagesData.Stage GetNextStage()
    {
        // For all stages in the game, which have we not visited?
        var unvisitedStages = (from stage in StarfallStages.Stages where !SessionData.traversedStages.Contains(stage) && !stage.isMenu select stage).ToList();

        // If there are stages we haven't seen yet, choose a random one of those to load.
        if (unvisitedStages.Count > 0)
        {
            return unvisitedStages[Random.Range(0, unvisitedStages.Count)];
        }
        
        // If we have visited every stage, we should continue to repeat the same pattern.
        // Get the index at which our current scene rests in traversedStages
        int thisIdx = 0;
        for (var i = 0; i < SessionData.traversedStages.Count; i++)
        {
            if (SessionData.traversedStages[i].StageName != SceneManager.GetActiveScene().name) continue;
            thisIdx = i;
            break;
        }

        // Load a new scene according to circular array logic
        return thisIdx + 1 >= SessionData.traversedStages.Count
            ? SessionData.traversedStages[0]
            : SessionData.traversedStages[thisIdx + 1];
    }

    private StagesData.Stage GetLeaderboardStage()
    {
        foreach (var stage in StarfallStages.Stages)
        {
            if (stage.StageName == "Leaderboard") return stage;
        }

        return new StagesData.Stage();
    }
    
    private StagesData.Stage GetMainMenuStage()
    {
        foreach (var stage in StarfallStages.Stages)
        {
            if (stage.StageName == "MainMenu") return stage;
        }

        return new StagesData.Stage();
    }
    
    private IEnumerator LoadSceneAsyncProcess(string sceneName)
    {
        // Begin to load the Scene you have specified.
        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        // Don't let the Scene activate until you allow it to.
        _asyncOperation.allowSceneActivation = false;
        
        yield return null;
    }

    /// <summary>
    /// The scene has loaded, but has not been activated.
    /// </summary>
    private void QueueNextSceneActivation(StagesData.Stage nextStage)
    {
        _distortionTimer = 0f;
        _isLoadingNewScene = true;
        SwapAllMaterialsInScene(true);
        _postProcessManager.LerpToEffect(secondsToDistortOnNewScene);
        CrossfadeToSongFromStage(nextStage);
        StartCoroutine(WaitThenActivateScene(secondsToDistortOnNewScene));
    }
    
    IEnumerator WaitThenActivateScene(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_asyncOperation != null)
        {
            _asyncOperation.allowSceneActivation = true;
        }
    }
    
    
    /// <summary>
    /// Called upon a new scene being activated, to initialize certain params.
    /// </summary>
    private void OnStageActivate()
    {
        foreach (var stage in StarfallStages.Stages)
        {
            if (stage.StageName == SceneManager.GetActiveScene().name)
            {
                CurrentStage = stage;
                break;
            }
        }
        if (!CurrentStage.isMenu) _aiManager.InitializeLevel();

        if (!dirLight) dirLight = TryFindDirLight();
    }

    public void WipeSessionData()
    {
        SessionData.sessionTotalTime = 0f;
        SessionData.sessionScore = 0;
        SessionData.traversedStages = new List<StagesData.Stage>();
    }

    private void Update()
    {
        // Always add time to the session's total time.
        if (SceneManager.GetActiveScene().name != "MainMenu") SessionData.sessionTotalTime += Time.deltaTime;
        
        //Songs
        
        if (_doubleAudioSource && !_doubleAudioSource.isPlaying) PlayNewSong();
        
        //Distortion

        if (_isAwakeDistorting || _isLoadingNewScene) _distortionTimer += Time.deltaTime;
        
        if (_isAwakeDistorting && _distortionTimer >= secondsToDistortOnNewScene) StopAwakeDistorting();

        if (_isAwakeDistorting)
        {
            DoFullscreenDistortion(false);
        }
        
        if (_isLoadingNewScene)
        {
            DoFullscreenDistortion(true);
        }
        
    }

    // private void SpawnPlayer()
    // {
    //     var spawns = GameObject.FindGameObjectsWithTag("PlayerSpawn");
    //     var playerChar = Instantiate(playerCharacter, spawns[Random.Range(0, spawns.Length)].transform.position, Quaternion.identity);
    //     aPlayer = playerChar.GetComponent<APlayer>();
    // }

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
        Debug.Log("final score " + finalScore);
        EndRun();
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

    /// <summary>
    /// Called to tell GameManager to load a new playable stage.
    /// </summary>
    public void InitLoadNewStage()
    {
        //Once we've started the transition, enemy score will no longer count and the game won't restart if you die.
        PlayerDeath -= OnPlayerDeath;
        EnemyDeath -= OnEnemyDeath;
    }

    private void BeginLoadNewStage(string sceneName)
    {
        _sceneNameToLoad = sceneName;
    }

    private void SwapAllMaterialsInScene(bool useEffectMaterial)
    {
        
        // Every time this is called, we need all of these. 
        List<Renderer> allRenderers = new List<Renderer>();
        foreach (var t in FindObjectsOfType<Transform>())
        {
            foreach (var r in t.GetComponents<Renderer>())
            {
                allRenderers.Add(r);
            }
        }

        // If we are moving TO a distorted state, cache the current state, then assign the effect material.
        if (useEffectMaterial)
        {
            _origRenderersMats = new Dictionary<Renderer, List<Material>>();
            foreach (var r in allRenderers)
            {
                _origRenderersMats.Add(r, r.materials.ToList());
                var newMats = new Material[r.materials.Length];
                for (var i = 0; i < newMats.Length; i++)
                {
                    newMats[i] = effectMaterial;
                }

                r.materials = newMats;
            }
        }
        else // Use cached values to put back normal mats.
        {
            foreach (var kv in _origRenderersMats)
            {
                var newMats = new Material[kv.Key.materials.Length];
                for (var i = 0; i < newMats.Length; i++)
                {
                    newMats[i] = kv.Value[i];
                }

                kv.Key.materials = newMats;
            }
        }

        foreach (var musicReactor in FindObjectsOfType<MusicReactor>())
        {
            musicReactor.AcquireMaterials();
        }
        
    }

    private void DoFullscreenDistortion(bool to)
    {
        foreach (var t in FindObjectsOfType<Transform>())
        {
            foreach (var r in t.GetComponents<Renderer>())
            {
                foreach (var m in r.materials)
                {
                    m.SetFloat(VertexResolution, to ? Mathf.Lerp(distortionFromVertexResolution, distortionToVertexResolution, _distortionTimer / secondsToDistortOnNewScene) : Mathf.Lerp(distortionToVertexResolution, distortionFromVertexResolution, _distortionTimer / secondsToDistortOnNewScene));
                    m.SetFloat(VertexDisplacmentAmount, to ? Mathf.Lerp(distortionFromVertexJitter, distortionToVertexJitter, _distortionTimer / secondsToDistortOnNewScene) : Mathf.Lerp(distortionToVertexJitter, distortionFromVertexJitter, _distortionTimer / secondsToDistortOnNewScene));
                    m.SetFloat(DistortionColorBalance, to ? Mathf.Lerp(distortionFromColorBalance, distortionToColorBalance, _distortionTimer / secondsToDistortOnNewScene) : Mathf.Lerp(distortionToColorBalance, distortionFromColorBalance, _distortionTimer / secondsToDistortOnNewScene));
                }
            }
        }
    }

    void StopAwakeDistorting()
    {
        _isAwakeDistorting = false;
        _distortionTimer = 0f;
        SwapAllMaterialsInScene(false);
    }

    void PlayNewSong()
    {
        Debug.Log(CurrentStage.StageName);
        if (!_doubleAudioSource) return;
        List<AudioClip> unplayedSongs = new();

        foreach (var audioClip in CurrentStage.StageSongs)
        {
            if (!_playedSongs.Contains(audioClip)) unplayedSongs.Add(audioClip);
        }

        var songToPlay = unplayedSongs.Count == 0 ? CurrentStage.StageSongs[Random.Range(0, CurrentStage.StageSongs.Length - 1)] : unplayedSongs[Random.Range(0, unplayedSongs.Count)];
        _doubleAudioSource.CrossFade(songToPlay, .2f, 0f);
        _playedSongs.Add(songToPlay);
    }

    void CrossfadeToSongFromStage(StagesData.Stage toStage)
    {
        List<AudioClip> unplayedSongs = new();

        foreach (var audioClip in toStage.StageSongs)
        {
            if (!_playedSongs.Contains(audioClip)) unplayedSongs.Add(audioClip);
        }

        var songToPlay = unplayedSongs.Count == 0 ? toStage.StageSongs[Random.Range(0, toStage.StageSongs.Length - 1)] : unplayedSongs[Random.Range(0, unplayedSongs.Count)];
        _doubleAudioSource.CrossFade(songToPlay, .35f, 6f);
        _playedSongs.Add(songToPlay);
    }

    
}
