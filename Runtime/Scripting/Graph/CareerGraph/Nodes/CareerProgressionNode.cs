using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Career Progression")]
    [NodeContext(typeof(CareerGraph))]
    public class CareerProgressionNode : ScriptableGraphNode
    {
        [NodeInput("From", Port.Capacity.Multi)]
        public ScriptableGraphNode From;

        [NodeOutput("Fulfill", Port.Capacity.Single)]
        public ScriptableGraphNode Fulfill;

        [NodeOutput("Reject", Port.Capacity.Single)]
        public ScriptableGraphNode Reject;

        public CareerProgressionNode() : base("Career Progression")
        {

        }
    }
}
