using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Contract")]
    public class ContractNode : ScriptableGraphNode
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
