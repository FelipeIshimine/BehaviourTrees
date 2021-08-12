namespace BehaviorTreeSystem.Decorators
{
    public class Repeater : Decorator
    {
        public Repeater(BehaviorTree behaviorTree, BehaviourNode child) : base(behaviorTree, child)
        {
        }

        protected override Result Execution()
        {
            var value = Child.Execute();

            if (value != Result.Running) Child.Reset();
            
            return Result.Running;
        }
    }
}