using Bolt;
using Ludiq;
using UnityEngine;

namespace Ishimine.BoltExtension.BehaviorTrees
{
    [UnitTitle("BehaviorNodeUnit")]
    [UnitCategory("BehaviorTree/Node")]
    public class BehaviorNodeUnit : Unit
    {
        [DoNotSerialize]
        public ControlInput input { get; private set; }
    
        [DoNotSerialize]
        public ControlOutput output { get; private set; }

        public ValueInput objectiveIn { get; private set; }
    
        protected override void Definition()
        {
            input = ControlInput("In", Enter);
            output = ControlOutput("Out");
            objectiveIn = ValueInput<string>("Objective");
            Requirement(objectiveIn, input);
        }

        public ControlOutput Enter(Flow flow)
        {
            Debug.Log(flow.loopIdentifier);
        
            string objective = flow.GetValue<string>(objectiveIn);
            Debug.Log(objective);
            return output;
        }
    }
}
