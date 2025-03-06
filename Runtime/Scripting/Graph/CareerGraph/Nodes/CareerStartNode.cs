using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(CareerProgressionGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class CareerStartNode : ScriptableGraphNode
    {
        public const string HiredPortName = "Hired";

        private readonly Port hiredPort;

        public CareerStartNode() : base()
        {
            title = "Start";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f));

            // Add an output port for the first career progression nodes.
            hiredPort = ObservablePort.Create<Edge>(HiredPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            outputContainer.Add(hiredPort);
        }
    }
}
