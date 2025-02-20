using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ContractGraph))]
    [NodePresentOnCreation]
    public class ContractNode : ScriptableGraphNode
    {
        [NodeInput(Port.Capacity.Single)]
        public ConditionNode Fulfill;

        [NodeInput(Port.Capacity.Single)]
        public ConditionNode Reject;

        public ContractNode() : base("Contract")
        {
        }
    }
}
