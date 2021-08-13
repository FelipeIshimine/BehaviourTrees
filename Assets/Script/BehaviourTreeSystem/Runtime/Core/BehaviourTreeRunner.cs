using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourTreeSystem.Runtime.Core;
using UnityEngine;

namespace TheKiwiCoder {
    public class BehaviourTreeRunner : MonoBehaviour
    {
        public static event Action<Node> OnNodeExecution;

        private string lastNodeExecuted;
        // The main behaviour tree asset
        public BehaviourTree tree;

        private readonly Dictionary<Type, object> _componentCache = new Dictionary<Type, object>();

        // Start is called before the first frame update
        void Start() 
        {
            tree = tree.Clone();
            tree.Bind(this);
        }

        // Update is called once per frame
        void Update() {
            if (tree) {
                tree.Update();
            }
        }

        private void OnDrawGizmosSelected() {
            if (!tree) {
                return;
            }

            BehaviourTree.Traverse(tree.rootNode, (n) => {
                if (n.drawGizmos) {
                    n.OnDrawGizmos();
                }
            });
        }

        public T GetCacheComponent<T>() where T : class
        {
            if(_componentCache.TryGetValue(typeof(T), out object component)) return component as T;
            component = GetComponent<T>();
            _componentCache.Add(typeof(T),component);
            return component as T;
        }

        public void NodeExecuted(Node node)
        {
            lastNodeExecuted = node.GetName();
            OnNodeExecution?.Invoke(node);
        }
        
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up, $"<color=green>{lastNodeExecuted}</color>", new GUIStyle(){richText = true});
#endif
        }
    }
}