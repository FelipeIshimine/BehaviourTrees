using System.Collections.Generic;

namespace BehaviorTreeSystem
{
    public abstract class CompositeNode : BehaviourNode
    {
        public List<BehaviourNode> Children { get; set; }
        public CompositeNode(BehaviorTree behaviorTree, BehaviourNode[] children) : base(behaviorTree)
        {
            Children = new List<BehaviourNode>(children);
        }
    }
}