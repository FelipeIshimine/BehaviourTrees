using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class Breakpoint : ActionNode
{
    protected override void Initialization()
    {
    }

    protected override void OnStart() {
        Debug.Log("Trigging Breakpoint");
        Debug.Break();
    }

    protected override void OnStop() {
    }

    protected override State Execution() {
        return State.Success;
    }
}
