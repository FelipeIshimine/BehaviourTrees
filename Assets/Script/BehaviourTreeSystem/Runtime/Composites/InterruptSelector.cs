using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class InterruptSelector : Selector {
        protected override State Execution() {
            int previous = current;
            base.OnStart();
            var status = base.Execution();
            if (previous != current) {
                if (children[previous].state == State.Running) {
                    children[previous].Abort();
                }
            }

            return status;
        }
    }
}