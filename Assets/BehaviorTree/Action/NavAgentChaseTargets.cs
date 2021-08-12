using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentChaseTargets : ActionNode
{
    public float reachedDistance = 1.1f;

    public float shortCircuitChaseDistance = 3;
    
    private NavMeshAgent _navMeshAgent;
    private IGetNavAgentTargets _iGetNavAgentTargets;
    
    protected override void Initialization()
    {
        GetComponent(out _navMeshAgent);
        GetComponent(out _iGetNavAgentTargets);
    }

    protected override void OnStart() { }

    protected override void OnStop() { }

    protected override State Execution()
    {
        Transform closest = GetClosest(_iGetNavAgentTargets.Get());

        if (closest == null)
            return State.Failure;

        float distance = Vector3.Distance(closest.transform.position, _navMeshAgent.transform.position);
        Debug.Log($"{_navMeshAgent.gameObject.name}:{distance}");
     
        if (distance < reachedDistance) return State.Success;
        _navMeshAgent.destination = closest.position;
        
        return State.Running;
    }
    

    private Transform GetClosest(IReadOnlyList<Transform> getAgents)
    {
        Transform transform = _navMeshAgent.transform;
        float sqrDistance = float.MaxValue;
        Transform closest = null;
        foreach (Transform agent in getAgents)
        {
            float currentSqrDistance = (transform.position - agent.position).sqrMagnitude;
            if (currentSqrDistance < sqrDistance)
            {
                sqrDistance = currentSqrDistance;
                closest = agent.transform;
                if (sqrDistance < reachedDistance || sqrDistance < shortCircuitChaseDistance)
                    break;
            }
        }
        return closest;
    }
    
    
  
}