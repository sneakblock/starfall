using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Ability Variables")]

    [Header("Dash Ability Variables")] 
    [SerializeField]
    public float dashAbilitySpeed;
    [SerializeField]
    public float dashAbilityTime;
    [SerializeField]
    public float dashAbilityCooldownTime;
    [SerializeField]
    [Tooltip("How many seconds the cooldown will be reduced per enemy kill.")]
    public float dashAbilityCooldownReductionPerKill;
    [SerializeField]
    public int dashAbilityDamage;
}