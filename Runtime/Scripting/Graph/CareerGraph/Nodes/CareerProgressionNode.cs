using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Contract")]
    public class CareerProgressionNode : ScriptableGraphNode
    {
        [NodeInput("From", Port.Capacity.Multi)]
        public CareerProgressionNode From;

        [NodeOutput("Fulfill", Port.Capacity.Single)]
        public CareerProgressionNode Fulfill;

        [NodeOutput("Reject", Port.Capacity.Single)]
        public CareerProgressionNode Reject;

        public CareerProgressionNode() : base("Contract")
        {

        }
    }
}
