namespace BehaviourTreeSystem.Runtime.Core
{
    public abstract class ConditionalNode : DecoratorNode
    {
        protected abstract bool Condition();

        protected override State Execution() => Condition() ? child.Execute() : State.Failure;
    
    }
}