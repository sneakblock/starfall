using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{

    public enum FiringMode
    {
        Auto,
        SemiAuto,
        BoltAction,
        Beam
    };

    [Header("Firing")]
    
    [Tooltip("The damage per bullet or tick of the weapon.")]
    public float damage;
    
    
    [Tooltip(
        "The firing mode of the weapon. Each mode has different behavior, and a different relationship to the firingRate " +
        "parameter.")]
    public FiringMode firingMode;

    [Header("Projectiles")] [Tooltip("The prefab object that the weapon will call fire on. This prefab must have a Bullet component, or else the weapon will do nothing with it.")]
    public GameObject bullet;

    [Tooltip("The force applied to the bullet when it is fired. This may or may not do something-- it's up to the bullet how it responds to force.")]
    public float firingForce;

    [Tooltip("The rate at which the weapon fires. This is measured in maximum rounds per second(!!!). (Not minute)")]
    public float firingRate;

    [Header("Ammo & Reloading")]
    
    [Tooltip("The number of bullets in each magazine. Set to -1 for infinite ammo per magazine.")]
    public int magazineSize;

    [Tooltip("Number of bullets fired per pull of the trigger.")]
    public int bulletsFiredPerShot;

    [Tooltip("The number of seconds it takes to reload the weapon.")]
    public float reloadTime;
    
    [Header("Spread and bloom")]
    
    [Tooltip(
        "The initial accuracy of the weapon when fired when un-aimed.")]
    [Range(0f, .3f)]
    public float minHipFireSpread;
    
    [Tooltip("The maximum spread when fired from the hip. The spread cannot increase beyond this point.")]
    [Range(0f, .3f)]
    public float maxHipFireSpread;

    [Tooltip("The initial accuracy of the weapon when fired during aiming.")]
    [Range(0f, .3f)]
    public float minAdsSpread;

    [Tooltip("The maximum spread when fired when aiming. The spread cannot increase beyond this point.")]
    [Range(0f, .3f)]
    public float maxAdsSpread;

    [Tooltip("How much spread is applied when fired from the hip.")]
    [Range(0f, .3f)]
    public float hipFireBloomIntensity;

    [Tooltip("How much spread is applied when fired when aiming.")]
    [Range(0f, .3f)]
    public float adsBloomIntensity;

    [Tooltip("Max aiming bloom adjustment sharpness, or, how quickly and sharply the bloom lerps between aiming and " +
             "hip firing modes.")]
    [Range(0f, 50f)]
    public float maxRecoverySharpness;
    
    [Tooltip("Min aiming bloom adjustment sharpness, or, how quickly and sharply the bloom lerps between aiming and " +
             "hip firing modes.")]
    [Range(0f, 50f)]
    public float minRecoverySharpness;

    [Tooltip("How much does each bullet reduce aiming recovery")]
    [Range(0f, 50f)]
    public float recoveryImpact;

    [Tooltip("This number represents how quickly recoverySharpness is restored")]
    [Range(0f, 50f)]
    public float recoveryRate;
}
