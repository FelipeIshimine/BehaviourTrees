using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class NavAgentRandomEvadePosition : ActionNode
{
    private NavMeshAgent _navMeshAgent;
    private Transform _transform;
    
    public float angleVariation = 5;
    public float moveRadius = 10;
    public float targetReachedThreshold = 1;
    
    protected override void Initialization()
    {
        GetComponent(out _navMeshAgent);
        GetComponent(out _transform);
    }

    protected override void OnStart()
    {
        FindNextDestination();
    }

    protected override void OnStop() {
    }

    protected override State Execution()
    {
        if (!TargetReached()) return State.Running;
        return FindNextDestination() ? State.Success : State.Failure;
    }
    
    private bool FindNextDestination()
    {
        Transform evadeTarget = PlayerAgent.Monster.transform;
        
        Vector2 myPosition = new Vector2(_transform.position.x, _transform.position.z);
        Vector2 evadeTargetPosition = new Vector2(evadeTarget.position.x, evadeTarget.position.z);

        Vector2 newDirection = (myPosition - evadeTargetPosition).normalized;

        float angle = AsAngle(newDirection) + Random.Range(-angleVariation,angleVariation);

        newDirection = AsVector(angle);

        Vector3 endDirection = new Vector3(newDirection.x, 0, newDirection.y) * moveRadius;
        
        if (NavMesh.SamplePosition(endDirection, out NavMeshHit hit, moveRadius, 1))
        {
            _navMeshAgent.destination = hit.position;
            return true;
        }
        return false;
    }
    
    private static float AsAngle(Vector2 source)
    {
        Vector2 normalized = source.normalized;
        float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
        return angle;
    }
    
    private static Vector2 AsVector(float degree)=> RadianToVector2(degree * Mathf.Deg2Rad);
    
    private static Vector2 RadianToVector2(float radian)=> new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

    private bool TargetReached()
    {
        float distance = Vector3.Distance(_transform.position, _navMeshAgent.destination);
        return distance < targetReachedThreshold;
    }

}