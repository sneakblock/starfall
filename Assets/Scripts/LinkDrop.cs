using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class LinkDrop : MonoBehaviour
{

    [Range(0, 100)]
    public float value;

    private Rigidbody _rb;
    private SphereCollider _coll;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (!GameManager.Instance) return;
        if (!(Vector3.Distance(transform.position, GameManager.Instance.aPlayer.gameObject.transform.position) <=
              3)) return;
        GameManager.Instance.aPlayer.Heal(value);
        // StartCoroutine(ReduceBleed(2f, 1));
        Destroy(gameObject);
    }

    // IEnumerator ReduceBleed(float seconds, int amount)
    // {
    //     GameManager.Instance.aPlayer.linkDamagePerSec -= amount;
    //     yield return new WaitForSeconds(seconds);
    //     GameManager.Instance.aPlayer.linkDamagePerSec += amount;
    // }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     Debug.Log("Collide!");
    //     Debug.Log(collision.gameObject.GetComponent<APlayer>());
    //     if (collision.gameObject.GetComponent<APlayer>() != GameManager.Instance.aPlayer) return;
    //     Debug.Log("Collide with player");
    //     collision.gameObject.GetComponent<APlayer>().Heal(value);
    //     Destroy(gameObject);
    // }
}
