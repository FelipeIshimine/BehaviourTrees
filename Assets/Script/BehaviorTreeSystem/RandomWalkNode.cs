using UnityEngine;

namespace BehaviorTreeSystem
{
    public class RandomWalkNode : BehaviourNode
    {
        public const string TransformKey = "Transform";
        public const string CurrentPositionKey = "CurrentPosition";
        public const string WorldBoundsKey = "WorldBounds";
        protected Vector3 NextDestination { get; set; }
        public float speed;
        public float reachDistanceThreshold;
        
        public RandomWalkNode(BehaviorTree behaviorTree) : base(behaviorTree)
        {
            FindNextDestination();
        }

        private bool FindNextDestination()
        {
            object o;
            bool found = BehaviorTree.Blackboard.TryGetValue(WorldBoundsKey, out o);
            if (found)
            {
                NextDestination = new Vector3(
                    Random.Range(0,1),
                    Random.Range(0,1),
                    Random.Range(0,1));
            }
            return found;
        }

        public override Result Execute()
        {
            Vector3 currentPosition = (Vector3)BehaviorTree.Blackboard[CurrentPositionKey];
            Transform transform = (Transform)BehaviorTree.Blackboard[TransformKey];
            if (Vector3.Distance(currentPosition, NextDestination) < reachDistanceThreshold)
            {
                if (!FindNextDestination())
                    return Result.Failure;
                return Result.Success;
            }
            
            //Move
            transform.Translate((NextDestination-currentPosition).normalized * Time.deltaTime * speed);
            return Result.Running;
        }
    }
}