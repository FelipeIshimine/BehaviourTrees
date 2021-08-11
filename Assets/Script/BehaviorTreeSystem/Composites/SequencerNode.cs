using System;

namespace BehaviorTreeSystem.Composites
{
    public class SequencerNode : CompositeNode
    {
        private int _currentNode = 0;

        public SequencerNode(BehaviorTree behaviorTree, BehaviourNode[] children) : base(behaviorTree, children)
        {
        }

        public override Result Execute()
        {
            if (_currentNode >= Children.Count) return Result.Success;

            Result result = Children[_currentNode].Execute();
            switch (result)
            {
                case Result.Running: //Todavia estamos procesando el nodo[_current]
                    return Result.Running;
                case Result.Failure: //El nodo _current fallo asi q reseteamos
                    _currentNode = 0;
                    return Result.Failure;
                case Result.Success:
                    _currentNode++;
                    if (_currentNode <
                        Children
                            .Count) //si no superamos al nodo mayor significa q no terminamos la secuencia y no es un success
                        return Result.Running;
                    _currentNode = 0;
                    return Result.Success;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Fallback nodes are used to find and execute the first child that does not fail. A fallback node will return immediately with a status code of success or running when one of its children returns success or running (see Figure I and the pseudocode below). The children are ticked in order of importance, from left to right.
        /// </summary>
        public class Selector : CompositeNode
        {
            public Selector(BehaviorTree behaviorTree, BehaviourNode[] children) : base(behaviorTree, children)
            {
            }

            public override Result Execute()
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var childStatus = Children[i].Execute();
                    if (childStatus != Result.Failure)
                        return childStatus;
                }
                return Result.Failure;
            }
        }
            
        /// <summary>
        /// Simulamos procesamiento en paralelo pero retornamos el 1er exito o fracazo, con prioridad en el exito
        /// </summary>
        public class Parallel : CompositeNode
        {
            public Parallel(BehaviorTree behaviorTree, BehaviourNode[] children) : base(behaviorTree, children)
            {
            }

            public override Result Execute()
            {
                Result[] results = new Result[Children.Count];
                for (int i = 0; i < Children.Count; i++)
                    results[i] = Children[i].Execute();

                foreach (Result result in results)
                    if (result == Result.Success)
                        return result;
                    
                foreach (Result result in results)
                    if (result == Result.Failure)
                        return result;
                    
                return Result.Running;
            }
        }
        }
}