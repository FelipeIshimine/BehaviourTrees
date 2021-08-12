using UnityEngine;

public class IsEvadeTargetInsideRadius : ConditionalNode
{
    public float radius = 4;
    private IGetNavAgentEvadeTarget _getNavAgentEvadeTarget;
    private Transform _transform;

    protected override void Initialization()
    {
        GetComponent(out _transform);
        GetComponent(out _getNavAgentEvadeTarget);
    }
    
    protected override void OnStart() { }

    protected override void OnStop() { }

    protected override bool Condition() => Vector3.Distance(_transform.position, _getNavAgentEvadeTarget.Get().position) <= radius;
}