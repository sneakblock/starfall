using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using UnityEngine.VFX;

public abstract class RangedWeapon : MonoBehaviour
{

    [SerializeField] 
    [Tooltip("Add a weaponData scriptable object here to give the weapon the information it needs to behave.")]
    protected WeaponData weaponData;

    [SerializeField] [Tooltip("The transform position from which the weapon will be fired.")]
    public Transform[] barrelTransforms;

    public AudioSource AudioSource;

    [SerializeField]
    private ImpactEffect[] impactEffects;

    //The owner of the weapon
    protected SCharacter OwnerChar;
    private bool _isOwnedByPlayer = false;
    
    //Spread data
    private float _currentSpread;
    private float _currentRecoverySharpness;
    
    //Firing conditions
    private bool _isAiming;
    private float _timeLastFired = 0f;
    private bool _bursting = false;
    private bool _reloading;
    
    //The direction in which to fire
    private Vector3 _firedDir;
    
    //The available bullets
    private int _bulletsCurrentlyInMagazine;
    
    //Events
    //Invoked when the player fires. The first int represents the bullets currently in the magazine, the second the total bullets in the magazine.
    public static event Action<int, int> OnUpdatePlayerAmmo;
    
    //Invoked on player reload. The float is reload time in seconds.
    public static event Action<float> OnPlayerReload;

    //Invoked each time the spread is calculated (every frame). The float is the current spread.
    public static event Action<float> OnCalculatePlayerSpread;

    protected virtual void Start()
    {
        _currentSpread = weaponData.minHipFireSpread;
        _bulletsCurrentlyInMagazine = weaponData.magazineSize;
        OwnerChar = GetComponentInParent<SCharacter>();
        if (GameManager.Instance) {
            if (GameManager.Instance.aPlayer == OwnerChar)
            {
                _isOwnedByPlayer = true;
                OnUpdatePlayerAmmo?.Invoke(_bulletsCurrentlyInMagazine, weaponData.magazineSize);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        ManageSpread();
    }
    
    /// <summary>
    /// This method checks the validity of the requested fire. If the weapon cannot fire, e.g, its fire rate is too low,
    /// it's out of ammunition, or it's a semi-auto weapon and the trigger has not been released, it will return and do nothing.
    /// However, if the request is valid, it will call CalculateTrajectory(), which will continue the firing process.
    /// </summary>
    /// <param name="targetPoint">
    /// The goal point in space that the agents *wants* the bullet to hit.
    /// </param>
    /// <param name="wasRequestingFireLastFrame">
    /// Was the agent requesting fire last frame? Some weapons require a release of the trigger before they will fire again.
    /// </param>
    public void RequestFire(Vector3 targetPoint, bool wasRequestingFireLastFrame)
    {
        switch (weaponData.firingMode)
        {
            case WeaponData.FiringMode.Auto:
                if (Time.time - _timeLastFired > (1 / weaponData.firingRate) && !_reloading)
                {
                    for (var i = 0; i < weaponData.bulletsFiredPerShot; i++)
                    {
                        if (_bulletsCurrentlyInMagazine > 0)
                        {
                            CalculateTrajectory(targetPoint);
                        }
                        else
                        {
                            if (!_reloading)
                            {
                                Reload();
                            }
                            break;
                        }
                    }
                    //Adding some kick
                    if (_isAiming)
                    {
                        _currentSpread += weaponData.adsBloomIntensity;
                    }
                    else
                    {
                        _currentSpread += weaponData.hipFireBloomIntensity;
                    }
                }
                break;
            case WeaponData.FiringMode.SemiAuto:
                if (!wasRequestingFireLastFrame && Time.time - _timeLastFired > (1 / weaponData.firingRate) && !_reloading)
                {
                    for (var i = 0; i < weaponData.bulletsFiredPerShot; i++)
                    {
                        if (_bulletsCurrentlyInMagazine > 0)
                        {
                            CalculateTrajectory(targetPoint);
                        }
                        else
                        {
                            if (!_reloading)
                            {
                                Reload();
                            }
                            break;
                        }
                    }
                    //Adding some kick
                    if (_isAiming)
                    {
                        _currentSpread += weaponData.adsBloomIntensity;
                    }
                    else
                    {
                        _currentSpread += weaponData.hipFireBloomIntensity;
                    }
                }
                break;
            case WeaponData.FiringMode.Burst:
                if (!wasRequestingFireLastFrame && Time.time - _timeLastFired > (1 / weaponData.firingRate) &&
                    !_reloading && !_bursting)
                {
                    _bursting = true;
                    StartCoroutine(FireBurst(weaponData.bulletsFiredPerShot, weaponData.burstDelay, targetPoint));
                    //Adding some kick
                    if (_isAiming)
                    {
                        _currentSpread += weaponData.adsBloomIntensity;
                    }
                    else
                    {
                        _currentSpread += weaponData.hipFireBloomIntensity;
                    }
                }

                break;
        }
    }

    /// <summary>
    /// For burst weapons, a parallel coroutine must handle their burst fire.
    /// </summary>
    /// <param name="bulletsPerBurst">
    /// How many total bullets to fire in this burst.
    /// </param>
    /// <param name="burstDelay">
    /// The number of seconds between each shot fired in the burst.
    /// </param>
    /// <param name="targetPoint">
    /// The point that the agent *wants* to hit.
    /// </param>
    /// <returns></returns>
    IEnumerator FireBurst(int bulletsPerBurst, float burstDelay, Vector3 targetPoint)
    {
        for (var i = 0; i < weaponData.bulletsFiredPerShot; i++)
        {
            if (_bulletsCurrentlyInMagazine > 0)
            {
                CalculateTrajectory(targetPoint);
                yield return new WaitForSeconds(burstDelay);
            }
            else
            {
                if (!_reloading)
                {
                    Reload();
                }
                break;
            }
        }
        _bursting = false;
    }

    /// <summary>
    /// This method fires the weapon from the barrelTransform to the targetPoint. This method should account for bullet
    /// spread, e.g the targetPoint is not sure to be hit if the spread is greater than zero. Because it represents a successful
    /// firing of the bullet, it also sets _timeSinceLastBulletFired to 0. 
    /// </summary>
    /// <param name="targetPoint">
    /// The Vector3 at which the projectile should fire, AKA the location the agent is aiming at. 
    /// </param>
    private void CalculateTrajectory(Vector3 targetPoint)
    {
        //The perfect direction to the targetPoint.
        foreach (var barrelTransform in barrelTransforms)
        {
            var barrelPos = barrelTransform.position;
            var goalDir = (targetPoint - barrelPos);
            goalDir = goalDir.normalized;
            var errorX = StarfallUtility.RandGaussian(_currentSpread);
            var errorY = StarfallUtility.RandGaussian(_currentSpread);
            var errorZ = StarfallUtility.RandGaussian(_currentSpread);
            goalDir.x += errorX;
            goalDir.y += errorY;
            goalDir.z += errorZ;

            //We want to reduce the recoverySharpness here
            _currentRecoverySharpness -= weaponData.recoveryImpact;

            //Fire, set the number of bullets in GameManager if applicable.
            Fire(goalDir);
        }

        //If this was the last shot, automatically start reloading
        if (_bulletsCurrentlyInMagazine == 0 && !_reloading)
        {
            Reload();
        }

    }
    
    protected virtual void Fire(Vector3 dir)
    {
        _timeLastFired = Time.time;
        _bulletsCurrentlyInMagazine--;
        if (_isOwnedByPlayer)
        {
            OnUpdatePlayerAmmo?.Invoke(_bulletsCurrentlyInMagazine, weaponData.magazineSize);
        }
    }


    /// <summary>
    /// This method tunes the spread to the desired level smoothly, over multiple frames.
    /// Spread is directly added to when the weapon is fired, and so this method must account for random changes in the currentSpread,
    /// while always moving to stabilize the spread at it's desired level.
    /// </summary>
    private void ManageSpread()
    {
        //Cap the spread at the weapon's max spread values
        if (_isAiming && _currentSpread > weaponData.maxAdsSpread)
        {
            _currentSpread = weaponData.maxAdsSpread;
            return;
        }

        if (!_isAiming && _currentSpread > weaponData.maxHipFireSpread)
        {
            _currentSpread = weaponData.maxHipFireSpread;
            return;
        }

        if (_currentRecoverySharpness < weaponData.minRecoverySharpness)
        {
            _currentRecoverySharpness = weaponData.minRecoverySharpness;
        }
        
        //Move the current recovery sharpness towards the max recovery sharpness with each call by some amount.
        _currentRecoverySharpness = Mathf.Lerp(_currentRecoverySharpness, weaponData.maxRecoverySharpness,
            1 - Mathf.Exp(-weaponData.recoveryRate * Time.deltaTime));

        if (!DoesSpreadNeedCorrection()) return;
        float goalSpread = _isAiming ? weaponData.minAdsSpread : weaponData.minHipFireSpread;
        _currentSpread = Mathf.Lerp(_currentSpread, goalSpread,
            1 - Mathf.Exp(-_currentRecoverySharpness * Time.deltaTime));

        if (_isOwnedByPlayer) OnCalculatePlayerSpread?.Invoke(_currentSpread);
    }

    private bool DoesSpreadNeedCorrection()
    {
        return _isAiming switch
        {
            true => Math.Abs(_currentSpread - weaponData.minAdsSpread) > .00001f,
            false => Math.Abs(_currentSpread - weaponData.minHipFireSpread) > .00001f
        };
    }

    public void Reload()
    {
        if (_bulletsCurrentlyInMagazine == weaponData.magazineSize) return;
        _reloading = true;
        if(_isOwnedByPlayer) OnPlayerReload?.Invoke(weaponData.reloadTime);
        Invoke(nameof(FillMagazine), weaponData.reloadTime);
    }

    public void FillMagazine()
    {
        _bulletsCurrentlyInMagazine = weaponData.magazineSize;
        OnUpdatePlayerAmmo?.Invoke(_bulletsCurrentlyInMagazine, weaponData.magazineSize);
        _reloading = false;
    }

    //TODO: Clean these up with appropriate { get; set; } syntax for each of them
    public float GetCurrentSpread()
    {
        return _currentSpread;
    }
    
    public void SetAiming(bool isAiming)
    {
        _isAiming = isAiming;
    }

    public float GetTimeLastFired()
    {
        return _timeLastFired;
    }

    public bool GetReloading()
    {
        return _reloading;
    }
    

    public WeaponData GetWeaponData()
    {
        return weaponData;
    }

    protected void HandleImpactEffects(GameObject impactedObj, RaycastHit hit)
    {
        var impactedObjMaterialType = impactedObj.GetComponent<ImpactEffectSurface>();
        if (impactedObjMaterialType == null) return;
        if (impactedObjMaterialType.impactSurfaceType == ImpactEffectSurface.ImpactSurfaceType.Flesh)
        {
            HandleBloodInstantiation(impactedObj, hit);
        }
        else
        {
            foreach (var kv in GameManager.Instance.SurfaceImpactPools)
            {
                if (kv.Key == impactedObjMaterialType.impactSurfaceType)
                {
                    HandleImpactInstantiation(kv.Value.Get(), hit);
                }
            }
        }
    }

    private void HandleBloodInstantiation(GameObject impactedObj, RaycastHit hit)
    {
        var bloodInstance = GameManager.Instance.BloodPool.Get();
        float angle = Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg + 180;
        bloodInstance.transform.position = hit.point;
        bloodInstance.transform.rotation = Quaternion.Euler(0, angle + 90, 0);
        var settings = bloodInstance.GetComponent<BFX_BloodSettings>();
        var motor = impactedObj.GetComponent<KinematicCharacterMotor>();
        float groundHeight;
        if (motor && motor.GroundingStatus.FoundAnyGround)
        {
            groundHeight = motor.GroundingStatus.GroundPoint.y;
        }
        else
        {
            var r = new Ray(hit.point, Vector3.down);
            if (Physics.Raycast(r, out var groundHit, 10f, 1 << LayerMask.NameToLayer("Default")))
            {
                groundHeight = groundHit.point.y;
            }
            else
            {
                groundHeight = -1000f;
            }
        }
        settings.GroundHeight = groundHeight;
    }

    private void HandleImpactInstantiation(GameObject impactObj, RaycastHit hit)
    {
        impactObj.transform.position = hit.point;
        impactObj.transform.LookAt(hit.point + hit.normal);
    }

}
