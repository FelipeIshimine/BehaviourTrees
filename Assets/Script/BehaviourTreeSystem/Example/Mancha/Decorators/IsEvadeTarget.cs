using System;
using BehaviorTreeSystem;
using BehaviourTreeSystem.Runtime.Core;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class IsEvadeTarget : ConditionalNode
{
    private IGetNavAgentEvadeTarget _getNavAgentEvadeTarget;
    private Transform _transform;

    protected override void Initialization()
    {
        GetComponent(out _transform);
        GetComponent(out _getNavAgentEvadeTarget);
    }

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }
    
    protected override bool Condition() => _getNavAgentEvadeTarget.Get() == _transform;
}