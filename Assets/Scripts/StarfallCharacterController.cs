using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Events;

public class StarfallCharacterController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor motor;
    
    [Header("Standard Movement")]
    public float maxStableMoveSpeed = 10f;
    public float stableMovementSharpness = 15f;
    [Tooltip("The speed of the interpolation between the desired look direction and the character's current forward orientation.")]
    public float orientationSharpness = 10f;
    public OrientationMethod orientationMethod = OrientationMethod.TowardsMovement;
    
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

    [Header("Weapons")] [SerializeField] private RangedWeapon _weapon;
    [Range(0, 1)] public float aimingMovementPenalty;

    [Header("Misc")]
    public List<Collider> ignoredColliders = new List<Collider>();
    public Vector3 gravity = new Vector3(0, -30f, 0);
    public bool isPlayer;
    [Tooltip("These layers are considered when calculating the raycast for the desired target position")]
    public LayerMask firingLayerMask;
    
    //The character state stuff isn't used yet, but it will probably be useful when it comes to stuns, abilities, etc.
    public CharacterState CurrentCharacterState { get; set; }
    
    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private float _timeSinceJumpRequested;
    private bool _jumpRequested;
    private bool _jumpedThisFrame;
    private bool _jumpConsumed = false;
    private float _timeSinceLastAbleToJump;
    private bool _doubleJumpConsumed;
    
    //Firing stuff
    private bool _isAiming;
    private bool _isFiring;
    private bool _wasAimingLastFrame = false;
    private bool _wasFiringLastFrame = false;
    private Vector3 _screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
    private Camera _cam;

    public enum CharacterState
    {
        Default,
    }
    
    public struct StarfallPlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool Primary;
        public bool Aim;
        public bool Ability;
        public bool Gadget;
    }

    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
    }
    
    void Start()
    {
        motor.CharacterController = this;
        _cam = Camera.main;
    }
    
    void Update()
    {
        
        switch (_isAiming)
        {
            case true:
                if (!_wasAimingLastFrame)
                {
                    AimDown();
                }
                break;
            case false:
                if (_wasAimingLastFrame)
                {
                    AimUp();
                }
                break;
        }

        if (_isFiring)
        {
            RequestFirePrimary();
        }

        //Update old stuff
        _wasAimingLastFrame = _isAiming;
        _wasFiringLastFrame = _isFiring;
    }

    public void AimDown()
    {
        orientationMethod = OrientationMethod.TowardsCamera;
        maxStableMoveSpeed *= aimingMovementPenalty;
        _weapon.SetAiming(true);
    }

    public void AimUp()
    {
        orientationMethod = OrientationMethod.TowardsMovement;
        maxStableMoveSpeed /= aimingMovementPenalty;
        _weapon.SetAiming(false);
    }
    
    public void RequestFirePrimary()
    {
        if (isPlayer)
        {
            //Update the screen center point
            _screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = _cam.ScreenPointToRay(_screenCenterPoint);
            // var targetPoint = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, firingLayerMask) ? hit.point : _cam.ScreenToWorldPoint(new Vector3(_screenCenterPoint.x, _screenCenterPoint.y, _cam.farClipPlane));
            var targetPoint = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, firingLayerMask) ? hit.point : ray.GetPoint(1000f);
            _weapon.RequestFire(targetPoint, _wasFiringLastFrame);
        }
        
    }

    public void SetInputs(ref StarfallPlayerCharacterInputs inputs)
    {
        //Just sets the desired move vector, received from Player, and clamps it's magnitude to not exceed 1.
        _moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);
        
        // Calculate camera direction and rotation on the character plane
        // I don't fully understand this yet.
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, motor.CharacterUp);
        
        //Sets this local character's move and look inputs to what we've found.
        _moveInputVector = cameraPlanarRotation * _moveInputVector;
        
        //Allows setting if the character should look in the direction of the camera, e.g aiming, or in the direction of movement for general navigation.
        switch (orientationMethod)
        {
            case OrientationMethod.TowardsCamera:
                _lookInputVector = cameraPlanarDirection;
                break;
            case OrientationMethod.TowardsMovement:
                _lookInputVector = _moveInputVector.normalized;
                break;
        }

        if (inputs.JumpDown)
        {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
        }

        //Firing
        _isAiming = inputs.Aim;
        _isFiring = inputs.Primary;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_lookInputVector.sqrMagnitude > 0f && orientationSharpness > 0f)
        {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-orientationSharpness * deltaTime)).normalized;
        
            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, motor.CharacterUp);
        
        }
        
        Vector3 currentUp = (currentRotation * Vector3.up);
        Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -gravity.normalized, 1 - Mathf.Exp(-orientationSharpness * deltaTime));
        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //if the character is grounded
        if (motor.GroundingStatus.IsStableOnGround)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;
            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;
            
            // Orients velocity dir on slopes to be tangent to them, so velocity is not lost to friction
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
            
            //Gets the vector for horizontal movement according to the upward orientation of the character, useful for slopes or the char being tilted.
            Vector3 inputRight = Vector3.Cross(_moveInputVector, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized *
                                      _moveInputVector.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * maxStableMoveSpeed;
            
            //Smooth movement velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1 - Mathf.Exp(-stableMovementSharpness * deltaTime));
            
        }
        //if the character is airborne
        else
        {
            //Calculate air movement parameters, given that some input is being received (attempting to move in air)
            if (_moveInputVector.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = _moveInputVector * (airAccelerationSpeed * deltaTime);
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
        _timeSinceJumpRequested += deltaTime;

        if (_jumpRequested)
        {
            if (canDoubleJump)
            {
                if (_jumpConsumed && !_doubleJumpConsumed && (allowJumpingWhenSliding
                        ? !motor.GroundingStatus.FoundAnyGround
                        : !motor.GroundingStatus.IsStableOnGround))
                {
                    motor.ForceUnground(.1f);
                    
                    currentVelocity += (motor.CharacterUp * jumpUpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                    _jumpRequested = false;
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
                currentVelocity += (_moveInputVector * jumpScalableForwardSpeed);
                _jumpRequested = false;
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
        if (_jumpRequested && _timeSinceJumpRequested > jumpPreGroundingGraceTime)
        {
            _jumpRequested = false;
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
        return this._weapon;
    }
}
