using BehaviourTreeSystem.Runtime.Core;
using UnityEngine;

namespace BehaviourTreeSystem.Runtime.Decorators 
{
    public class Timeout : DecoratorNode {
        public float duration = 1.0f;
        float startTime;

        protected override void Initialization()
        {
        }

        protected override void OnStart() {
            startTime = Time.time;
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            if (Time.time - startTime > duration) {
                return State.Failure;
            }

            return child.Execute();
        }
    }
}