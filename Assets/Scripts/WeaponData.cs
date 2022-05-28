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

    public enum HitMode
    {
        HitScan,
        Projectile
    };
    
    [Header("Firing")]
    
    [Tooltip("The damage per bullet or tick of the weapon.")]
    public float damage;
    
    
    [Tooltip(
        "The firing mode of the weapon. Each mode has different behavior, and a different relationship to the firingRate " +
        "parameter.")]
    public FiringMode firingMode;

    
    [Tooltip("The hit detection method for the weapon. HitScan uses instantly calculated rays, whereas projectile " +
             "spawns entities in the world that travel along a path to strike targets and have travel time.")]
    public HitMode hitMode;

    
    [Tooltip("The speed of each projectile fired from the weapon. This is a 'true' speed parameter t, meaning that " +
             "it determines the increment by which the projectile moves each frame along it's path (accounting for deltaTime)." +
             "This does nothing for HitScan weapons.")]
    public float projectileSpeed;
    

    [Tooltip("The rate at which the weapon fires. Functions differently for each firing mode.")]
    public float firingRate;

    [Header("Ammo & Reloading")]
    
    [Tooltip("The number of bullets in each magazine. Set to -1 for infinite ammo per magazine.")]
    public int magazineSize;

    [Tooltip("The number of seconds it takes to reload the weapon.")]
    public float reloadTime;
    
    [Header("Spread and bloom")]
    
    [Tooltip(
        "The initial accuracy of the weapon when fired when un-aimed. This number multiplies the radius of the unit circle," +
        "meaning that a spread of 1 will fire only at the middle pixel of the screen, and a spread of 25 will place the bullet" +
        " somewhere in the circle centered at the center pixel with a radius of 25.")]
    //TODO: Evaluate if this spread will be independent of screen resolution.
    public float minHipFireSpread;
    
   
    [Tooltip("The maximum spread when fired from the hip. The spread cannot increase beyond this point.")]
    public float maxHipFireSpread;

    [Tooltip("The initial accuracy of the weapon when fired during aiming.")]
    public float minAdsSpread;

   
    [Tooltip("The maximum spread when fired when aiming. The spread cannot increase beyond this point.")]
    public float maxAdsSpread;

    [Tooltip("The intensity of the bloom when fired from the hip.")]
    public float hipFireBloomIntensity;

    [Tooltip("The intensity of the bloom when fired when aiming.")]
    public float adsBloomIntensity;

   
    [Tooltip("Aiming bloom adjustment sharpness, or, how quickly and sharply the bloom lerps between aiming and " +
             "hip firing modes.")]
    public float aimingBloomSharpness;
}
