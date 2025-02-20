using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [NodeMenu("Career Progression")]
    [NodeContext(typeof(CareerGraph))]
    public class CareerProgressionNode : ScriptableGraphNode
    {
        [NodeInput(Port.Capacity.Multi)]
        public ScriptableGraphNode From;

        [NodeOutput(Port.Capacity.Single)]
        public ScriptableGraphNode Fulfill;

        [NodeOutput(Port.Capacity.Single)]
        public ScriptableGraphNode Reject;

        public CareerProgressionNode() : base("Career Progression")
        {

        }
    }
}
