using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; private set; }

    [Header("Player Data")] public PlayerData playerData;

    [Header("Spawn Point")] public Transform spawnPoint;
    
    private Player _player;
    private GameObject _playerCharacterGameObject;
    private GameObject _playerCameraGameObject;

    //UI
    private Crosshair _crosshair;
    private AmmoCounter _ammoCounter;
    private ReloadBar _reloadBar;

    public Player GetPlayer()
    {
        return _player;
    }

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

        //Set up UI references
        var crosshair = GameObject.FindObjectOfType<Crosshair>();
        var ammoCounter = GameObject.FindObjectOfType<AmmoCounter>();
        var reloadBar = GameObject.FindObjectOfType<ReloadBar>();
        if (crosshair != null && ammoCounter != null && reloadBar != null)
        {
            _crosshair = crosshair;
            _ammoCounter = ammoCounter;
            _reloadBar = reloadBar;
        }
        else
        {
            Debug.LogWarning("Something went wrong in GameManager's UI setup! One of more UI components were not found.");
        }

        var position = spawnPoint.position;
        _playerCharacterGameObject = Instantiate(playerData.playerCharacter, position, Quaternion.identity);
        _playerCameraGameObject = Instantiate(playerData.camera,
            position - (spawnPoint.forward *
                        playerData.camera.GetComponent<ExampleCharacterCamera>().DefaultDistance),
            Quaternion.identity);
        
        //Set up Player
        _player = gameObject.AddComponent<Player>();
        var c = _playerCharacterGameObject.GetComponent<SCharacterController>();
        var cam = _playerCameraGameObject.GetComponent<ExampleCharacterCamera>();
        var orbitPoint = c.orbitPoint;
        if (c != null && cam != null)
        {
            _player.SetCharacter(c);
            _player.orbitCamera = cam;
            _player.cameraFollowPoint = orbitPoint;
        }
        else
        {
            Debug.LogWarning("Something went wrong in GameManager's Player setup! One or more essential player components are not properly configured.");
        }
        _player.playerFiringLayerMask = playerData.playerFiringLayerMask;
        
        //Set up event listeners
        _player.onPlayerReloadStart.AddListener(_reloadBar.AnimateReloadBar);
        _player.onPlayerReloadComplete.AddListener(_ammoCounter.UpdateAmmoCounter);
        _player.onPlayerFire.AddListener(_ammoCounter.UpdateAmmoCounter);
        
        
    }

    private void Start()
    {
        
    }
    
}
