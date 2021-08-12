using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorTreeSystem;
using BehaviorTreeSystem.Composites;
using BehaviorTreeSystem.Decorators;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAgent_BehaviorTreeAlpha : MonoBehaviour
{
    public PlayerAgent agent;
    private NavMeshAgent _navMeshAgent;

    private BehaviorTree _tree;
    private string lastNodeExecuted;

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
        _tree = new BehaviorTree();
        _tree.OnNodeExecution += OnNodeExecution;
        var myTransform = transform;
        
        RandomWalkNavMeshNode randomWalkNavMeshNode = new RandomWalkNavMeshNode(_tree, myTransform, _navMeshAgent, agent.moveRadius, agent.targetReachedDistanceThreshold);
        EvadeWalkNavMeshNode evadeWalkNavMeshNode = new EvadeWalkNavMeshNode(_tree, myTransform, GetMonster, _navMeshAgent, agent.moveRadius, agent.targetReachedDistanceThreshold, agent.angleVariation);

        InsideRadiusNode evadeOrWalkNode = new InsideRadiusNode(_tree, myTransform, GetMonster, agent.evadeRadius,
            evadeWalkNavMeshNode, randomWalkNavMeshNode);

        ChaseInRadiusOrClosest chaseNode = new ChaseInRadiusOrClosest(_tree,GetAgentsTransform, _navMeshAgent,  agent.touchDistance, agent.evadeRadius, x => x.GetComponent<PlayerAgent>().SetAsMonster());

        WaitNode waitNode = new WaitNode(_tree,agent.waitTime);
        
        SequencerNode chaseSequence = new SequencerNode(_tree, waitNode, chaseNode);
        
        IfNode ifNode = new IfNode(_tree, IsTheMonster, chaseSequence, evadeOrWalkNode);

        Repeater repeater = new Repeater(_tree, ifNode);
        
        _tree.OverrideRoot(repeater);   
    }
    
    
    private void OnNodeExecution(BehaviourNode obj)
    {
        lastNodeExecuted = obj.ToString();
    }

    private bool IsTheMonster()
    {
        bool value = PlayerAgent.Monster != null && PlayerAgent.Monster.transform == transform;
        Debug.Log($"{name} {(value?"<color=red>is</color>":"is not")} the Monster");
        return value;
    }
    
    private Transform GetMonster() => PlayerAgent.Monster.transform;
    
    private List<Transform> GetAgentsTransform() => PlayerAgent.Victims;

    public void FixedUpdate()
    {
        _tree.Tick();
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, $"<color=green>{lastNodeExecuted}</color>", new GUIStyle(){richText = true});
#endif
    }
}
