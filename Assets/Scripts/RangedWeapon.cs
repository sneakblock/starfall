using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using FMODUnity;

public abstract class RangedWeapon : MonoBehaviour
{

    [SerializeField] 
    [Tooltip("Add a weaponData scriptable object here to give the weapon the information it needs to behave.")]
    protected WeaponData weaponData;

    [SerializeField] [Tooltip("The transform position from which the weapon will be fired.")]
    protected Transform barrelTransform;
    
    [SerializeField]
    public EventReference gunShotEvent;


    private Crosshair _crosshair;
    private ReloadBar _reloadBar;
    private AmmoCounter _ammoCounter;

    private StarfallCharacterController _ownerChar;
    private float _currentSpread;
    private float _currentRecoverySharpness;
    private bool _isAiming;
    private Vector3 _firedDir;
    private float _timeLastFired = 0f;
    private int _bulletsCurrentlyInMagazine;
    private bool _reloading;
    private bool _bursting = false;
    protected EventInstance Instance;
    


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

    protected virtual void Start()
    {
        _currentSpread = weaponData.minHipFireSpread;
        _bulletsCurrentlyInMagazine = weaponData.magazineSize;
        if (gameObject.CompareTag("Player"))
        {
            _crosshair = StarfallPlayer.Instance.crosshair;
            _reloadBar = StarfallPlayer.Instance.reloadBar;
            _ammoCounter = StarfallPlayer.Instance.ammoCounter;
        }
        _ownerChar = GetComponentInParent<StarfallCharacterController>();
        _ammoCounter.UpdateAmmoCounter(_bulletsCurrentlyInMagazine, weaponData.magazineSize);
    }

    private void Update()
    {
        ManageSpread();
    }

    private float RandGaussian(float stddev, float mean = 0f)
    {
        float v0 = 1f - Random.Range(0f, 1f);
        float v1 = 1f - Random.Range(0f, 1f);
        float randNorm = Mathf.Sqrt(-2f * Mathf.Log(v0)) * Mathf.Sin(2f * Mathf.PI * v1);
        return mean + stddev * randNorm;
    }

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
                    StartCoroutine(FireBurst(weaponData.bulletsFiredPerShot, weaponData.burstDelay));
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

    IEnumerator FireBurst(int bulletsPerBurst, float burstDelay)
    {
        for (var i = 0; i < weaponData.bulletsFiredPerShot; i++)
        {
            if (_bulletsCurrentlyInMagazine > 0)
            {
                CalculateTrajectory(_ownerChar.GetTarget());
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
    /// spread, e.g the targetPoint is not sure to be hit if the spread is less than zero. Because it represents a successful
    /// firing of the bullet, it also sets _timeSinceLastBulletFired to 0. 
    /// </summary>
    /// <param name="targetPoint">
    /// The Vector3 at which the projectile should fire, AKA the location the agent is aiming at. 
    /// </param>
    private void CalculateTrajectory(Vector3 targetPoint)
    {
        _timeLastFired = Time.time;
        _bulletsCurrentlyInMagazine--;

        //The perfect direction to the targetPoint.
        var barrelPos = barrelTransform.position;
        var goalDir = (targetPoint - barrelPos);
        goalDir = goalDir.normalized;
        var errorX = RandGaussian(_currentSpread);
        var errorY = RandGaussian(_currentSpread);
        var errorZ = RandGaussian(_currentSpread);
        goalDir.x += errorX;
        goalDir.y += errorY;
        goalDir.z += errorZ;

        //We want to reduce the recoverySharpness here
        _currentRecoverySharpness -= weaponData.recoveryImpact;

        //Fire
        Fire(goalDir);
        _ammoCounter.UpdateAmmoCounter(_bulletsCurrentlyInMagazine, weaponData.magazineSize);

        //If this was the last shot, automatically start reloading
        if (_bulletsCurrentlyInMagazine == 0 && !_reloading)
        {
            Reload();
        }

    }

    //TODO: Make virtual, add sound function
    protected abstract void Fire(Vector3 dir);


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
        
        //Update UI if applicable
        if (_crosshair)
        {
            _crosshair.UpdateSize(_currentSpread);
        }
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
        //Animation and UI stuff here
        if (_reloadBar) _reloadBar.AnimateReloadBar(weaponData.reloadTime);
        Invoke(nameof(FillMagazine), weaponData.reloadTime);
    }

    private void FillMagazine()
    {
        _bulletsCurrentlyInMagazine = weaponData.magazineSize;
        _ammoCounter.UpdateAmmoCounter(_bulletsCurrentlyInMagazine, weaponData.magazineSize);
        _reloading = false;
    }

    private void OnDrawGizmos()
    {
        
    }


    public abstract void AnimateAim();
    public abstract void AnimateFire();
    public abstract void AnimateReload();
    public abstract void DoFireEffects();

    public float GetCurrentSpread()
    {
        return this._currentSpread;
    }

}
