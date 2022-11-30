using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Flyer : SAi
{

    [Tooltip("Each navmesh corner, when used, is increased by a random value between the min and max corner height.")]
    [SerializeField] private float minCornerHeight = 3f;
    [SerializeField] private float maxCornerHeight = 8f;
    private Vector3 _currentSeekPoint;
    
    
    protected override void StartCharacter()
    {
        base.StartCharacter();
        if (!targetChar) targetChar = FindObjectOfType<Kuze>();
    }

    // public override void SetDestination(Vector3 destination)
    // {
    //     var r = new Ray(transform.position, Vector3.down);
    //     if (!Physics.Raycast(r, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default"))) return;
    //     NavMeshHit navMeshHit;
    //     if (!NavMesh.SamplePosition(hit.point, out navMeshHit, 5f, NavMesh.AllAreas))
    //     {
    //         pathStatus = PathStatus.Failed;
    //         return;
    //     }
    //     var valid = NavMesh.CalculatePath(navMeshHit.position, destination, NavMesh.AllAreas, NavMeshPath);
    //     if (!valid)
    //     {
    //         pathStatus = PathStatus.Failed;
    //         return;
    //     }
    //     Debug.Log($"{gameObject.name}'s path is pending. Assigned new path.");
    //     pathStatus = PathStatus.Pending;
    //     CurrCornersIndex = 0;
    // }

    // protected override void LookAtTargetCharacter()
    // {
    //     if (!targetChar) return;
    //     Inputs.LookVector = (targetChar.Collider.bounds.center - Collider.bounds.center).normalized;
    // }

    public override void SetDestination(Vector3 destination)
    {
        Debug.Log($"Setting destination {destination}");
        var checkPoint =
            Physics.Raycast(motor.Capsule.bounds.center, Vector3.down, out var rayCastHit, Mathf.Infinity,
                1 << LayerMask.NameToLayer("Default"))
                ? rayCastHit.point
                : motor.Capsule.bounds.center;
        if (NavMesh.SamplePosition(checkPoint, out var navMeshHit,
                maxCornerHeight + motor.Capsule.height, NavMesh.AllAreas))
        {
            Debug.Log($"Found pos on navmesh {navMeshHit.position}. Drawn in green.");
            Debug.DrawLine(motor.Capsule.bounds.center, navMeshHit.position, Color.green, 10f);
            var valid = NavMesh.CalculatePath(navMeshHit.position, destination, NavMesh.AllAreas, NavMeshPath);
            if (valid)
            {
                Debug.Log($"NavMeshPath found from navmeshhit to destination.");
                pathStatus = PathStatus.Pending;
                CurrCornersIndex = 0;
            }
            else
            {
                Debug.Log("Failing navmesh status");
                pathStatus = PathStatus.Failed;
            }
        }
        else
        {
            Debug.Log("Failing navmesh status");
            pathStatus = PathStatus.Failed;
        }
    }

    protected override void MoveAgent()
    {
        if (pathStatus != PathStatus.Pending || NavMeshPath.corners.Length <= 0) return;
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
        //ONLY listen for x, z positions for the flying enemy.
        var bounds = motor.Capsule.bounds;
        var flattened = new Vector3(bounds.center.x, NavMeshPath.corners[CurrCornersIndex].y, bounds.center.z);
        if (Vector3.Distance(flattened, NavMeshPath.corners[CurrCornersIndex]) <= stopDistance)
        {
            if (CurrCornersIndex == NavMeshPath.corners.Length - 1)
            {
                pathStatus = PathStatus.Reached;
                return;
            }
            CurrCornersIndex++;
            var heightChangeTuple = CalculateCornerHeight(NavMeshPath.corners[CurrCornersIndex]);
            if (heightChangeTuple.Item2)
            {
                _currentSeekPoint = heightChangeTuple.Item1;
                Debug.Log($"Corner {NavMeshPath.corners[CurrCornersIndex]} was translated vertically into {_currentSeekPoint}");
            }
            else
            {
                Debug.Log($"{gameObject.name} path failed");
                pathStatus = PathStatus.Failed;
                return;
            }
        }
        
        Debug.DrawLine(motor.Capsule.bounds.center, _currentSeekPoint, Color.red);
        Inputs.MoveVector = (_currentSeekPoint - motor.Capsule.bounds.center).normalized;
        if (lookAtBehavior == LookAtBehavior.AtPath)
            Inputs.LookVector = new Vector3(Inputs.MoveVector.x, 0, Inputs.MoveVector.z);
        //Ignores Y so the character doesn't look at the ground in a silly/goofy way.
    }

    public bool HasReachedDestination(float threshold, Vector3 location)
    {
        var bounds = motor.Capsule.bounds;
        var flattened = new Vector3(bounds.center.x, location.y, bounds.center.z);
        return Vector3.Distance(flattened, location) <= threshold;
    }

    (Vector3, bool) CalculateCornerHeight(Vector3 origCorner)
    {
        //First, try maintaining current height. No need to change if it's not needed.
        var bounds = motor.Capsule.bounds;
        var newSeekPoint = origCorner;
        // var newSeekPoint = new Vector3(origCorner.x, bounds.center.y, origCorner.z);
        // if (IsClearLineToPoint(newSeekPoint))
        // {
        //     return (newSeekPoint, true);
        // }

        //If we need to change the height to find something usable, try a random new height i times.
        for (var i = 0; i < 5; i++)
        {
            // newSeekPoint.y = origCorner.y + Random.Range(minCornerHeight, maxCornerHeight);
            var thisTest = new Vector3(newSeekPoint.x, newSeekPoint.y + Random.Range(minCornerHeight, maxCornerHeight),
                newSeekPoint.z);
            if (IsClearLineToPoint(thisTest))
            {
                return (thisTest, true);
            }
        }
        
        return (origCorner, false);
    }

    private bool IsClearLineToPoint(Vector3 point)
    {
        var bounds = motor.Capsule.bounds;
        var direction = point - bounds.center;
        var radius = motor.Capsule.radius;
        var bottomCenter =
            transform.TransformPoint(motor.Capsule.center - Vector3.up * (motor.Capsule.height / 2 - radius));
        var topCenter = transform.TransformPoint(motor.Capsule.center + Vector3.up * (motor.Capsule.height / 2 - radius));
        
        Debug.DrawRay(bottomCenter, direction, Color.blue, 5f);
        Debug.DrawRay(topCenter, direction, Color.blue, 5f);
        
        return !Physics.CapsuleCast(bottomCenter, topCenter, radius, direction,
            direction.magnitude, 1 << LayerMask.NameToLayer("Default"));
    }

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (motor.GroundingStatus.FoundAnyGround) motor.ForceUnground();
        
        var targetMovementVelocity = moveInputVector * maxStableMoveSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
            1 - Mathf.Exp(-stableMovementSharpness * deltaTime));
        
        // Apply Drag
        currentVelocity *= 1f / (1f + drag * deltaTime);
    }
}
