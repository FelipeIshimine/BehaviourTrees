using BehaviourTreeSystem.Runtime.Core;

namespace BehaviourTreeSystem.Runtime.Decorators {
    public class Repeat : DecoratorNode {

        public bool restartOnSuccess = true;
        public bool restartOnFailure = false;

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
                    break;
                case State.Failure:
                    if (restartOnFailure) {
                        return State.Running;
                    } else {
                        return State.Failure;
                    }
                case State.Success:
                    if (restartOnSuccess) {
                        return State.Running;
                    } else {
                        return State.Success;
                    }
            }
            return State.Running;
        }
    }

    
}
