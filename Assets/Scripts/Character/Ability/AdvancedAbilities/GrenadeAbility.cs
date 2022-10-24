using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAbility : AdvancedAbility
{
    private GrenadeSpawn grenadeSpawner;

    public GrenadeAbility(SCharacter character) : base(character, 5f, 0.2f)
    {
        if (!character.transform.Find("GrenadeSpawner")) return;
        grenadeSpawner = character.transform.Find("GrenadeSpawner").GetComponent<GrenadeSpawn>();
    }

    public override void OnCastStarted() {
        base.OnCastStarted();
        Vector3 direction = character.transform.forward + new Vector3(0, 1, 0);
        grenadeSpawner.CreateGrenade(direction);
        
    }

}