using TheKiwiCoder;
using UnityEngine;

namespace BehaviourTreeSystem.Runtime.Core {
    public abstract class Node : ScriptableObject {
        public enum State {
            Running,
            Failure,
            Success
        }

        public State state = State.Running;
        public string overrideName = string.Empty;
        [HideInInspector] public bool reached = false;
        [HideInInspector] public bool started = false;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public BehaviourTreeRunner tree;
        [HideInInspector] public Blackboard blackboard;
        [TextArea] public string description;

        public bool drawGizmos = false;

        public void Initialize(BehaviourTreeRunner tree)
        {
            this.tree = tree;
            Initialization();
        }

        protected abstract void Initialization();

        protected T GetComponent<T>() where T : class => tree.GetCacheComponent<T>();
        protected T GetComponent<T>(out T aux) where T : class => aux = tree.GetCacheComponent<T>();
        
        
        public State Execute()
        {

            reached = true;
            if (!started) {
                OnStart();
                started = true;
            }

            tree.NodeExecuted(this);
            state = Execution();

            if (state != State.Running) {
                OnStop();
                started = false;
            }

            return state;
        }

        public virtual Node Clone() {
            return Instantiate(this);
        }

        public void Abort() {
            BehaviourTree.Traverse(this, (node) => {
                node.started = false;
                node.state = State.Running;
                node.OnStop();
            });
        }

        public virtual void OnDrawGizmos() { }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State Execution();

        public virtual string GetName() => GetType().Name;
    }
}