using System;
using BehaviorTreeSystem;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public abstract class ConditionalNode : DecoratorNode
{
    protected abstract bool Condition();

    protected override State Execution() => Condition() ? child.Execute() : State.Failure;
    
}