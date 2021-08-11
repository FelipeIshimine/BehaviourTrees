namespace BehaviorTreeSystem.Generics
{
    public abstract class GenericBehaviorTree<T> : BehaviorTree
    {
        public T Owner { get; private set; }
        public GenericBehaviorTree(T owner)
        {
            Owner = owner;
        }
    }
}