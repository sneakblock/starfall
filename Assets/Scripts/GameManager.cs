using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Character")] public APlayer aPlayer;
    public RangedWeapon playerWeapon { get; private set; }

    private Player _player;
    private GameObject _playerCharacterGameObject;
    private GameObject _playerCameraGameObject;

    // TODO(mish): have SCharacter main player and set up a function to switch
    // between different players
    private SCharacter _currentPlayer;
    //private Kuze _kuze;

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

    }

    
}
