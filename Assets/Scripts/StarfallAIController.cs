using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StarfallAIController : MonoBehaviour
{

    public StarfallCharacterController character;
    public Transform target;
    public float satisfiedRange;
    
    private KinematicSeek _kinematicSeek;
    private NavMeshPath _navMeshPath;
    private int i = 0;
    private Vector3 _localTarget;
    private bool _seeking = true;

    void Awake()
    {
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _navMeshPath = new NavMeshPath();
        NavMesh.CalculatePath(character.transform.position, target.position, NavMesh.AllAreas, _navMeshPath);
        _kinematicSeek = new KinematicSeek(character, _navMeshPath.corners[i]);
        _localTarget = _navMeshPath.corners[i];
    }

    // Update is called once per frame
    void Update()
    {
        if ((_localTarget - character.transform.position).magnitude <= satisfiedRange)
        {
            //We are close enough to the point to move on to the next point.
            if (i + 1 < _navMeshPath.corners.Length)
            {
                i++;
                Debug.Log("incremented the localtarget");
                _localTarget = _navMeshPath.corners[i];
                _kinematicSeek = new KinematicSeek(character, _navMeshPath.corners[i]);
            }
            else
            {
                _seeking = false;
                Debug.Log("Path is done.");
            }
        }
        
        for (int j = 0; j < _navMeshPath.corners.Length - 1; j++) {
            Debug.DrawLine(_navMeshPath.corners[j], _navMeshPath.corners[j + 1], Color.yellow); 
        }
        
        StarfallCharacterController.StarfallAICharacterInputs inputs =
            new StarfallCharacterController.StarfallAICharacterInputs();

        if (_seeking)
        {
            var seek = _kinematicSeek.GetSteering();
            inputs.MoveVector = seek.Velocity;
            inputs.LookVector = new Vector3(seek.Rotation.x, 0, seek.Rotation.z);
        }

        character.SetInputs(ref inputs);
    }
}
