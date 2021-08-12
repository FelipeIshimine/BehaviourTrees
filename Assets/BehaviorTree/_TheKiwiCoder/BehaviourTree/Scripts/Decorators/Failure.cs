using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class Failure : DecoratorNode {
        protected override void Initialization()
        {
        }

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            var state = child.Execute();
            if (state == State.Success) {
                return State.Failure;
            }
            return state;
        }
    }
}