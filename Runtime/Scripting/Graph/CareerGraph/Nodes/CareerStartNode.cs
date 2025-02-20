using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(CareerGraph))]
    [NodePresentOnCreation(x: -300, y: 0)]
    public class CareerStartNode : ScriptableGraphNode
    {
        [NodeOutput(Port.Capacity.Multi)]
        public ScriptableGraphNode Hire;

        public CareerStartNode() : base("Start")
        {

        }
    }
}
