using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class Log : ActionNode
    {
        public string message;

        protected override void Initialization()
        {
        }

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            Debug.Log($"{message}");
            return State.Success;
        }
    }
}
