using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Core Assignments")]
    public GameObject camera;
    public GameObject playerCharacter;
    
    [Header("Runtime Data")]
    public float weaponSpread;
    public int currentAmmo;
    public int totalAmmo;
}
