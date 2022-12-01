using UnityEngine;
using UnityEngine.Serialization;


public class Kuze : APlayer
{
    
    [FormerlySerializedAs("_anim")] public Animator anim;
    // Sample Abilities
    private MoveFastAbility _moveFastAbility;
    private BlinkAbility _blinkAbility;
    private DashAbility _dashAbility;
    private GrenadeAbility _grenadeAbility;
    private GrappleAbility _grappleAbility;

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

    private LinkBar linkBarUI;
    private static readonly int FallFromStandard = Animator.StringToHash("fallFromStandard");

    protected override void StartPlayer()
    {
        //linkBarUI = GameObject.Find("Link Bar").GetComponent<LinkBar>();

        // TODO(ben): Will this be player specific or general for all the
        // players?
        anim = base.GetComponentInChildren<Animator>();

        // NEW: GIVING ABILITIES TO KUZE
        // base.RegisterAbility(_moveFastAbility = new MoveFastAbility(this));

        // NEW: Blink ability where you can blink every 45 seconds. To blink, call onEnable.
        // RegisterAbility(_grenadeAbility = new GrenadeAbility(this));
        //
        // // NEW: This uses the default dash cooldown and cast delay
        // base.RegisterAbility(_dashAbility = new DashAbility(this, characterData.dashAbilityCooldownTime, characterData.dashAbilityTime));
        //
        // // NEW: 
        // base.RegisterAbility(_grappleAbility = new GrappleAbility(this, 5f));
    }

    protected override void UpdatePlayer()
    {
        base.UpdatePlayer();
        
        // Implement Kuze specific update code here

    }

    protected override void HandlePlayerInputs()
    {
        HandleAnimationInputs();

        // Implement other Kuze specific inputs here

        // If you press a specific key, call this function to toggle the ability
        // _moveFastAbility.Toggle();
    }

    public override void Damage(float damage)
    {
        base.Damage(damage);
    }

    public override void Heal(float healing)
    {
        base.Heal(healing);
    }

    // protected override void UseAbility1()
    // {
    //     base.UseAbility1();
    //     _grenadeAbility.Enable();
    // }
    //
    // protected override void UseAbility2()
    // {
    //     base.UseAbility2();
    //     _dashAbility.Enable();
    // }

    // Other players could have different animations.
    private void HandleAnimationInputs()
    {
        if (!anim) return;
        anim.SetBool(IsFiring, isFiring);
        anim.SetBool(IsAiming, isAiming);
        if (jumpRequested && !_jumpConsumed)
        {
            anim.SetTrigger(JumpDown);
        }
        anim.SetBool(InAir, !motor.GroundingStatus.FoundAnyGround);
        anim.SetBool(IsFalling, !motor.GroundingStatus.FoundAnyGround && motor.Velocity.y <= -.05f);
        if (anim.GetBool(IsFalling) || anim.GetBool(InAir))
        {
            Ray r = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(r, out var hit, 500f, 1 << LayerMask.NameToLayer("Default")))
            {
                anim.SetFloat(DistToGround, hit.distance);
            }
        }
        anim.SetBool(FallFromStandard, anim.GetBool(IsFalling) && anim.GetFloat(DistToGround) > 1.2);
        anim.SetFloat(VelX,  inputVector.x, .05f, Time.deltaTime);
        anim.SetFloat(VelY, inputVector.z, .05f, Time.deltaTime);
        anim.SetBool(IsMoving, inputVector.magnitude > 0.5f);
        anim.SetBool(LookAtCamera, orientationMethod == OrientationMethod.TowardsCamera);
    }
}