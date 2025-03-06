using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ScriptableCareerProgressionGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class CareerEndNode : ScriptableGraphNode
    {
        public const string InputPortName = "Retired";

        private readonly Port inputPort;

        public CareerEndNode() : base()
        {
            title = "End";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the final career progression nodes.
            inputPort = ObservablePort.Create<Edge>(InputPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ScriptableCareerProgression));
            inputContainer.Add(inputPort);
        }
    }
}
