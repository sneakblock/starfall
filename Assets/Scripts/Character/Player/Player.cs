using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KinematicCharacterController.Examples;
using KinematicCharacterController;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
            public ExampleCharacterCamera orbitCamera;
            public Transform cameraFollowPoint;
            
            [SerializeField]
            private SCharacterController _sCharacter;
            
            [Header("Player Firing Behavior")]
            public LayerMask playerFiringLayerMask;
            
            //Player Events
            public UnityEvent onPlayerAimDown = new UnityEvent();
            public UnityEvent onPlayerAimUp = new UnityEvent();
            public UnityEvent onPlayerFire = new UnityEvent();
            public UnityEvent onPlayerReloadStart = new UnityEvent();
            public UnityEvent onPlayerReloadComplete = new UnityEvent();

            private const string HorizontalInput = "Horizontal";
            private const string VerticalInput = "Vertical";
            private bool _oldAim = false;
            private bool _oldFire = false;
            private int _zoom = 1;
            private Camera _cam;
            
            //TEMPORARY, HACKY ANIMATION CONTROLLER FOR PITCH
            //TODO: FIX THIS TRASH
            public Animator anim;

            public SCharacterController GetCharacter()
            {
                return _sCharacter;
            }

            public void SetCharacter(SCharacterController c)
            {
                _sCharacter = c;
                anim = c.GetComponentInChildren<Animator>();
            }

            private void Start()
            {
                //Get camera component
                _cam = orbitCamera.Camera;
                
                //Subscribe ToggleZoom to the OnPlayerAimDown event
                onPlayerAimDown.AddListener(ToggleZoom);
                onPlayerAimUp.AddListener(ToggleZoom);
                
                //Assign whatever character we have the label and layer of player, and all children of that character.
                var o = _sCharacter.gameObject;
                foreach (Transform t in o.GetComponentsInChildren<Transform>())
                {
                    var gameObject1 = t.gameObject;
                    gameObject1.layer = 6;
                    gameObject1.tag = "Player";
                }
                
                //Lock the cursor
                Cursor.lockState = CursorLockMode.Locked;
    
                // Tell camera to follow transform
                orbitCamera.SetFollowTransform(cameraFollowPoint);
    
                // Ignore the character's collider(s) for camera obstruction checks
                orbitCamera.IgnoredColliders = _sCharacter.GetComponentsInChildren<Collider>().ToList();
            }
    
            private void Update()
            {
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

                // Apply inputs to the camera
                orbitCamera.UpdateWithInput(Time.deltaTime, _zoom, lookInputVector);
            }

            private void HandleCharacterInput()
            {
                PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();
                
                // Build the CharacterInputs struct
                characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
                characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
                characterInputs.CameraRotation = orbitCamera.Transform.rotation;
                characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
                characterInputs.Primary = Input.GetMouseButton(0);
                characterInputs.Aim = Input.GetMouseButton(1);
                characterInputs.Reload = Input.GetKeyDown(KeyCode.R);
                //Update the screen center point
                var screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Ray ray = _cam.ScreenPointToRay(screenCenterPoint);
                var targetPoint = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, playerFiringLayerMask) ? hit.point : ray.GetPoint(1000f);
                characterInputs.Target = targetPoint;
                
                switch (characterInputs.Aim)
                {
                    case true when !_oldAim:
                        onPlayerAimDown.Invoke();
                        break;
                    case false when _oldAim:
                        onPlayerAimUp.Invoke();
                        break;
                }

                _oldAim = characterInputs.Aim;
                _oldFire = characterInputs.Primary;

                bool isMoving = characterInputs.MoveAxisForward != 0 || characterInputs.MoveAxisRight != 0;
                if (anim != null)
                {
                    anim.SetBool("isMoving", isMoving);
                    anim.SetBool("isFiring", characterInputs.Primary);
                }
                
                // Apply inputs to character
                _sCharacter.SetInputs(ref characterInputs);
            }
}
