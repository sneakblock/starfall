using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public static Leaderboards leaderboards { get; private set; }

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

    private BloodPool _bloodPoolComponent;
    public ObjectPool<GameObject> BloodPool;

    private MuzzleFlashPool _muzzleFlashPoolComponent;
    public ObjectPool<GameObject> MuzzleFlashPool;

    private BulletTrailPool _bulletTrailPoolComponent;
    public ObjectPool<GameObject> BulletTrailPool;

    private SurfaceImpactPool[] _surfaceImpactPoolComponents;
    public readonly Dictionary<ImpactEffectSurface.ImpactSurfaceType, ObjectPool<GameObject>> SurfaceImpactPools = new();

    private LinkPool _linkPoolComponent;
    public ObjectPool<GameObject> LinkPool;


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
        
        Leaderboards leaderboards = gameObject.AddComponent<Leaderboards>();

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

        _surfaceImpactPoolComponents = GetComponents<SurfaceImpactPool>();
        foreach (var component in _surfaceImpactPoolComponents)
        {
            SurfaceImpactPools.Add(component.surfaceType, component.Pool);
        }

        _linkPoolComponent = GetComponent<LinkPool>();
        LinkPool = _linkPoolComponent.Pool;
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
        Invoke(nameof(LoadCharacterSelectScene), 3.0f);
    }
    
    private void OnEnemyDeath(GameObject enemy)
    {
        Debug.Log("bitchass mofo dead");
        Destroy(enemy);
    }

    private void LoadCharacterSelectScene()
    {
        //Replace the Testing scene with the name of the character select scene
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
