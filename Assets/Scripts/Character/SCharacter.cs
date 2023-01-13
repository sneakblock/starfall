using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class SCharacter : MonoBehaviour, IDamageable, ICharacterController
{
    public KinematicCharacterMotor motor;

    [Header("Link")][SerializeField] 
    public float health = 100;
    public float maxHealth { get; protected set; }

    //TODO(mish): make these private vars
    [Header("Standard Movement")]
    public float maxStableMoveSpeed = 10f;
    public float stableMovementSharpness = 15f;
    [Tooltip("The speed of the interpolation between the desired look direction and the character's current forward orientation.")]
    public float orientationSharpness = 10f;

    [Header("Air Movement")]
    public float maxAirMoveSpeed = 15f;
    public float airAccelerationSpeed = 15f;
    public float drag = 0.1f;

    [Header("Jumping")]
    public bool allowJumpingWhenSliding = true;
    public bool canDoubleJump = false;
    public float jumpUpSpeed = 10f;
    public float jumpScalableForwardSpeed = 10f;
    [Tooltip("The JumpPreGroundingGraceTime and JumpPostGroundingGraceTime respectively represent the extra time before landing where you can press jump and it’ll still jump once you land, and the extra time after leaving stable ground where you’ll still be allowed to jump.")]
    public float jumpPreGroundingGraceTime = 0f;
    public float jumpPostGroundingGraceTime = 0f;

    [Header("Weapons")][SerializeField] protected RangedWeapon _weapon;
    [Range(0, 1)] public float aimingMovementPenalty = .75f;

    [Header("Misc")]
    public List<Collider> ignoredColliders = new List<Collider>();
    public LayerMask layerMask;
    public Vector3 gravity = new Vector3(0, -30f, 0);
    public Collider Collider;

    public AudioClip[] deathSoundClips;
    public AudioSource deathSoundSource;
    
    public bool isDead = false;


    //Moving and jumping
    protected Vector3 moveInputVector;
    public Vector3 lookInputVector;
    protected float timeSinceJumpRequested;
    protected bool jumpRequested;
    protected bool _jumpedThisFrame;
    protected bool _jumpConsumed = false;
    private float _timeSinceLastAbleToJump;
    private bool _doubleJumpConsumed;

    public Vector3 MoveInputVector => moveInputVector;

    //Firing stuff
    protected bool isAiming;
    protected bool isFiring;
    protected bool wasAimingLastFrame = false;
    protected bool _wasFiringLastFrame = false;
    private Vector3 _screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
    protected Vector3 targetPoint;
    protected bool reloadedThisFrame;

    //Abilities
    private List<Ability> _abilities;
    private AbilityManager abilityManager;
    private const int MAX_ABILITIES = 3;


    //Events
    //Invoked when the entity is damaged
    //public static event Action<int, int> OnHit;


    //Bleed
    protected bool isBleeding = false;
    protected float bleedTimer = 0f;
    protected float bleedTickRate = .25f;
    protected float bleedTickTracker = 0f;
    protected float bleedDmgPerTick;

    [Header("Ragdoll")] [SerializeField] private bool ragdollOnDeath = true;
    [SerializeField] private Transform ragdollRoot;

    private CapsuleCollider _ragdollMotorCollider;
    private List<CharacterJoint> _ragdollCharacterJoints = new();
    private List<Collider> _ragdollColliders = new();
    private List<Rigidbody> _ragdollRigidbodies = new();
    private Animator _ragdollAnim;


    void Start()
    {
        isDead = false;
        abilityManager = new AbilityManager();
        motor.CharacterController = this;
        maxHealth = health;
        Collider = GetComponent<Collider>();
        
        //If the weapon was not set in editor, the SCharacter will attempt to find a weapon in its children.
        //This is more or less a quality of life functionality, allowing quicker weapon swaps without needing to always wire things up
        //in the editor. However, if a weapon is assigned in editor, this will not execute. 
        if (!_weapon)
        {
            _weapon = GetComponentInChildren<RangedWeapon>(false);
        }

        if (ragdollOnDeath)
        {
            SetupRagdoll();
            DisableRagdoll();
        }

        //Abstract, to be overridden.
        StartCharacter();

        // NEW: calls start function for every single registered ability once
        abilityManager.Start();
    }

    void SetupRagdoll()
    {
        if (!ragdollRoot)
        {
            Debug.LogWarning($"SCharacter {gameObject.name} does not have an assigned ragdoll root.");
            return;
        }

        _ragdollAnim = GetComponentInChildren<Animator>();
        if (!_ragdollAnim)
        {
            Debug.LogWarning($"{gameObject.name} could not find an animator in SetupRagdoll()");
            return;
        }

        _ragdollMotorCollider = motor.Capsule;
        _ragdollCharacterJoints = ragdollRoot.GetComponentsInChildren<CharacterJoint>().ToList();
        _ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>().ToList();
        _ragdollRigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>().ToList();

        // Ensure these colldiers are treated as colliders of the SCharacter's layer.
        foreach (var c in _ragdollColliders)
        {
            c.gameObject.layer = gameObject.layer;
        }
    }

    protected void DisableRagdoll()
    {
        if (!_ragdollAnim || _ragdollColliders.Count == 0 || _ragdollCharacterJoints.Count == 0 ||
            _ragdollRigidbodies.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name} tried to DisableRagdoll() but failed, as one or more references were not set up.");
            return;
        }

        _ragdollAnim.enabled = true;

        _ragdollMotorCollider.enabled = true;

        foreach (var j in _ragdollCharacterJoints)
        {
            j.enableCollision = false;
        }

        foreach (var c in _ragdollColliders)
        {
            c.enabled = false;
        }

        foreach (var r in _ragdollRigidbodies)
        {
            r.velocity = Vector3.zero;
            r.detectCollisions = false;
            r.useGravity = false;
            r.isKinematic = true;
        }
    }
    
    protected void DoRagdoll()
    {
        if (!_ragdollAnim || _ragdollColliders.Count == 0 || _ragdollCharacterJoints.Count == 0 ||
            _ragdollRigidbodies.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name} tried to DoRagdoll() but failed, as one or more references were not set up.");
            return;
        }

        _ragdollAnim.enabled = false;
        _ragdollMotorCollider.enabled = false;

        foreach (var j in _ragdollCharacterJoints)
        {
            j.enableCollision = true;
        }

        foreach (var c in _ragdollColliders)
        {
            c.enabled = true;
        }

        foreach (var r in _ragdollRigidbodies)
        {
            r.velocity = Vector3.zero;
            r.detectCollisions = true;
            r.useGravity = true;
            r.isKinematic = false;
        }
    }

	void Update()
	{
        HandleInputs();
        UpdateCharacter();

        // NEW: calls update function for every single registered ability
        abilityManager.Update();

        if (_weapon) UpdateWeapon();
        if (isBleeding) UpdateBleed();
    }
    
    // NEW: subclasses (players or... enemies...) should call this function to pair an ability to a player
    protected void RegisterAbility(Ability ability)
    {
        abilityManager.Register(ability);
    }

    protected abstract void HandleInputs();

    protected virtual void StartCharacter()
    {
        // Get all abilities attached to this GameObject
        _abilities = GetComponents<Ability>().ToList();
        
        for (int i = 0; i < MAX_ABILITIES; i++)
        {
            if (i >= _abilities.Count || !_abilities[i]) break;
            _abilities[i].SetCharacter(this);
            abilityManager.Register(_abilities[i]);
        }
    }

    protected abstract void UpdateCharacter();

    void UpdateWeapon()
    {
        //Aim down: First time aiming
        if (isAiming && !wasAimingLastFrame)
        {
            AimDown();
        }

        //Aim up: Stopped aiming
        if (!isAiming && wasAimingLastFrame)
        {
            AimUp();
        }

        if (isFiring)
        {
            RequestFirePrimary();
        }

        if (reloadedThisFrame && _weapon)
        {
            _weapon.Reload();
        }

        wasAimingLastFrame = isAiming;
        _wasFiringLastFrame = isFiring;
    }

    protected virtual void AimDown()
    {
        maxStableMoveSpeed *= aimingMovementPenalty;
        _weapon.SetAiming(true);
    }

    protected virtual void AimUp()
    {
        maxStableMoveSpeed /= aimingMovementPenalty;
        _weapon.SetAiming(false);
    }
   
    protected virtual void RequestFirePrimary()
    {
        if (!_weapon || !gameObject.activeInHierarchy) return;
        _weapon.RequestFire(targetPoint, _wasFiringLastFrame);
    }

    public virtual void Damage(float damage)
    {
        //call event for hitmarker
        //OnHit?.Invoke();
        // if (health <= 0) return;
        if (isDead) return;
        health -= damage;
        if (health <= 0)
        {
            Kill();
        }
    }

	public virtual void Heal(float healing)
    {
        health += healing;
        if (health > maxHealth) health = maxHealth;
    }

    public virtual void Kill()
    {
        isDead = true;
        if (deathSoundClips.Length > 0 && deathSoundSource)
        {
            Debug.Log($"{gameObject.name} played death sound.");
            PlayDeathSound();
        }
        // All SCharacters disable their motor on death.
        motor.enabled = false;
        if (_weapon) _weapon.enabled = false;
        if (ragdollOnDeath) DoRagdoll();
    }

    private void PlayDeathSound()
    {
        var clip = deathSoundClips[Random.Range(0, deathSoundClips.Length)];
        deathSoundSource.PlayOneShot(clip);
    }

    public void StartBleeding(float totalDamage, float duration)
    {
        isBleeding = true;
        bleedTimer = duration;
        float ticksInDuration = duration / bleedTickRate;
        bleedDmgPerTick = totalDamage / ticksInDuration;
    }

    public void StopBleeding()
    {
        isBleeding = false;
    }

    public bool IsBleeding()
    {
        return isBleeding;
    }

    public virtual void UpdateBleed()
    {
        bleedTickTracker += Time.deltaTime;
        if (bleedTickTracker >= bleedTickRate)
        {
            //Bleed here
            Damage(bleedDmgPerTick);

            //Blood effects
            var bloodInstance = GameManager.Instance.BloodPool.Get();
            var position = transform.position;
            var center = new Vector3(position.x, position.y + motor.Capsule.center.y, position.z);
            float angle = Mathf.Atan2(position.x, position.z) * Mathf.Rad2Deg + 180;
            bloodInstance.transform.position = center;
            bloodInstance.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            var settings = bloodInstance.GetComponent<BFX_BloodSettings>();
            float groundHeight;
            if (motor && motor.GroundingStatus.FoundAnyGround)
            {
                groundHeight = motor.GroundingStatus.GroundPoint.y;
            }
            else
            {
                var r = new Ray(center, Vector3.down);
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

            //Reset the tick tracker
            bleedTickTracker = 0f;
        }

        bleedTimer -= Time.deltaTime;
        if (bleedTimer <= 0f)
        {
            StopBleeding();
        }
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    //These functions can be overridden in subclasses for more flexibility
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (lookInputVector.sqrMagnitude > 0f && orientationSharpness > 0f)
        {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(motor.CharacterForward, lookInputVector, 1 - Mathf.Exp(-orientationSharpness * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, motor.CharacterUp);

        }

        Vector3 currentUp = (currentRotation * Vector3.up);
        Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -gravity.normalized, 1 - Mathf.Exp(-orientationSharpness * deltaTime));
        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
    }

    public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //if the character is grounded
        if (motor.GroundingStatus.IsStableOnGround)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;
            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;

            // Orients velocity dir on slopes to be tangent to them, so velocity is not lost to friction
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

            //Gets the vector for horizontal movement according to the upward orientation of the character, useful for slopes or the char being tilted.
            Vector3 inputRight = Vector3.Cross(moveInputVector, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized *
                                      moveInputVector.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * maxStableMoveSpeed;

            //Smooth movement velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1 - Mathf.Exp(-stableMovementSharpness * deltaTime));

        }
        //if the character is airborne
        else
        {
            //Calculate air movement parameters, given that some input is being received (attempting to move in air)
            if (moveInputVector.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = moveInputVector * (airAccelerationSpeed * deltaTime);
                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

                // Limiting air velocity
                if (currentVelocityOnInputsPlane.magnitude < maxAirMoveSpeed)
                {
                    //clamp added velocity so that it does not exceed the max air move speed, and reassign it.
                    //Not sure why this needs to involve the inputs on the plane, I guess to isolate inputs?
                    Vector3 newTotal =
                        Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, maxAirMoveSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    //If the velocity is equal to the maxAirMoveSpeed
                    // Don't understand this.
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                    {
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                    }
                }

                // Prevent air-climbing sloped walls
                // This was copied verbatim from the example code, I don't understand how it works.
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        Vector3 perpendicularObstructionNormal = Vector3.Cross(Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal), motor.CharacterUp).normalized;
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpendicularObstructionNormal);
                    }
                }

                // Apply added velocity to the air movement
                currentVelocity += addedVelocity;

            }

            // Apply Gravity
            currentVelocity += gravity * deltaTime;

            // Apply Drag
            currentVelocity *= (1f / (1f + (drag * deltaTime)));
        }

        // Jumping logic
        // We have not jumped this frame, (yet)
        _jumpedThisFrame = false;
        // Keep track of the time since last jump was requested
        timeSinceJumpRequested += deltaTime;

        if (jumpRequested)
        {
            if (canDoubleJump)
            {
                if (_jumpConsumed && !_doubleJumpConsumed && (allowJumpingWhenSliding
                        ? !motor.GroundingStatus.FoundAnyGround
                        : !motor.GroundingStatus.IsStableOnGround))
                {
                    motor.ForceUnground(.1f);

                    currentVelocity += (motor.CharacterUp * jumpUpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                    jumpRequested = false;
                    _doubleJumpConsumed = true;
                    _jumpedThisFrame = true;
                }
            }

            // Check if jump request is valid given constraints, cool-downs, etc.
            if (!_jumpConsumed && (allowJumpingWhenSliding ? motor.GroundingStatus.FoundAnyGround : motor.GroundingStatus.IsStableOnGround || _timeSinceLastAbleToJump <= jumpPostGroundingGraceTime))
            {
                // Calculate the jump direction
                Vector3 jumpDirection = motor.CharacterUp;
                if (motor.GroundingStatus.FoundAnyGround && !motor.GroundingStatus.IsStableOnGround)
                {
                    // If we are on a slope(?) jump according to it's tilt and not straight up.
                    // Double check this.
                    jumpDirection = motor.GroundingStatus.GroundNormal;
                }

                motor.ForceUnground();

                // Perform the jump by adding it to the currentVelocity, and set bools appropriately.
                // WHY does this involve a projection?
                currentVelocity += (jumpDirection * jumpUpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                currentVelocity += (moveInputVector * jumpScalableForwardSpeed);
                jumpRequested = false;
                _jumpConsumed = true;
                _jumpedThisFrame = true;
            }
        }

        //TODO: Look into an internal velocity add, for explosions etc.
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Handle jumping pre-ground grace period
        if (jumpRequested && timeSinceJumpRequested > jumpPreGroundingGraceTime)
        {
            jumpRequested = false;
        }

        if (allowJumpingWhenSliding ? motor.GroundingStatus.FoundAnyGround : motor.GroundingStatus.IsStableOnGround)
        {
            // If we're on a ground surface, reset jumping values
            if (!_jumpedThisFrame)
            {
                _doubleJumpConsumed = false;
                _jumpConsumed = false;
            }
            _timeSinceLastAbleToJump = 0f;
        }
        else
        {
            // Keep track of time since we were last able to jump (for grace period)
            _timeSinceLastAbleToJump += deltaTime;
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (ignoredColliders.Count == 0)
        {
            return true;
        }

        return !ignoredColliders.Contains(coll);
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }

    public RangedWeapon GetRangedWeapon()
    {
        return _weapon;
    }

    public Vector3 GetTargetMovementDirection()
    {
        Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;
        //Gets the vector for horizontal movement according to the upward orientation of the character, useful for slopes or the char being tilted.
            Vector3 inputRight = Vector3.Cross(moveInputVector, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized *
                                      moveInputVector.magnitude;
        return reorientedInput.normalized;
    }
    
    protected virtual void UseAbility1()
    {
        //If there's any generic stuff like updating ui or something, it can go here,
        //as every character should call base.UseAbility1()
        if (!_abilities[0]) return;
        _abilities[0].Enable();
    }

    protected virtual void UseAbility2()
    {
        if (!_abilities[1]) return;
        _abilities[1].Enable();
    }

    protected virtual void UseAbility3()
    {
        if (!_abilities[2]) return;
        _abilities[2].Enable();
    }

    public Vector3 GetTargetPoint()
    {
        return targetPoint;
    }

}

