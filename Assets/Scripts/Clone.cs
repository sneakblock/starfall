using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum CloneOrientation
{
    TowardsMovement,
    TowardsTarget
}

public enum MaterializationState
{
    Materializing,
    Dematerializing,
    Stable
}

public class Clone : SCharacter
{
    public Kuze baseKuze { get; set; }
    public Vector3 mirrorNormal { get; set; }
    
    public CloneAbility cloneAbility { get; set; }

    public CloneOrientation cloneOrientation { get; set; } = CloneOrientation.TowardsMovement;

    public MaterializationState materializationState = MaterializationState.Stable;

    [SerializeField] private GameObject characterMesh;
    [SerializeField] private float secondsToMaterialize = 1f;
    [SerializeField] private float dematerializedVertexResolution = 0f;
    [SerializeField] private float dematerializedVertexDisplacement = 5f;
    [SerializeField] [Range(0f, 1f)] private float dematerializedDistortionColorBalance = 1f;
    private float _materializationTimer;
    
    private static readonly int VertexResolution = Shader.PropertyToID("Vector1_B2CC132F");
    private static readonly int VertexDisplacmentAmount = Shader.PropertyToID("_VertexDisplacementAmount");
    private static readonly int DistortionColorBalance = Shader.PropertyToID("_DistortionColorBalance");
    private Dictionary<Renderer, List<Material>> _renderersMats = new();
    private Dictionary<Material, Dictionary<int, float>> _originalValues = new();

    public Animator anim;
    
    private static readonly int IsFiring = Animator.StringToHash("isFiring");
    private static readonly int VelX = Animator.StringToHash("velX");
    private static readonly int VelY = Animator.StringToHash("velY");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int LookAtCamera = Animator.StringToHash("lookAtCamera");
    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private static readonly int InAir = Animator.StringToHash("inAir");
    private static readonly int DistToGround = Animator.StringToHash("distToGround");
    private static readonly int JumpDown = Animator.StringToHash("jumpDown");
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int FallFromStandard = Animator.StringToHash("fallFromStandard");

    protected override void StartCharacter()
    {
        base.StartCharacter();
        anim = GetComponentInChildren<Animator>();
        baseKuze.orbitCamera.IgnoredColliders.Add(GetComponentInChildren<Collider>());
        foreach (var r in characterMesh.GetComponentsInChildren<Renderer>())
        {
            var materials = new List<Material>();
            foreach (var m in r.materials)
            {
                materials.Add(m);
                var paramValues = new Dictionary<int, float>();
                paramValues.Add(VertexResolution, m.GetFloat(VertexResolution));
                paramValues.Add(VertexDisplacmentAmount, m.GetFloat(VertexDisplacmentAmount));
                paramValues.Add(DistortionColorBalance, m.GetFloat(DistortionColorBalance));
                _originalValues.Add(m, paramValues);
            }
            _renderersMats.Add(r, materials);
        }
    }

    protected override void HandleInputs()
    {
        if (!baseKuze) return;

        if (baseKuze.MoveInputVector != Vector3.zero)
        {
            moveInputVector = Vector3.ClampMagnitude(Vector3.Reflect(baseKuze.MoveInputVector, mirrorNormal), 1f);
            Debug.DrawRay(transform.position, moveInputVector, Color.green, 1f);
        }
        else
        {
            moveInputVector = Vector3.zero;
        }

        var baseKuzeDirToTarget =
            baseKuze.GetTargetPoint() - (baseKuze.transform.position + baseKuze.motor.Capsule.center);
        var reflected = Vector3.Reflect(baseKuzeDirToTarget, mirrorNormal);
        var center = motor.Capsule.center;
        var position = transform.position;
        targetPoint = position + center + reflected;

        switch (cloneOrientation)
        {
            case CloneOrientation.TowardsMovement:
                lookInputVector = moveInputVector.normalized;
                break;
            case CloneOrientation.TowardsTarget:
                lookInputVector = (targetPoint - Collider.bounds.center);
                lookInputVector.y = 0f;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (baseKuze.RewiredPlayer.GetButtonDown("Jump"))
        {
            timeSinceJumpRequested = 0f;
            jumpRequested = true;
        }
        
        if (baseKuze.RewiredPlayer.GetButtonDown("Ability1"))
        {
            UseAbility1();
        }

        if (baseKuze.RewiredPlayer.GetButtonDown("Ability2"))
        {
            UseAbility2();
        }
        
        isAiming = baseKuze.RewiredPlayer.GetButton("Aim");
        isFiring = baseKuze.RewiredPlayer.GetButton("Fire");
        reloadedThisFrame = baseKuze.RewiredPlayer.GetButtonDown("Reload");
        
        HandleAnimationInputs();
    }

    protected override void AimDown()
    {
        base.AimDown();
        cloneOrientation = CloneOrientation.TowardsTarget;
    }

    protected override void AimUp()
    {
        base.AimUp();
        cloneOrientation = CloneOrientation.TowardsMovement;
    }

    public override void Damage(float damage)
    {
        //IMMUNE
    }

    private void HandleAnimationInputs()
    {
        if (!anim) return;
        anim.SetBool(IsFiring, isFiring);
        anim.SetBool(IsAiming, isAiming);
        if (jumpRequested && !_jumpConsumed)
        {
            anim.SetTrigger(JumpDown);
        }
        
        anim.SetBool(InAir, !motor.GroundingStatus.IsStableOnGround);
        anim.SetBool(IsFalling, !motor.GroundingStatus.IsStableOnGround && motor.Velocity.y <= -.05f);
        if (anim.GetBool(InAir) || anim.GetBool(IsFalling))
        {
            Ray r = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(r, out var hit, 500f, 1 << LayerMask.NameToLayer("Default")))
            {
                anim.SetFloat(DistToGround, hit.distance);
            }
        }
        anim.SetBool(FallFromStandard, anim.GetBool(IsFalling) && anim.GetFloat(DistToGround) > 1.2);
        anim.SetFloat(VelX,  moveInputVector.x, .05f, Time.deltaTime);
        anim.SetFloat(VelY, moveInputVector.z, .05f, Time.deltaTime);
        anim.SetBool(IsMoving, moveInputVector.magnitude > 0.5f);
        anim.SetBool(LookAtCamera, cloneOrientation == CloneOrientation.TowardsTarget);
    }

    protected override void RequestFirePrimary()
    {
        if (!isFiring) return;
        base.RequestFirePrimary();
        if (!_weapon.GetReloading() && gameObject.activeInHierarchy)
        {
            StartCoroutine(OrientationTimer(baseKuze.secondsToLockShootingOrientation));
        }
    }

    protected override void UpdateCharacter()
    {
        if (materializationState is not (MaterializationState.Dematerializing or MaterializationState.Materializing))
            return;
        _materializationTimer -= Time.deltaTime;
        if (_materializationTimer <= 0f)
        {
            SetMaterialization(MaterializationState.Stable);
            return;
        }

        if (materializationState is MaterializationState.Materializing)
        {
            motor.SetPosition(baseKuze.motor.GetState().Position);
            motor.SetRotation(baseKuze.motor.GetState().Rotation);
        }
        DoMaterialization();
    }

    private void DoMaterialization()
    {
        foreach (var kv in _renderersMats)
        {
            foreach (var m in kv.Key.materials)
            {
                var restingValues = _originalValues[m];
                m.SetFloat(VertexResolution, materializationState is MaterializationState.Materializing ? Mathf.Lerp(dematerializedVertexResolution, restingValues[VertexResolution], 1 - _materializationTimer / secondsToMaterialize) : Mathf.Lerp(restingValues[VertexResolution], dematerializedVertexResolution, 1 - _materializationTimer / secondsToMaterialize));
                m.SetFloat(VertexDisplacmentAmount, materializationState is MaterializationState.Materializing ? Mathf.Lerp(dematerializedVertexDisplacement, restingValues[VertexDisplacmentAmount], 1 - _materializationTimer / secondsToMaterialize) : Mathf.Lerp(restingValues[VertexDisplacmentAmount], dematerializedVertexDisplacement, 1 - _materializationTimer / secondsToMaterialize));
                m.SetFloat(DistortionColorBalance, materializationState is MaterializationState.Materializing ? Mathf.Lerp(dematerializedDistortionColorBalance, restingValues[DistortionColorBalance], 1 - _materializationTimer / secondsToMaterialize) : Mathf.Lerp(restingValues[DistortionColorBalance], dematerializedDistortionColorBalance, 1 - _materializationTimer / secondsToMaterialize));
            }
        }
    }

    public void SetMaterialization(MaterializationState state)
    {
        if (materializationState == MaterializationState.Dematerializing && state == MaterializationState.Stable)
        {
            WipeInputs();
            gameObject.SetActive(false);
            return;
        }
        materializationState = state;
        if (materializationState is (MaterializationState.Dematerializing or MaterializationState.Materializing))
        {
            _materializationTimer = secondsToMaterialize;
        } else if (materializationState is MaterializationState.Stable)
        {
            foreach (var kv in _renderersMats)
            {
                //Reset materials to default values
                foreach (var m in kv.Key.materials)
                {
                    m.SetFloat(VertexResolution, _originalValues[m][VertexResolution]);
                    m.SetFloat(VertexDisplacmentAmount, _originalValues[m][VertexDisplacmentAmount]);
                    m.SetFloat(DistortionColorBalance, _originalValues[m][DistortionColorBalance]);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(targetPoint, .25f);
    }

    public void CallOrientationTimer()
    {
        StartCoroutine(OrientationTimer(baseKuze.secondsToLockShootingOrientation));
    }
    
    private IEnumerator OrientationTimer(float duration)
    {
        cloneOrientation = CloneOrientation.TowardsTarget;
        yield return new WaitForSeconds(duration);
        if (!isAiming && Time.time - _weapon.GetTimeLastFired() >= duration - .1f)
            cloneOrientation = CloneOrientation.TowardsMovement;
    }

    private void WipeInputs()
    {
        GetComponent<DaggerAbility>().RecoverAllDaggers();
        cloneOrientation = CloneOrientation.TowardsMovement;
        jumpRequested = false;
        isAiming = false;
        isFiring = false;
        moveInputVector = Vector3.zero;
        lookInputVector = Vector3.zero;
        _weapon.FillMagazine();
    }

    public override void Kill()
    {
        //Can't kill a clone...
    }
}
