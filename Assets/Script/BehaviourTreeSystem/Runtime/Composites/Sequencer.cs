using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class Sequencer : CompositeNode {
        protected int current;

        protected override void Initialization()
        {
        }

        protected override void OnStart() {
            current = 0;
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            for (int i = current; i < children.Count; ++i) {
                current = i;
                var child = children[current];

                switch (child.Execute()) {
                    case State.Running:
                        return State.Running;
                    case State.Failure:
                        return State.Failure;
                    case State.Success:
                        continue;
                }
            }

            return State.Success;
        }
    }
}