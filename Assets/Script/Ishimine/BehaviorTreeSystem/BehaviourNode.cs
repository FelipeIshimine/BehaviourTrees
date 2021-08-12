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

        public Result Execute()
        {
            BehaviorTree.NodeExecuted(this);
            return Execution();
        }

        protected virtual Result Execution()
        {
            return Result.Failure;
        }

        public virtual void Reset()
        {
        }
    }
}