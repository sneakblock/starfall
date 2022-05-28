using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class RangedWeapon : MonoBehaviour
{

    [SerializeField] 
    [Tooltip("Add a weaponData scriptable object here to give the weapon the information it needs to behave.")]
    private WeaponData weaponData;

    [SerializeField] [Tooltip("The transform position from which the weapon will be fired.")]
    private Transform barrelTransform;
    
    private float _currentSpread;
    private readonly Vector3 _screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    private bool _isAiming;

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

    public void CalculateTrajectory()
    {
        
    }
    

    public void Fire()
    {
        
    }
    

    public void Aim()
    {
        if (Math.Abs(_currentSpread - weaponData.minAdsSpread) > .001f)
        {
            _currentSpread = Mathf.Lerp(_currentSpread, weaponData.minAdsSpread,
                1 - Mathf.Exp(-weaponData.aimingBloomSharpness * Time.deltaTime));
        }
    }

    public void UnAim()
    {
        if (Math.Abs(_currentSpread - weaponData.minHipFireSpread) > .001f)
        {
            _currentSpread = Mathf.Lerp(_currentSpread, weaponData.minHipFireSpread,
                1 - Mathf.Exp(-weaponData.aimingBloomSharpness * Time.deltaTime));
        }
    }
    

    public void Reload()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null || barrelTransform == null) return;
        Vector3 dir = Camera.main.ScreenPointToRay(_screenCenterPoint).direction;
        float halfSpread = _currentSpread / 2f;
        Gizmos.color = Color.green;
        var position = barrelTransform.position;
        Gizmos.DrawRay(position, dir * 25f);
        Gizmos.color = Color.yellow;
        Quaternion leftSpreadRotation = Quaternion.AngleAxis(-halfSpread, Vector3.up);
        Quaternion rightSpreadRotation = Quaternion.AngleAxis(halfSpread, Vector3.up);
        Vector3 leftRayDir = leftSpreadRotation * dir;
        Vector3 rightRayDir = rightSpreadRotation * dir;
        Gizmos.DrawRay(position, leftRayDir * 25f);
        Gizmos.DrawRay(position, rightRayDir * 25f);
    }


    public abstract void AnimateAim();
    public abstract void AnimateFire();
    public abstract void AnimateReload();
    public abstract void DoFireEffects();

}
