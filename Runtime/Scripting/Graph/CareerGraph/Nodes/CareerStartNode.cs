using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ScriptableCareerProgressionGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class CareerStartNode : ScriptableGraphNode
    {
        public const string OutputPortName = "Hired";

        private readonly Port outputPort;

        public CareerStartNode() : base()
        {
            title = "Start";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f));

            // Add an output port for the first career progression nodes.
            outputPort = ObservablePort.Create<Edge>(OutputPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            outputContainer.Add(outputPort);
        }
    }
}
