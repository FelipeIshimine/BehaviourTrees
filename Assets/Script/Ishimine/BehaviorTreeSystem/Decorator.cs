namespace BehaviorTreeSystem
{
    public class Decorator : BehaviourNode
    {
        protected BehaviourNode Child { get; set; }
        public Decorator(BehaviorTree behaviorTree, BehaviourNode child) : base(behaviorTree)
        {
            Child = child;
        }
    }
}