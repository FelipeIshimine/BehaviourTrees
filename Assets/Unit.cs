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
public class Unit : MonoBehaviour
{
    public Renderer renderer;
    
    public Material normalMaterial;
    public Material monsterMaterial;
    
    public float evadeRadius = 3;
    public float moveRadius = 6;
    public float targetReachedDistanceThreshold = .2f;
    public float angleVariation = 45;
    public float touchDistance = .5f;
    
    private BehaviorTree _tree;
    private NavMeshAgent _navMeshAgent;

    public static Unit Monster { get; private set; }

    public static List<Unit> Victims { get; private set; } = new List<Unit>();
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        _tree = new BehaviorTree();

        var myTransform = transform;
        
        RandomWalkNavMeshNode randomWalkNavMeshNode = new RandomWalkNavMeshNode(_tree, myTransform, _navMeshAgent, moveRadius, targetReachedDistanceThreshold);
        EvadeWalkNavMeshNode evadeWalkNavMeshNode = new EvadeWalkNavMeshNode(_tree, myTransform, GetEnemyTransform, _navMeshAgent, moveRadius, targetReachedDistanceThreshold, angleVariation);

        InsideRadiusNode insideRadiusNode = new InsideRadiusNode(_tree, myTransform, GetEnemyTransform, evadeRadius,
            evadeWalkNavMeshNode, randomWalkNavMeshNode);

        ChaseClosestTransform chaseNode = new ChaseClosestTransform(_tree,GetAgentsTransform, _navMeshAgent,  touchDistance);
        IfNode ifNode = new IfNode(_tree, IsTheMonster, chaseNode, insideRadiusNode);
        
        Repeater repeater = new Repeater(_tree, ifNode);
        
        _tree.OverrideRoot(repeater);

    }

    private void Start()
    {
        if (Victims.Count == 4)
            SetAsMonster();
    }

    private void OnEnable()
    {
        Victims.Add(this);
    }

    private void OnDisable()
    {
        Victims.Remove(this);
    }

    private bool IsTheMonster() => Monster.transform == transform; 
    private Transform GetEnemyTransform() => Monster.transform;
    private List<Unit> GetAgentsTransform() => Victims;
    public void FixedUpdate()
    {
        _tree.Tick();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, evadeRadius);
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, moveRadius);
    }

    public void SetMaterial(bool isEnemy) => renderer.material = isEnemy ? normalMaterial : monsterMaterial;

    void SetNormal()
    {
        Monster = null;
        SetMaterial(false);
        Victims.Add(this);
    }
    
    public void SetAsMonster()
    {
        Victims.Remove(this);
        SetMaterial(true);
        if (Monster)
            Monster.SetNormal();
        Monster = this;
    }
}

public class ChaseClosestTransform : BehaviourNode
{
    private readonly NavMeshAgent _navMeshAgent;
    private Func<List<Unit>> _getAgents;
    private float _touchDistance;
    public ChaseClosestTransform(BehaviorTree behaviorTree, Func<List<Unit>> getAgents, NavMeshAgent navMeshAgent, float touchDistance) : base(behaviorTree)
    {
        _getAgents = getAgents;
        _navMeshAgent = navMeshAgent;
        _touchDistance = touchDistance;
    }

    public override Result Execute()
    {
        Transform getClosest = GetClosest(_getAgents.Invoke());

        if (getClosest == null)
            return Result.Failure;

        if (Vector3.Distance(getClosest.transform.position, _navMeshAgent.transform.position) < _touchDistance)
        {
            getClosest.GetComponent<Unit>().SetAsMonster();
            return Result.Success;
        }

        _navMeshAgent.destination = getClosest.position;
        
        return Result.Running;
    }

    private Transform GetClosest(List<Unit> getAgents)
    {
        Transform transform = _navMeshAgent.transform;
        float sqrDistance = float.MaxValue;
        Transform closest = null;
        foreach (Unit agent in getAgents)
        {
            float currentSqrDistance = (transform.position - agent.transform.position).sqrMagnitude;
            if (currentSqrDistance < sqrDistance)
            {
                sqrDistance = currentSqrDistance;
                closest = agent.transform;
                if (sqrDistance < _touchDistance)
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

    public override Result Execute() => IsInsideRange() ? _insideRadius.Execute() : _outsideRadius.Execute();

    private bool IsInsideRange() => Vector3.Distance(_transform.position, _targetTransform.position) <= _radius;
}

public class IfNode : BehaviourNode
{
    private readonly Func<bool> _condition;
    private readonly BehaviourNode _trueNode, _falseNode;
    
    public IfNode(BehaviorTree behaviorTree, Func<bool> condition, BehaviourNode trueNode, BehaviourNode falseNode) : base(behaviorTree)
    {
        _condition = condition;
        _trueNode = trueNode;
        _falseNode = falseNode;
    }

    public override Result Execute() => _condition.Invoke() ? _trueNode.Execute() : _falseNode.Execute();
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

        Vector2 newDirection = (evadeTargetPosition - myPosition).normalized;

        float angle = AsAngle(newDirection) + Random.Range(-_angleVariation,_angleVariation);

        newDirection = AsVector(angle) * _moveRadius;
        
        if (NavMesh.SamplePosition(newDirection, out NavMeshHit hit, _moveRadius, 1))
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
    
    public override Result Execute()
    {
        Debug.Log("EvadeWalkNavMeshNode");
        if (!TargetReached()) return Result.Running;
        return FindNextDestination() ? Result.Success : Result.Failure;
    }

    private bool TargetReached()
    {
        float distance = Vector3.Distance(_transform.position, _navMeshAgent.destination);
        return distance < _targetReachedThreshold;
    }
}

public class RandomWalkNavMeshNode : BehaviourNode
{
    private readonly Transform _transform;
    private readonly NavMeshAgent _navMeshAgent;
    
    private readonly float _moveRadius;
    private readonly float _targetReachedThreshold;
    
    public RandomWalkNavMeshNode(BehaviorTree behaviorTree, Transform transform, NavMeshAgent navMeshAgent, float moveRadius, float targetReachedThreshold) : base(behaviorTree)
    {
        _transform = transform;
        _navMeshAgent = navMeshAgent;
        _moveRadius = moveRadius;
        _targetReachedThreshold = targetReachedThreshold;
        FindNextDestination();
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

    public override Result Execute()
    {
        Debug.Log("RandomWalkNavMeshNode");
        if (!TargetReached()) return Result.Running;
        return FindNextDestination() ? Result.Success : Result.Failure;
    }

    private bool TargetReached()
    {
        float distance = Vector3.Distance(_transform.position, _navMeshAgent.destination);
        return distance < _targetReachedThreshold;
    }
}
