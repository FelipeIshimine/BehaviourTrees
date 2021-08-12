using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TheKiwiCoder {
    public class RandomSelector : CompositeNode {
        protected int current;

        protected override void Initialization()
        {
        }

        protected override void OnStart() {
            current = Random.Range(0, children.Count);
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            var child = children[current];
            return child.Execute();
        }
    }
}