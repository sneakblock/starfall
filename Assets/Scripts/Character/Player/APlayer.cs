using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KinematicCharacterController.Examples;
using KinematicCharacterController;
using UnityEngine.Events;


public abstract class APlayer : SCharacter
{
    [Header("Orbit Camera")]  public ExampleCharacterCamera orbitCamera;

    [Header("Firing Logic")] public LayerMask playerFiringLayerMask;

    public UnityEvent onPlayerAimDown = new UnityEvent();
    public UnityEvent onPlayerAimUp = new UnityEvent();
    public UnityEvent onPlayerFire = new UnityEvent();
    public UnityEvent onPlayerReloadStart = new UnityEvent();
    public UnityEvent onPlayerReloadComplete = new UnityEvent();

    protected const string HorizontalInput = "Horizontal";
    protected const string VerticalInput = "Vertical";
    private bool _oldAim = false;
    private bool _oldFire = false;

    // Camera exists for APlayer and not SCharacter because Enemy is an
    // SCharacter and they do not deserve a camera.
    protected Camera cam;

    private int _zoom = 1;


    protected override void StartCharacter()
    {
        cam = orbitCamera.Camera;

        orbitCamera.SetFollowTransform(base.orbitPoint);

        //Subscribe ToggleZoom to the OnPlayerAimDown event
        onPlayerAimDown.AddListener(ToggleZoom);
        onPlayerAimUp.AddListener(ToggleZoom);

        //Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        

        // Ignore the character's collider(s) for camera obstruction checks
        orbitCamera.IgnoredColliders = base.GetComponentsInChildren<Collider>().ToList();


        StartPlayer();
    }

    protected override void UpdateCharacter()
    {
        UpdatePlayer();
    }

    protected override void HandleInputs()
    {
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

        float moveAxisForward = Input.GetAxisRaw(VerticalInput);
        float moveAxisRight = Input.GetAxisRaw(HorizontalInput);

        //Just sets the desired move vector, received from Player, and clamps it's magnitude to not exceed 1.
        Vector3 inputVector = Vector3.ClampMagnitude(new Vector3(moveAxisRight, 0f, moveAxisForward), 1f);

        //Sets this local character's move and look inputs to what we've found.
        base.moveInputVector = cameraPlanarRotation * inputVector;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            timeSinceJumpRequested = 0f;
            jumpRequested = true;
        }

        //Update the screen center point
        var screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(screenCenterPoint);
        bool rayhit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, playerFiringLayerMask);
        var targetPoint =  rayhit ? hit.point : ray.GetPoint(1000f);

        base.target = targetPoint;

        HandleFiringInputs();

        // Calls player specific inputs (each character can have extra
        // player inputs).
        HandlePlayerInputs();

    }

    protected abstract void HandlePlayerInputs();
    protected abstract void StartPlayer();
    protected abstract void UpdatePlayer();

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
        float mouseLookAxisUp = Input.GetAxisRaw("Mouse Y");
        float mouseLookAxisRight = Input.GetAxisRaw("Mouse X");
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
        base.isAiming = Input.GetMouseButton(1);
        base.isFiring = Input.GetMouseButton(0);
        base.reloadedThisFrame = Input.GetKeyDown(KeyCode.R);

        if (base.isAiming && !_oldAim)
        {
            onPlayerAimDown.Invoke();
        }
        if (!base.isAiming && _oldAim)
        {
            onPlayerAimUp.Invoke();
        }

        _oldAim = base.isAiming;
        _oldFire = base.isFiring;
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
}

