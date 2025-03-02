using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(CareerProgressionGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class CareerEndNode : ScriptableGraphNode
    {
        private readonly Port retirePort;

        public CareerEndNode() : base("End", new Color(0.3f, 0.6f, 0.3f))
        {
            // Add an input port for the final career progression nodes.
            retirePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(CareerProgressionNode));
            retirePort.name = retirePort.portName = "Retire";
            inputContainer.Add(retirePort);
        }
    }
}
