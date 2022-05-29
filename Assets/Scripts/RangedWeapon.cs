using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private Vector3 _firedDir;
    private List<Ray> _firedRays = new List<Ray>();
    private const int SecondsPerDebugRay = 2;

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

    private Vector3 CalculateTrajectory(Vector3 targetPoint)
    {
        var goalDir = (targetPoint - barrelTransform.position).normalized;
        var pointOnSpreadCircle = Random.insideUnitCircle * _currentSpread;
        Vector3 trueDir = new Vector3(goalDir.x + pointOnSpreadCircle.x, goalDir.y, goalDir.z + pointOnSpreadCircle.y).normalized;
        return goalDir;
        
        // Quaternion implementation (?)
        // Quaternion deviationRotation = Quaternion.Euler(pointOnSpreadCircle.x, pointOnSpreadCircle.y, 0);
    }
    

    public void Fire()
    {
        if (Camera.main == null) return;
        //TODO: Change this to allow for targetPoints that aren't just the player's target point. This is essential if we want to be able to use this logic for enemy or AI weapons.
        
        //TODO: THIS DOESN'T WORK LOL :)
        _firedDir = CalculateTrajectory(Camera.main.ScreenPointToRay(_screenCenterPoint).direction);
        StartCoroutine(DebugRayManager(_firedDir));

    }

    IEnumerator DebugRayManager(Vector3 toDir)
    {
        Ray r = new Ray(barrelTransform.position, toDir);
        _firedRays.Add(r);

        yield return new WaitForSeconds(SecondsPerDebugRay);

        _firedRays.Remove(r);
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

        Gizmos.color = Color.red;
        foreach (var r in _firedRays)
        {
            Gizmos.DrawRay(r);
        }
    }


    public abstract void AnimateAim();
    public abstract void AnimateFire();
    public abstract void AnimateReload();
    public abstract void DoFireEffects();

}
