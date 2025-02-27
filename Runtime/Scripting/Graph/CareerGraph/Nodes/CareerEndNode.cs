using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(CareerGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class CareerEndNode : ScriptableGraphNode
    {
        [NodeInput(Port.Capacity.Multi)]
        public bool Retire;

        public CareerEndNode() : base("End", new Color(0.3f, 0.6f, 0.3f))
        {

        }
    }
}
