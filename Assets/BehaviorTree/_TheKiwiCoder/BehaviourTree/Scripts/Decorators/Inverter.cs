using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class Inverter : DecoratorNode {
        protected override void Initialization()
        {
        }

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            switch (child.Execute()) {
                case State.Running:
                    return State.Running;
                case State.Failure:
                    return State.Success;
                case State.Success:
                    return State.Failure;
            }
            return State.Failure;
        }
    }
}