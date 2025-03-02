using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(CareerProgressionGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class CareerStartNode : ScriptableGraphNode
    {
        private readonly Port hirePort;

        public CareerStartNode() : base()
        {
            title = "Start";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f));

            // Add an output port for the first career progression nodes.
            hirePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(CareerProgressionNode));
            hirePort.name = hirePort.portName = "Hire";
            outputContainer.Add(hirePort);
        }
    }
}
