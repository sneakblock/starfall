using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAbility : AdvancedAbility
{
    [SerializeField] private float throwPower = 5f;
    [SerializeField] private GameObject grenade;
    [SerializeField] private Transform grenadeSpawnPoint;

    public override void OnCastStarted() {
        base.OnCastStarted();
        Vector3 direction = character.transform.forward + new Vector3(0, 1, 0);
        var createdGrenade = Instantiate(grenade, grenadeSpawnPoint.position, Quaternion.identity);
        createdGrenade.transform.LookAt(direction);
        Physics.IgnoreCollision(character.Collider, createdGrenade.GetComponent<Collider>());
        createdGrenade.GetComponent<Rigidbody>().AddForce(direction * throwPower, ForceMode.Impulse);
    }
}