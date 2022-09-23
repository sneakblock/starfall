using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [SerializeField] public SCharacterController character;
    public EnemyData enemyData;

    private KinematicSeek _kinematicSeek;
    private NavMeshPath _path;
    //Local target on the path we are currently seeking
    private Vector3 _localTarget;
    //Current ultimate target meant to be reached by traveling the path.
    private Vector3 _goToTarget;
    private AICharacterInputs _inputs;
    private bool _hasPath = false;
    private float _range = 2f;
    private int _currPathIndex = 0;
    private SCharacter _lookAtSCharacter;
    private Vector3 _lookAtPoint;
    public LookAtBehavior _lookAtBehavior;

    public enum LookAtBehavior
    {
        Path,
        Character,
        Point
    }

    private void Awake()
    {
        character = GetComponent<SCharacterController>();
    }

    private void Start()
    {
        //By default, entities will look where they are going.
        _lookAtBehavior = LookAtBehavior.Path;
    }

    //Wipes _inputs clean
    public AICharacterInputs InitInputs()
    {
        _inputs = new AICharacterInputs();
        return _inputs;
    }

    //Feeds the inputs the controller's character
    public void AssignInputsToCharacter(AICharacterInputs inputs)
    {
        _inputs = inputs;
        // Debug.Log("Assigned move vector to entity" + _inputs.MoveVector);
        character.SetInputs(ref _inputs);
    }
    
    public bool SetPath(Vector3 target)
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

    public AICharacterInputs FollowPath(AICharacterInputs inputs)
    {
        for (int i = 0; i < _path.corners.Length - 1; i++)
        {
            Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.yellow);
        }
        //Look ahead to the next point on the path, assuming we are not on the final point of the path.
        if (Vector3.Distance(character.transform.position, _localTarget) <= _range)
        {
            if (_currPathIndex == _path.corners.Length - 1)
            {
                Debug.Log("Arrived.");
                StopGoing();
                return inputs;
            }
            Debug.Log("Incremented localTarget");
            _currPathIndex++;
            _localTarget = _path.corners[_currPathIndex];
        }
        var steering = new KinematicSeek(character, _localTarget).GetSteering();
        inputs.MoveVector = steering.Velocity;
        if (_lookAtBehavior == LookAtBehavior.Path) inputs.LookVector = steering.Rotation;
        return inputs;
    }
    
    //Aiming method. This sets "aim" in the given inputs to true. 
    public AICharacterInputs Aim(AICharacterInputs inputs)
    {
        inputs.Aim = true;
        return inputs;
    }

    public AICharacterInputs Fire(AICharacterInputs inputs)
    {
        inputs.Primary = true;
        return inputs;
    }

    //TODO: Distort this value based on the enemy's accuracy
    public AICharacterInputs SetTarget(AICharacterInputs inputs, Vector3 goalTarget)
    {
        inputs.Target = goalTarget;
        return inputs;
    }

    public AICharacterInputs SetLookAtInputs(AICharacterInputs inputs)
    {
        switch (_lookAtBehavior)
        {
            case LookAtBehavior.Path:
                // Does nothing
                break;
            case LookAtBehavior.Character:
                var temp0 = (_lookAtSCharacter.transform.position - character.transform.position).normalized;
                inputs.LookVector = new Vector3(temp0.x, 0, temp0.z);
                break;
            case LookAtBehavior.Point:
                var temp1 = (_lookAtPoint - character.transform.position).normalized;
                inputs.LookVector = new Vector3(temp1.x, 0, temp1.z);
                break;
        }

        return inputs;
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

    /// <summary>
    /// A public method that determines what the enemy looks at. As of now, this can either be a character or null.
    /// </summary>
    /// <param name="c">
    /// The character for the AI character to look towards. If this is null, the character will look at it's path as it traverses it.
    /// </param>
    public void SetLookAtCharacter(SCharacter c)
    {
        if (c == null)
        {
            Debug.Log("Character cannot be null.");
            return;
        }
        _lookAtSCharacter = c;
        _lookAtBehavior = LookAtBehavior.Character;
    }

    public void SetLookAtCharacter()
    {
        _lookAtBehavior = LookAtBehavior.Character;
    }

    public void SetLookAtPath()
    {
        _lookAtBehavior = LookAtBehavior.Path;
    }

    public void SetLookAtPoint(Vector3 p)
    {
        _lookAtBehavior = LookAtBehavior.Point;
        _lookAtPoint = p;
    }
    
    public void SetLookAtPoint()
    {
        _lookAtBehavior = LookAtBehavior.Point;
    }

    public bool IsReloading()
    {
        return character.GetRangedWeapon().GetReloading();
    }

}
