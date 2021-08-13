using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class Wait : ActionNode 
    {
        public float duration = 1;
        public float startTime;

        protected override void Initialization()
        {
        }

        protected override void OnStart() {
            startTime = Time.time;
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            if (Time.time - startTime > duration) 
                return State.Success;
            return State.Running;
        }
        
        public override string GetName() => Application.isPlaying && started?$"{base.GetName()}:{TimeSpan.FromSeconds(duration- (Time.time - startTime)):g}":$"{base.GetName()}:{TimeSpan.FromSeconds(duration):g}";

    }
}
