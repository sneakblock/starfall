using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KinematicCharacterController.Examples;
using KinematicCharacterController;
using UnityEngine.Events;

public class StarfallPlayer : MonoBehaviour
{
            public ExampleCharacterCamera orbitCamera;
            public Transform cameraFollowPoint;
            public StarfallCharacterController character;

            [Header("Player Aim Behavior")]
            public UnityEvent onAimDown = new UnityEvent();
            public UnityEvent onAimUp = new UnityEvent();

            [Header("Player Firing Behavior")] 
            public UnityEvent onFireDown = new UnityEvent();
            public UnityEvent onFireUp = new UnityEvent();
            public LayerMask firingLayerMask;

            [Header("Debugging")] public bool debug;
            
            private const string HorizontalInput = "Horizontal";
            private const string VerticalInput = "Vertical";
            private bool _oldAim = false;
            private bool _oldFire = false;
            private int _zoom = 1;
            private Camera _cam;

            private void Awake()
            {
                onAimDown.AddListener(ToggleZoom);
                onAimUp.AddListener(ToggleZoom);
                _cam = orbitCamera.Camera;
            }

            private void Start()
            {
                Cursor.lockState = CursorLockMode.Locked;
    
                // Tell camera to follow transform
                orbitCamera.SetFollowTransform(cameraFollowPoint);
    
                // Ignore the character's collider(s) for camera obstruction checks
                orbitCamera.IgnoredColliders = character.GetComponentsInChildren<Collider>().ToList();
            }
    
            private void Update()
            {
                //What is this for?
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }

                HandleCharacterInput();
            }
    
            private void LateUpdate()
            {
                HandleCameraInput();
            }

            private void ToggleZoom()
            {
                _zoom = (_zoom == 1) ? -1 : 1;
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
    
                // Input for zooming the camera (disabled in WebGL because it can cause problems)
        //         float scrollInput = -Input.GetAxis("Mouse ScrollWheel");
        // #if UNITY_WEBGL
        //         scrollInput = 0f;
        // #endif
    
                // Apply inputs to the camera
                orbitCamera.UpdateWithInput(Time.deltaTime, _zoom, lookInputVector);
    
                // Handle toggling zoom level
                // if (Input.GetMouseButtonDown(1))
                // {
                //     orbitCamera.TargetDistance = (orbitCamera.TargetDistance == 0f) ? orbitCamera.DefaultDistance : 0f;
                // }
            }

            private void HandleCharacterInput()
            {
                StarfallCharacterController.StarfallPlayerCharacterInputs characterInputs = new StarfallCharacterController.StarfallPlayerCharacterInputs();
                
                // Build the CharacterInputs struct
                characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
                characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
                characterInputs.CameraRotation = orbitCamera.Transform.rotation;
                characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
                characterInputs.Primary = Input.GetMouseButton(0);
                characterInputs.Aim = Input.GetMouseButton(1);
                //Update the screen center point
                var screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Ray ray = _cam.ScreenPointToRay(screenCenterPoint);
                var targetPoint = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, firingLayerMask) ? hit.point : ray.GetPoint(1000f);
                characterInputs.Target = targetPoint;

                if (debug)
                {
                    Debug.Log("Move axis right:" + characterInputs.MoveAxisRight);
                    Debug.Log("Move axis forward: " + characterInputs.MoveAxisForward);
                }
                
                
                
                switch (characterInputs.Aim)
                {
                    case true when !_oldAim:
                        onAimDown.Invoke();
                        break;
                    case false when _oldAim:
                        onAimUp.Invoke();
                        break;
                }
                
                switch (characterInputs.Primary)
                {
                    case true when !_oldFire:
                        onFireDown.Invoke();
                        break;
                    case false when _oldFire:
                        onFireUp.Invoke();
                        break;
                }
                
                _oldAim = characterInputs.Aim;
                _oldFire = characterInputs.Primary;

                // Apply inputs to character
                character.SetInputs(ref characterInputs);

            }
}
