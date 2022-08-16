using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; private set; }

    [Header("Player Data")] public PlayerData playerData;
    
    private Player _player;
    
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
        
        //Set up Player
        _player = gameObject.AddComponent<Player>();
        var c = playerData.playerCharacter.GetComponent<StarfallCharacterController>();
        var cam = playerData.camera.GetComponent<ExampleCharacterCamera>();
        var orbitPoint = c.orbitPoint;
        if (c != null && cam != null)
        {
            _player.character = c;
            _player.orbitCamera = cam;
            _player.cameraFollowPoint = orbitPoint;
        }
        else
        {
            Debug.LogWarning("Something went wrong in GameManager's Player setup! One or more essential player components are not properly configured.");
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
        
        

    }

    private void Start()
    {
        
    }
    
}
