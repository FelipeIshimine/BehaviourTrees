using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTreeSystem;
using BehaviorTreeSystem.Composites;
using BehaviorTreeSystem.Decorators;
using BehaviorTreeSystem.Generics;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerAgent : MonoBehaviour, IGetNavAgentTargets, IGetNavAgentEvadeTarget, ISetAsEvadeTarget
{
    public Renderer renderer;
    
    public Material normalMaterial;
    public Material monsterMaterial;
    
    public float evadeRadius = 3;
    public float moveRadius = 6;
    public float targetReachedDistanceThreshold = .2f;
    public float angleVariation = 45;
    public float touchDistance = .5f;
    public float waitTime = 1.5f;

    
    private NavMeshAgent _navMeshAgent;

    public static PlayerAgent Monster { get; private set; }

    public static readonly List<Transform> Victims = new List<Transform>();
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        Debug.Log(Victims.Count);
        if (Monster == null)
            SetAsMonster();
        else
            SetNormal();
       
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, evadeRadius);
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, moveRadius);

    
        
        if (_navMeshAgent)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_navMeshAgent.destination, .2f);
            Gizmos.DrawLine(transform.position, _navMeshAgent.destination);
        }
    }

    public void SetMaterial(bool isEnemy) => renderer.material = isEnemy ? normalMaterial : monsterMaterial;

    public bool IsMonster() => Monster == this;
    void SetNormal()
    {
        if(Monster == this) Monster = null;
        SetMaterial(false);
        Victims.Add(transform);
        name = name.Replace("Monster ", string.Empty);
    }
    
    public void SetAsMonster()
    {
        Victims.Remove(transform);
        SetMaterial(true);
        if (Monster) Monster.SetNormal();
        
        
        Monster = this;
        name = $"Monster {name}";
        _navMeshAgent.destination = transform.position;
        _navMeshAgent.velocity = Vector3.zero;
    }

    public IReadOnlyList<Transform> Get() => Victims;
    Transform IGetNavAgentEvadeTarget.Get() => Monster.transform;

    public void Set(bool value)
    {
        if (value)
            SetAsMonster();
        else 
            SetNormal();
    }
}

public class WaitNode : BehaviourNode
{
    private readonly float _waitTime;
    private float _currentTime = 0;
    
    public WaitNode(BehaviorTree behaviorTree, float waitTime) : base(behaviorTree)
    {
        _waitTime = waitTime;
    }
    
    protected override Result Execution()
    {
        if(_currentTime >= _waitTime)
        {
            _currentTime = 0;
            return Result.Success;
        }

        _currentTime += Time.deltaTime;
        return Result.Running;
    }

    public override void Reset()
    {
        base.Reset();
        _currentTime = 0;
    }
}

public class ChaseInRadiusOrClosest : BehaviourNode
{
    private readonly NavMeshAgent _navMeshAgent;
    private readonly Func<IReadOnlyList<Transform>> _getTransforms;
    private readonly float _touchDistance;
    private readonly Action<Transform> _onTargetReached;
    private readonly float _shortCircuitRadius;
    public ChaseInRadiusOrClosest(BehaviorTree behaviorTree, Func<IReadOnlyList<Transform>> getTransforms, NavMeshAgent navMeshAgent, float touchDistance, float shortCircuitRadius, Action<Transform> onTargetReached) : base(behaviorTree)
    {
        _shortCircuitRadius = shortCircuitRadius * _shortCircuitRadius;
        _onTargetReached = onTargetReached;
        _getTransforms = getTransforms;
        _navMeshAgent = navMeshAgent;
        _touchDistance = touchDistance;
    }

        protected override Result Execution()

    {
        Transform closest = GetClosest(_getTransforms.Invoke());

        if (closest == null)
            return Result.Failure;

        float distance = Vector3.Distance(closest.transform.position, _navMeshAgent.transform.position);
         Debug.Log($"{_navMeshAgent.gameObject.name}:{distance}");
        if (distance < _touchDistance)
        {
            _onTargetReached.Invoke(closest);
            return Result.Success;
        }
        _navMeshAgent.destination = closest.position;
        
        return Result.Running;
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
                if (sqrDistance < _touchDistance || sqrDistance < _shortCircuitRadius)
                    break;
            }
        }
        return closest;
    }
}

public class InsideRadiusNode : BehaviourNode
{
    private readonly Transform _transform;
    private readonly Transform _targetTransform;
    private readonly float _radius;
    private readonly BehaviourNode _insideRadius;
    private readonly BehaviourNode _outsideRadius;
    private readonly Func<Transform> _getTarget;
    private Transform GetTarget() => _targetTransform ? _targetTransform : _getTarget.Invoke();

    private bool _lastValue = false;
    
    public InsideRadiusNode(BehaviorTree behaviorTree, Transform transform, Transform targetTransform, float radius, BehaviourNode insideRadius, BehaviourNode outsideRadius) : base(behaviorTree)
    {
        _transform = transform;
        _targetTransform = targetTransform;
        _radius = radius;
        _insideRadius = insideRadius;
        _outsideRadius = outsideRadius;
    }
    
    public InsideRadiusNode(BehaviorTree behaviorTree, Transform transform, Func<Transform> getEvadeTarget, float radius, BehaviourNode insideRadius, BehaviourNode outsideRadius) : base(behaviorTree)
    {
        _getTarget = getEvadeTarget;
        _transform = transform;
        _targetTransform = null;
        _radius = radius;
        _insideRadius = insideRadius;
        _outsideRadius = outsideRadius;
    }

    protected override Result Execution()
    {
        bool value = IsInsideRange();
        if (_lastValue != value)
        {
            _insideRadius.Reset();
            _outsideRadius.Reset();
            _lastValue = value;
        }
        return value ? _insideRadius.Execute() : _outsideRadius.Execute();
    }
 

    private bool IsInsideRange() => Vector3.Distance(_transform.position, GetTarget().position) <= _radius;
}

public class IfNode : BehaviourNode
{
    private bool _lastValue;
    private readonly Func<bool> _condition;
    private readonly BehaviourNode _trueNode, _falseNode;
    
    public IfNode(BehaviorTree behaviorTree, Func<bool> condition, BehaviourNode trueNode, BehaviourNode falseNode) : base(behaviorTree)
    {
        _condition = condition;
        _trueNode = trueNode;
        _falseNode = falseNode;
    }

    protected override Result Execution()
    {
        bool newValue = _condition.Invoke();
        if (newValue != _lastValue)
        {
            _trueNode.Reset();
            _falseNode.Reset();
            _lastValue = newValue;
        }
        return newValue ? _trueNode.Execute() : _falseNode.Execute();
    }

    public override void Reset()
    {
        base.Reset();
        
        _trueNode.Reset();
        _falseNode.Reset();
    }
}

public class EvadeWalkNavMeshNode : BehaviourNode
{
    private readonly Transform _transform;
    private readonly NavMeshAgent _navMeshAgent;

    private readonly Transform _evadeTarget;
    private readonly Func<Transform> _getEvadeTarget;
    
    private readonly float _moveRadius;
    private readonly float _targetReachedThreshold;
    private readonly float _angleVariation;
    private Transform GetEvadeTarget() => _evadeTarget ? _evadeTarget : _getEvadeTarget.Invoke();

    private bool _started = false;
    public EvadeWalkNavMeshNode(BehaviorTree behaviorTree, Transform transform, Transform evadeTarget, NavMeshAgent navMeshAgent,  float moveRadius, float targetReachedThreshold, float angleVariation)  : base(behaviorTree)
    {
        _targetReachedThreshold = targetReachedThreshold;
        _angleVariation = angleVariation/2;
        _moveRadius = moveRadius;
        _navMeshAgent = navMeshAgent;
        _transform = transform;
        _evadeTarget = evadeTarget;
    }
    
    public EvadeWalkNavMeshNode(BehaviorTree behaviorTree, Transform transform, Func<Transform> getEvadeTarget, NavMeshAgent navMeshAgent, float moveRadius, float targetReachedThreshold, float angleVariation)  : base(behaviorTree)
    {
        _evadeTarget = null;
        _getEvadeTarget = getEvadeTarget;
        _targetReachedThreshold = targetReachedThreshold;
        _angleVariation = angleVariation/2;
        _moveRadius = moveRadius;
        _navMeshAgent = navMeshAgent;
        _transform = transform;
    }
    
    private bool FindNextDestination()
    {
        Transform evadeTarget = GetEvadeTarget();
        
        Vector2 myPosition = new Vector2(_transform.position.x, _transform.position.z);
        Vector2 evadeTargetPosition = new Vector2(evadeTarget.position.x, evadeTarget.position.z);

        Vector2 newDirection = (myPosition - evadeTargetPosition).normalized;

        float angle = AsAngle(newDirection) + Random.Range(-_angleVariation,_angleVariation);

        newDirection = AsVector(angle);

        Vector3 endDirection = new Vector3(newDirection.x, 0, newDirection.y) * _moveRadius;
        
        if (NavMesh.SamplePosition(endDirection, out NavMeshHit hit, _moveRadius, 1))
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
    
    protected override Result Execution()
    {
        if (!_started)
        {
            _started = true;
            FindNextDestination();
        }
        Debug.Log($"{_transform.name}: EvadeWalkNavMeshNode");
        if (!TargetReached()) return Result.Running;
        return FindNextDestination() ? Result.Success : Result.Failure;
    }

    private bool TargetReached()
    {
        float distance = Vector3.Distance(_transform.position, _navMeshAgent.destination);
        return distance < _targetReachedThreshold;
    }

    public override void Reset()
    {
        base.Reset();
        _started = false;
        
        Debug.Log($"{this} Reset()");
    }
}

public class RandomWalkNavMeshNode : BehaviourNode
{
    private readonly Transform _transform;
    private readonly NavMeshAgent _navMeshAgent;
    
    private readonly float _moveRadius;
    private readonly float _targetReachedThreshold;
    private bool _started = false;
    public RandomWalkNavMeshNode(BehaviorTree behaviorTree, Transform transform, NavMeshAgent navMeshAgent, float moveRadius, float targetReachedThreshold) : base(behaviorTree)
    {
        _transform = transform;
        _navMeshAgent = navMeshAgent;
        _moveRadius = moveRadius;
        _targetReachedThreshold = targetReachedThreshold;
    }

    private bool FindNextDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * _moveRadius;
        randomDirection += _transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _moveRadius, 1))
        {
            _navMeshAgent.destination = hit.position;
            return true;
        }
        return false;
    }

        protected override Result Execution()

    {
        if (!_started)
        {
            _started = true;
            FindNextDestination();
        }
        if (!TargetReached()) return Result.Running;
        return FindNextDestination() ? Result.Success : Result.Failure;
    }

    private bool TargetReached()
    {
        float distance = Vector3.Distance(_transform.position, _navMeshAgent.destination);
        return distance < _targetReachedThreshold;
    }

    public override void Reset()
    {
        _started = false;
        base.Reset();
    }
}
