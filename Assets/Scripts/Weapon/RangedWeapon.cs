using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public abstract class RangedWeapon : MonoBehaviour
{

    [SerializeField] 
    [Tooltip("Add a weaponData scriptable object here to give the weapon the information it needs to behave.")]
    protected WeaponData weaponData;

    [SerializeField] [Tooltip("The transform position from which the weapon will be fired.")]
    protected Transform barrelTransform;

    // NOTE(cameron): This is somewhat weird, but I need to put this here because
    // Player needs a LayerMask in order to raycast and change the player's view.
    public LayerMask FiringMask { get; private set; }
    
    //The owner of the weapon
    private SCharacterController _ownerChar;
    private bool _isOwnedByPlayer;
    
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

    protected virtual void Start()
    {
        _currentSpread = weaponData.minHipFireSpread;
        _bulletsCurrentlyInMagazine = weaponData.magazineSize;
        _ownerChar = GetComponentInParent<SCharacterController>();
        if (GameManager.Instance) {
            //_isOwnedByPlayer = GameManager.Instance.GetPlayer().GetCharacter() == _ownerChar;
            _isOwnedByPlayer = false;
        }
        if (_isOwnedByPlayer)
        {
            GameManager.Instance.playerData.weaponSpread = _currentSpread;
            GameManager.Instance.playerData.totalAmmo = weaponData.magazineSize;
            GameManager.Instance.playerData.currentAmmo = _bulletsCurrentlyInMagazine;
        }
    }

    private void Update()
    {
        ManageSpread();
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
        if (_isOwnedByPlayer)
        {
            GameManager.Instance.playerData.currentAmmo = _bulletsCurrentlyInMagazine;
            GameManager.Instance.GetPlayer().onPlayerFire.Invoke();
        }

        //If this was the last shot, automatically start reloading
        if (_bulletsCurrentlyInMagazine == 0 && !_reloading)
        {
            Reload();
        }

    }
    
    protected virtual void Fire(Vector3 dir)
    {
        Debug.Log("Fire");
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

        if (_isOwnedByPlayer) GameManager.Instance.playerData.weaponSpread = _currentSpread;
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
        if(_isOwnedByPlayer) GameManager.Instance.GetPlayer().onPlayerReloadStart.Invoke();
        //Animation and UI stuff here
        Invoke(nameof(FillMagazine), weaponData.reloadTime);
    }

    private void FillMagazine()
    {
        _bulletsCurrentlyInMagazine = weaponData.magazineSize;
        if (_isOwnedByPlayer)
        {
            GameManager.Instance.playerData.currentAmmo = _bulletsCurrentlyInMagazine;
            GameManager.Instance.GetPlayer().onPlayerReloadComplete.Invoke();
        }
        _reloading = false;
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
