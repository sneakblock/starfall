using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KinematicCharacterController.Examples;
using KinematicCharacterController;
using Rewired;
using UnityEngine.Events;

[System.Serializable] public class _UnityEventFloat:UnityEvent<float> {}

public abstract class APlayer : SCharacter
{
    [Header("Camera Info")] public Transform orbitPoint;
    
    [Header("Orbit Camera")]  public ExampleCharacterCamera orbitCamera;
    
    public OrientationMethod orientationMethod = OrientationMethod.TowardsMovement;
    
    [SerializeField]
    [Tooltip("How many seconds should the character lock into 'towards camera' orientation after firing from the hip?")]
    private float secondsToLockShootingOrientation = 1f;

    protected const string HorizontalInput = "Horizontal";
    protected const string VerticalInput = "Vertical";
    private bool _oldAim = false;

    // Camera exists for APlayer and not SCharacter because Enemy is an
    // SCharacter and they do not deserve a camera.
    protected Camera cam;
    
    //Rewired input system
    protected const int PlayerID = 0;
    protected Rewired.Player RewiredPlayer;

    //The input vector, clamped to 1, as read by the input system, before any transformations regarding camera/into world space.
    //Useful for animations.
    protected Vector3 inputVector;

    public _UnityEventFloat invokeLinkSlider = new _UnityEventFloat();

    private int _zoom = 1;

    //Event to reset score on player death
    public static event Action OnPlayerDeath;
    //Event to decrease multiplier when damage is taken
    public static event Action OnDamage;
    
    
    public int linkDamagePerSec = 3;
    protected bool isDying = true;
    
    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
    }
    
    protected override void StartCharacter()
    {
        base.StartCharacter();
        if (!orbitCamera) orbitCamera = GameObject.FindWithTag("MainCamera").GetComponent<ExampleCharacterCamera>();
        
        cam = orbitCamera.Camera;
        orbitCamera.SetFollowTransform(orbitPoint);

        //Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;

        // Ignore the character's collider(s) for camera obstruction checks
        orbitCamera.IgnoredColliders = base.GetComponentsInChildren<Collider>().ToList();

        RewiredPlayer = ReInput.players.GetPlayer(PlayerID);

        StartPlayer();

        base._maxHealth = 200;
    }

    protected override void UpdateCharacter()
    {
        UpdatePlayer();
    }

    protected override void HandleInputs()
    {
        //TODO: Old input system used here. Update to Rewired.
        if (Input.GetKeyDown(KeyCode.F))
        {
            //Kill();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Calculate camera direction and rotation on the character plane
        // I don't fully understand this yet.
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(orbitCamera.Transform.rotation * Vector3.forward, motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(orbitCamera.Transform.rotation * Vector3.up, motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, motor.CharacterUp);

        SetOrientation(cameraPlanarDirection);

        float moveAxisForward = RewiredPlayer.GetAxisRaw("MoveForward");
        float moveAxisRight = RewiredPlayer.GetAxisRaw("MoveRight");

        //Just sets the desired move vector, received from Player, and clamps it's magnitude to not exceed 1.
        inputVector = Vector3.ClampMagnitude(new Vector3(moveAxisRight, 0f, moveAxisForward), 1f);

        //Sets this local character's move and look inputs to what we've found.
        base.moveInputVector = cameraPlanarRotation * inputVector;

        if (RewiredPlayer.GetButtonDown("Jump"))
        {
            timeSinceJumpRequested = 0f;
            jumpRequested = true;
        }

        if (RewiredPlayer.GetButtonDown("Ability1"))
        {
            UseAbility1();
        }

        if (RewiredPlayer.GetButtonDown("Ability2"))
        {
            UseAbility2();
        }

        if (RewiredPlayer.GetButtonDown("Ability3"))
        {
            UseAbility3();
        }


        //TODO(ben): Not sure if this is the best way to handle this-- target needs to be set on the SCharacter level because AI agents also have ideal target points. But calculating the center screen point every frame seems a bit silly and/or goofy. (It's only like this right now because moving the Unity window mid-play will break the "center" of the screen.)
        //Update the screen center point
        var screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(screenCenterPoint);
        bool rayhit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask);
        var tp =  rayhit ? hit.point : ray.GetPoint(1000f);

        base.targetPoint = tp;

        HandleFiringInputs();

        // Calls player specific inputs (each character can have extra
        // player inputs).
        HandlePlayerInputs();

    }

    protected abstract void HandlePlayerInputs();
    protected abstract void StartPlayer();

    protected virtual void UpdatePlayer()
    {
        //kill player by x amount every second
        if(isDying) {
            Damage(linkDamagePerSec * Time.deltaTime);
            invokeLinkSlider.Invoke(linkDamagePerSec * Time.deltaTime);
        }
    }

    protected override void AimDown()
    {
        base.AimDown();
        orientationMethod = OrientationMethod.TowardsCamera;
    }

    protected override void AimUp()
    {
        base.AimUp();
        orientationMethod = OrientationMethod.TowardsMovement;
    }

    protected override void RequestFirePrimary()
    {
        base.RequestFirePrimary();
        if (!_weapon.GetReloading())
        {
            //TODO(ben): Right now this works fine, but it's running a lot of coroutines in the background. Can probably clean this up with a simpler Time.time approach.
            StartCoroutine(OrientationTimer(secondsToLockShootingOrientation));
        }
    }

    private IEnumerator OrientationTimer(float duration)
    {
        orientationMethod = OrientationMethod.TowardsCamera;
        yield return new WaitForSeconds(duration);
        if (!isAiming && Time.time - _weapon.GetTimeLastFired() >= duration - .1f) orientationMethod = OrientationMethod.TowardsMovement;
    }

    private void ToggleZoom()
    {
        _zoom = (_zoom == 1) ? -1 : 1;
    }

    private void LateUpdate()
    {
        HandleCameraInput();
    }

    private void HandleCameraInput()
    {
        // Create the look input vector for the camera
        float mouseLookAxisUp = RewiredPlayer.GetAxisRaw("LookUp");
        float mouseLookAxisRight = RewiredPlayer.GetAxisRaw("LookRight");
        Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

        // Prevent moving the camera while the cursor isn't locked
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            lookInputVector = Vector3.zero;
        }

        // Apply inputs to the camera
        orbitCamera.UpdateWithInput(Time.deltaTime, _zoom, lookInputVector);
    }

    private void HandleFiringInputs()
    {
        base.isAiming = RewiredPlayer.GetButton("Aim");
        base.isFiring = RewiredPlayer.GetButton("Fire");
        base.reloadedThisFrame = RewiredPlayer.GetButtonDown("Reload");

        if (base.isAiming && !_oldAim)
        {
            ToggleZoom();
        }
        if (!base.isAiming && _oldAim)
        {
            ToggleZoom();
        }

        _oldAim = base.isAiming;
    }

    private void SetOrientation(Vector3 cameraPlanarDirection)
    {
        // Allows setting if the player should look in the direction of the
        // camera, e.g aiming, or in the direction of movement for navigation.
        switch (orientationMethod)
        {
            case OrientationMethod.TowardsCamera:
                lookInputVector = cameraPlanarDirection;
                break;
            case OrientationMethod.TowardsMovement:
                lookInputVector = moveInputVector.normalized;
                break;
        }
    }

    public void CallOrientationTimer()
    {
        StartCoroutine(OrientationTimer(secondsToLockShootingOrientation));
    }
    
    public override void Damage(float damage) {
        OnDamage?.Invoke();
        base.Damage(damage);
        // Debug.Log(damage);
    }

    public override void Kill()
    {
        base.Kill();
        OnPlayerDeath?.Invoke();
        // Snake? Snaaaaaaaaaaaaaaaaaaaaaaaaaaake!
        GameManager.PlayerDeath?.Invoke(this);
    }
}

