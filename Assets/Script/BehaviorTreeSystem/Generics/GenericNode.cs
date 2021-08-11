namespace BehaviorTreeSystem.Generics
{
    public class GenericNode<T, TB> : BehaviourNode where TB : GenericBehaviorTree<T>
    {
        private readonly TB _tree;
        public T Owner => _tree.Owner;
        public GenericNode(TB tree) : base(tree)
        {
            _tree = tree;
        }
    }
}