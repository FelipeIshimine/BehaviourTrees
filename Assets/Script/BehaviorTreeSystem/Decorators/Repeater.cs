namespace BehaviorTreeSystem.Decorators
{
    public class Repeater : Decorator
    {
        public Repeater(BehaviorTree behaviorTree, BehaviourNode child) : base(behaviorTree, child)
        {
        }

        public override Result Execute()
        {
            Child.Execute();
            return Result.Running;
        }
    }
}