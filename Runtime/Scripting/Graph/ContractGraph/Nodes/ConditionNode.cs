using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Condition")]
    [NodeContext(typeof(ContractGraph))]
    public class ConditionNode : ScriptableGraphNode
    {
        [NodeOutput(Port.Capacity.Single)]
        public ConditionNode Satisfied;

        [NodeOutput(Port.Capacity.Single)]
        public ConditionNode Dissatisfied;

        public ConditionNode() : base("Condition", new Color(0.3f, 0.3f, 0.6f))
        {
            Satisfied = this;
            Dissatisfied = this;
        }
    }
}
