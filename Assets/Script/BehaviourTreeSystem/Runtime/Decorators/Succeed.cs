using System.Collections;
using System.Collections.Generic;
using BehaviourTreeSystem.Runtime.Core;
using UnityEngine;

public class Succeed : DecoratorNode
{
    protected override void Initialization()
    {
    }

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State Execution()
    {
        var state = child.Execute();
        if (state == State.Failure)
        {
            return State.Success;
        }

        return state;
    }
}
