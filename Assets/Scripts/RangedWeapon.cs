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
    private WeaponData weaponData;

    [SerializeField] [Tooltip("The transform position from which the weapon will be fired.")]
    private Transform barrelTransform;
    
    private float _currentSpread;
    private bool _isAiming;
    private Vector3 _firedDir;
    private List<Ray> _firedRays = new List<Ray>();
    private const int SecondsPerDebugRay = 2;
    private float _timeSinceLastBulletFired = Mathf.Infinity;

    public void SetAiming(bool isAiming)
    {
        _isAiming = isAiming;
    }

    private void Start()
    {
        _currentSpread = weaponData.minHipFireSpread;
    }

    private void Update()
    {
        switch (_isAiming)
        {
            case true:
                Aim();
                break;
            case false:
                UnAim();
                break;
        }
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
                
                break;
            case WeaponData.FiringMode.SemiAuto:
                if (!wasRequestingFireLastFrame)
                {
                    Fire(targetPoint);
                }
                break;
        }
    }

    /// <summary>
    /// This method fires the weapon from the barrelTransform to the targetPoint. This method should account for bullet
    /// spread, e.g the targetPoint is not sure to be hit if the spread is less than zero. Because it represents a successful
    /// firing of the bullet, it also sets _timeSinceLastBulletFired to 0. 
    /// </summary>
    /// <param name="targetPoint">
    /// The Vector3 at which the projectile should fire. The location the agent is aiming at. 
    /// </param>
    public void Fire(Vector3 targetPoint)
    {
        _timeSinceLastBulletFired = 0;

        //The perfect direction to the targetPoint.
        var barrelPos = barrelTransform.position;
        var goalDir = (targetPoint - barrelPos);
        goalDir = goalDir.normalized;
        var errorX = RandGaussian(_currentSpread);
        var errorY = RandGaussian(_currentSpread);
        var errorZ = RandGaussian(_currentSpread);
        Debug.Log("goalDir is " + goalDir);
        Debug.Log("With a current spread of " + _currentSpread + ", a Gaussian errorX of " + errorX + ", an errorY of " + errorY);
        //Rotate the vector along the y axis by the x error, and along the x axis by the y error.
        // goalDir = Quaternion.AngleAxis(errorX, Vector3.up) * goalDir;
        // goalDir = Quaternion.AngleAxis(errorY, Vector3.right) * goalDir;
        goalDir.x += errorX;
        goalDir.y += errorY;
        goalDir.z += errorZ;
        Debug.Log("After applying error, new goalDir is " + goalDir);

        //Now we want to actually fire the weapon, depending on what sort of weapon it is.
        Debug.DrawRay(barrelPos, goalDir * 1000f, Color.red, .5f);
        Debug.Log("Current spread is " + _currentSpread);
        

    }
    
    public void Aim()
    {
        // if (Math.Abs(_currentSpread - weaponData.minAdsSpread) > .0001f)
        // {
        //     _currentSpread = Mathf.Lerp(_currentSpread, weaponData.minAdsSpread,
        //         1 - Mathf.Exp(-weaponData.aimingBloomSharpness * Time.deltaTime));
        // }
        _currentSpread = weaponData.minAdsSpread;
    }

    public void UnAim()
    {
        // if (Math.Abs(_currentSpread - weaponData.minHipFireSpread) > .0001f)
        // {
        //     _currentSpread = Mathf.Lerp(_currentSpread, weaponData.minHipFireSpread,
        //         1 - Mathf.Exp(-weaponData.aimingBloomSharpness * Time.deltaTime));
        // }
        _currentSpread = weaponData.minHipFireSpread;
    }
    

    public void Reload()
    {
        
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
