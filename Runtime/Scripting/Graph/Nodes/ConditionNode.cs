using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ContractGraph
{
    [NodeMenu("Condition")]
    public class ConditionNode : ContractGraphNode
    {
        [NodeOutput("Satisfied", Port.Capacity.Single)]
        public ConditionNode Satisfied;

        [NodeOutput("Dissatisfied", Port.Capacity.Single)]
        public ConditionNode Dissatisfied;

        public ConditionNode() : base("Condition", new Color(0.3f, 0.3f, 0.6f))
        {
            Satisfied = this;
            Dissatisfied = this;
        }
    }
}
