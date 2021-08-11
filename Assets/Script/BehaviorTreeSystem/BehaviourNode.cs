namespace BehaviorTreeSystem
{
    public abstract class BehaviourNode 
    {
        public enum Result {Running, Failure, Success}
        public BehaviorTree BehaviorTree { get; set; }
    
        public BehaviourNode(BehaviorTree behaviorTree)
        {
            BehaviorTree = behaviorTree;
        }

        public virtual Result Execute()
        {
            return Result.Failure;
        }
    }
}