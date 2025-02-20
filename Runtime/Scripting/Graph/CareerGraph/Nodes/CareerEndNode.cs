using UnityEditor.Experimental.GraphView;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(CareerGraph))]
    [NodePresentOnCreation(x: 200, y: 0)]
    public class CareerEndNode : ScriptableGraphNode
    {
        [NodeInput(Port.Capacity.Multi)]
        public ScriptableGraphNode Retire;

        public CareerEndNode() : base("End")
        {

        }
    }
}
