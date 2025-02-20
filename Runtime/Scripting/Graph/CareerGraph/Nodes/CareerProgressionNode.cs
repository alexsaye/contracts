using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Contract")]
    public class CareerProgressionNode : ScriptableGraphNode
    {
        [NodeInput("From", Port.Capacity.Multi)]
        public ConditionNode From;

        [NodeInput("Fulfill", Port.Capacity.Single)]
        public ConditionNode Fulfill;

        [NodeInput("Reject", Port.Capacity.Single)]
        public ConditionNode Reject;

        public CareerProgressionNode() : base("Contract")
        {

        }
    }
}
