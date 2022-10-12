using System;
using UnityEngine;

public class Grapple : Ability
{
    public Grapple(SCharacter character) : base(character)
    {
        // Create instance variables if needed
    }

    public override void Start()
    {
        // By default, abilities are disabled when it is registered
        // for this specific example, this ability is enabled when registered
        // for testing purposes
        base.Enable();
    }

    public override void OnEnable()
    {
        // This gets called once when enabled
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log(player.transform.localRotation.eulerAngles);
    }
}