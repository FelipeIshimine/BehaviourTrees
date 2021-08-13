using TheKiwiCoder;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentRandomPosition : ActionNode
{
    private NavMeshAgent _navMeshAgent;
    private Transform _transform;
    
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
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius;
        randomDirection += _transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRadius, 1))
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