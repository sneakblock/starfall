using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StarfallAIController : MonoBehaviour
{
    [SerializeField]
    public StarfallCharacterController character { get; private set; }

    public Transform test;

    private KinematicSeek _kinematicSeek;
    private NavMeshPath _path;
    //Local target on the path we are currently seeking
    private Vector3 _localTarget;
    //Current ultimate target meant to be reached by traveling the path.
    private Vector3 _goToTarget;
    private StarfallCharacterController.StarfallAICharacterInputs _inputs;
    private bool _hasPath = false;
    private float _range = 2f;
    private int _currPathIndex = 0;
    private bool _lookAtNavTarget = true;

    private void Start()
    {
        GoTo(test.position);
    }

    //Wipes _inputs clean
    public void InitInputs()
    {
        _inputs = new StarfallCharacterController.StarfallAICharacterInputs();
    }

    //Feeds the inputs the controller's character
    public void AssignInputsToCharacter()
    {
        character.SetInputs(ref _inputs);
    }

    public bool GoTo(Vector3 target)
    {
        if (_hasPath)
        {
            var dist = Vector3.Distance(_goToTarget, target);
            //The given target is the same as the current target, or so close to it that it need not
            //run another A*.
            if (dist <= _range) return true;
        }
        _path = new NavMeshPath();
        if (NavMesh.CalculatePath(character.transform.position, target, NavMesh.AllAreas, _path))
        {
            _hasPath = true;
            _currPathIndex = 0;
            _localTarget = _path.corners[0];
            return true;
        }
        else
        {
            Debug.Log("Could not find a valid path to this target.");
            return false;
        }
    }

    public void StopGoing()
    {
        _hasPath = false;
    }

    void Update()
    {
        InitInputs();
        if (_hasPath)
        {
            for (int i = 0; i < _path.corners.Length - 1; i++)
            {
                Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.yellow);
            }
            FollowPath();
        }
        AssignInputsToCharacter();
    }

    private void FollowPath()
    {
        //Look ahead to the next point on the path, assuming we are not on the final point of the path.
        if (Vector3.Distance(character.transform.position, _localTarget) <= _range)
        {
            if (_currPathIndex == _path.corners.Length - 1)
            {
                Debug.Log("Arrived.");
                StopGoing();
                return;
            }
            Debug.Log("Incremented localTarget");
            _currPathIndex++;
            _localTarget = _path.corners[_currPathIndex];
        }
        var steering = new KinematicSeek(character, _localTarget).GetSteering();
        _inputs.MoveVector = steering.Velocity;
        if (_lookAtNavTarget) _inputs.LookVector = steering.Rotation;
    }

    public bool ReachedTarget()
    {
        if (_hasPath)
        {
            return (_path.corners[^1] - character.transform.position).magnitude <= _range;
        }

        return false;
    }

    public bool HasPath()
    {
        return _hasPath;
    }

}
