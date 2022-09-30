using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.AI;

public class SAi : SCharacter
{
    protected BehaviourTreeRunner TreeRunner;
    protected BehaviourTree BehaviourTree;

    public LookAtBehavior lookAtBehavior = LookAtBehavior.AtPath;

    public NavMeshPath NavMeshPath;
    public PathStatus pathStatus = PathStatus.None;
    public float stopDistance = .1f;

    public Collider coll;

    protected SCharacterInputs Inputs;
    private int _currCornersIndex = 0;

    [Header("Debug")] public bool debug = false;

    public enum LookAtBehavior
    {
        AtPath,
        AtPoint
    }

    public enum PathStatus
    {
        Pending,
        Reached,
        Failed,
        None
    }

    protected override void StartCharacter()
    {
        NavMeshPath = new NavMeshPath();
        coll = GetComponent<Collider>();
        TreeRunner = GetComponent<BehaviourTreeRunner>();
        if (!TreeRunner)
        {
            Debug.LogWarning($"Entity {gameObject} has no attached behaviorTreeRunner.");
        }
    }
    
    protected override void HandleInputs()
    {
        InitInputs();
        MoveAgent();
    }

    protected override void UpdateCharacter()
    {
        AssignInputs(ref Inputs);
    }
    
    public bool SetDestination(Vector3 destination)
    {
        var valid = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, NavMeshPath);
        if (valid)
        {
            pathStatus = PathStatus.Pending;
            _currCornersIndex = 0;
        }
        return valid;
    }

    private void InitInputs()
    {
        Inputs = new SCharacterInputs();
    }

    //TODO: Add some logic to fail or abandon a path if the agent gets stuck.
    /// <summary>
    /// MoveAgent will, if a path is pending, follow each corner of the path until it reaches the end of the corners. When it reaches
    /// the end of the path, it will set pathPending to false and return.
    /// </summary>
    protected virtual void MoveAgent()
    {
        if (pathStatus != PathStatus.Pending) return;
        //Nowhere to move!

        if (debug)
        {
            for (int i = 0; i < NavMeshPath.corners.Length - 1; i++)
            {
                Debug.DrawLine(NavMeshPath.corners[i], NavMeshPath.corners[i + 1], Color.yellow);
            }
        }
        //Draw the path for the SAi.

        //We use a point on the collider because using the transform may result in an unreachable point at low stopDistances
        if (Vector3.Distance((coll.ClosestPoint(NavMeshPath.corners[_currCornersIndex])), NavMeshPath.corners[_currCornersIndex]) <= stopDistance)
        {
            if (_currCornersIndex == NavMeshPath.corners.Length - 1)
            {
                pathStatus = PathStatus.Reached;
                return;
            }
            _currCornersIndex++;
        }
        
        Inputs.MoveVector = (NavMeshPath.corners[_currCornersIndex] - transform.position).normalized;
        if (lookAtBehavior == LookAtBehavior.AtPath)
            Inputs.LookVector = new Vector3(Inputs.MoveVector.x, 0, Inputs.MoveVector.z);
        //Ignores Y so the character doesn't look at the ground in a silly/goofy way.
    }

}
