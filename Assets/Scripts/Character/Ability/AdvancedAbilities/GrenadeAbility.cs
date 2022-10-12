using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAbility : AdvancedAbility
{
    private GrenadeSpawn grenadeSpawner;

    public GrenadeAbility(SCharacter character) : base(character, 0f, 0.2f)
    {

        grenadeSpawner = character.transform.Find("GrenadeSpawner").GetComponent<GrenadeSpawn>();
    }

    public override void OnCastStarted() {
        base.OnCastStarted();
        Vector3 direction = character.transform.forward;
        Debug.Log(direction);
        grenadeSpawner.CreateGrenade(direction);
        
    }

}