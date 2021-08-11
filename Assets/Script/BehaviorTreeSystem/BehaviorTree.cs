using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTreeSystem
{
    public class BehaviorTree
    {
        private BehaviourNode _root;
        public BehaviourNode Root => _root;

        private bool _startedBehaviour;
        public Dictionary<string, object> Blackboard { get; set; }

        private BehaviourNode.Result _result;

        public BehaviorTree()
        {
            Blackboard = new Dictionary<string, object>();
            _startedBehaviour = false;
        }
        
        public BehaviorTree(Dictionary<string, object> blackboard)
        {
            Blackboard = blackboard;
            _startedBehaviour = false;
        }

        public BehaviorTree(BehaviourNode node, Dictionary<string, object> blackboard) : this(blackboard)
        {
            _root = node;
        }

        public void OverrideRoot(BehaviourNode nRoot)
        {
            _root = nRoot;
        }
        
        public BehaviourNode.Result Tick()
        {
            if (!_startedBehaviour)
            {
                _startedBehaviour = true;
                return _result = Root.Execute();
            }
            
            if(_result == BehaviourNode.Result.Running)
                return _result = Root.Execute();

            Debug.Log($"Behavior has finished with: {_result}");
            return _result;
        }
    
    }
}