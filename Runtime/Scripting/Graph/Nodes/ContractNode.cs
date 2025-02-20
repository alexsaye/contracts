using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ContractGraph
{
    [NodeMenu("Contract")]
    public class ContractNode : ContractGraphNode
    {
        [NodeInput("Fulfill", Port.Capacity.Single)]
        public ConditionNode Fulfill;

        [NodeInput("Reject", Port.Capacity.Single)]
        public ConditionNode Reject;

        public ContractNode() : base("Contract")
        {

        }
    }
}
