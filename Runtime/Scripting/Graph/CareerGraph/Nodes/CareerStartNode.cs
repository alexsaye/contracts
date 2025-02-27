using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(CareerGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class CareerStartNode : ScriptableGraphNode
    {
        [NodeOutput(Port.Capacity.Multi)]
        public bool Hire;

        public CareerStartNode() : base("Start", new Color(0.6f, 0.3f, 0.3f))
        {

        }
    }
}
